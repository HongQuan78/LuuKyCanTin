using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

/// <summary>
/// AC 3 and AC 4: hiding a menu entry is never the protection. A caller that skips the UI is refused by the
/// service, and a permission revoked after sign-in stops the very next write even though the session cache
/// still says yes.
/// </summary>
public sealed class PermissionBypassTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private readonly AppDatabaseFixture _fixture;
    private readonly List<AppDbContext> _contexts = [];

    public PermissionBypassTests(AppDatabaseFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Runs after every test, pass or fail, so a signed-in session never leaks into another test.
        _fixture.User.SignOut();
        foreach (var context in _contexts)
            await context.DisposeAsync();
    }

    private AppDbContext NewContext()
    {
        var db = _fixture.CreateAuditedContext();
        _contexts.Add(db);
        return db;
    }

    private static string NewCode() => "T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

    private static SaveOfficerRequest OfficerRequest() => new(NewCode(), "Nguyễn Văn Bị Chặn", "Cán bộ", false);

    private async Task<User> CreateUserAsync(int? roleId)
    {
        await using var db = NewContext();
        var officer = new Officer(NewCode(), "Cán bộ " + Guid.NewGuid().ToString("N")[..6], "Cán bộ", false);
        db.Officer.Add(officer);
        await db.SaveChangesAsync();

        var user = new User
        {
            UserName = "u" + Guid.NewGuid().ToString("N")[..12],
            OfficerId = officer.Id,
            PasswordHash = "PBKDF2-SHA256$1$abc$def",
            IsActive = true,
        };
        db.User.Add(user);
        await db.SaveChangesAsync();

        if (roleId is not null)
        {
            db.UserRole.Add(new UserRole { UserId = user.Id, RoleId = roleId.Value });
            await db.SaveChangesAsync();
        }

        return user;
    }

    /// <summary>A test-only role granting exactly the given codes, so revoking it affects nobody else.</summary>
    private async Task<int> CreateRoleAsync(IReadOnlyCollection<string> permissionCodes)
    {
        await using var db = NewContext();
        var role = new Role { Code = "R" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(), Name = "Vai trò thử" };
        db.Role.Add(role);
        await db.SaveChangesAsync();

        var permissionIds = await db.Permission
            .Where(permission => permissionCodes.Contains(permission.Code))
            .Select(permission => permission.Id)
            .ToListAsync();
        foreach (var permissionId in permissionIds)
            db.RolePermission.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
        await db.SaveChangesAsync();

        return role.Id;
    }

    private async Task<(int Officer, int User, int RolePermission, int AuditLog)> CountsAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        return (
            await db.Officer.CountAsync(),
            await db.User.CountAsync(),
            await db.RolePermission.CountAsync(),
            await db.AuditLog.CountAsync());
    }

    private OfficerService CreateOfficerService(AppDbContext db) =>
        new(db, new PermissionChecker(db, _fixture.User), new SaveOfficerRequestValidator());

    private AccountService CreateAccountService(AppDbContext db)
    {
        var userStore = new UserStore(db);
        return new AccountService(
            db,
            userStore,
            new PermissionChecker(db, _fixture.User),
            new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User)),
            new Pbkdf2PasswordHasher(),
            _fixture.Clock,
            _fixture.User,
            new CreateAccountRequestValidator(),
            new LastAdministratorGuard(db, userStore));
    }

    private RoleService CreateRoleService(AppDbContext db)
    {
        var userStore = new UserStore(db);
        return new RoleService(
            db,
            new PermissionChecker(db, _fixture.User),
            new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User)),
            new LastAdministratorGuard(db, userStore));
    }

    [SqlServerFact]
    public async Task WriteServices_ViewOnlyUser_AreRefusedAndWriteNothing()
    {
        // Cán bộ quản giáo holds LK-BC.Xem only: enough to sign in and see a screen, no write anywhere.
        var viewOnlyRoleId = await GetRoleIdAsync(RoleCodes.SupervisingOfficer);
        var user = await CreateUserAsync(viewOnlyRoleId);
        await _fixture.SignInAsync(user);

        var before = await CountsAsync();

        await using (var db = NewContext())
        {
            await Should.ThrowAsync<PermissionDeniedException>(() => CreateOfficerService(db).AddAsync(OfficerRequest()));
        }

        await using (var db = NewContext())
        {
            await Should.ThrowAsync<PermissionDeniedException>(
                () => CreateAccountService(db).CreateAsync(new CreateAccountRequest("u" + Guid.NewGuid().ToString("N")[..8], 1, [1])));
        }

        await using (var db = NewContext())
        {
            var role = (await CreateRoleService(db).GetAllAsync()).Single(r => r.Code == RoleCodes.SupervisingOfficer);
            await Should.ThrowAsync<PermissionDeniedException>(
                () => CreateRoleService(db).UpdatePermissionsAsync(role.Id, [PermissionCodes.CustodyReporting.Print], role.RowVer));
        }

        (await CountsAsync()).ShouldBe(before);
    }

    [SqlServerFact]
    public async Task PermissionRevokedAfterSignIn_StopsTheNextWriteEvenWhenTheCacheSaysYes()
    {
        var roleId = await CreateRoleAsync([PermissionCodes.MasterData.Create]);
        var user = await CreateUserAsync(roleId);
        await _fixture.SignInAsync(user);
        _fixture.User.HasPermission(PermissionCodes.MasterData.Create).ShouldBeTrue();

        // The first add passes: the database still grants DM.Them.
        await using (var db = NewContext())
            (await CreateOfficerService(db).AddAsync(OfficerRequest())).Id.ShouldBeGreaterThan(0);

        // The administrator removes DM.Them from the role in another scope.
        await using (var db = NewContext())
            await db.RolePermission.Where(rolePermission => rolePermission.RoleId == roleId).ExecuteDeleteAsync();

        var officerCount = await CountOfficersAsync();
        // The session snapshot is stale on purpose: the UI would still show the button.
        _fixture.User.HasPermission(PermissionCodes.MasterData.Create).ShouldBeTrue();

        await using (var db = NewContext())
        {
            await Should.ThrowAsync<PermissionDeniedException>(() => CreateOfficerService(db).AddAsync(OfficerRequest()));
        }

        (await CountOfficersAsync()).ShouldBe(officerCount);
    }

    private async Task<int> CountOfficersAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.Officer.CountAsync();
    }

    private async Task<int> GetRoleIdAsync(string roleCode)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.Role.Where(role => role.Code == roleCode).Select(role => role.Id).SingleAsync();
    }
}
