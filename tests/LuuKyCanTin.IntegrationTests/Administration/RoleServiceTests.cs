using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

public sealed class RoleServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private readonly AppDatabaseFixture _fixture;
    private readonly List<AppDbContext> _contexts = [];

    public RoleServiceTests(AppDatabaseFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var context in _contexts)
            await context.DisposeAsync();
    }

    private AppDbContext NewContext()
    {
        var db = _fixture.CreateAuditedContext();
        _contexts.Add(db);
        return db;
    }

    private RoleService CreateService(AppDbContext db)
    {
        var userStore = new UserStore(db);
        var auditLog = new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User));
        return new RoleService(
            db,
            new PermissionChecker(db, _fixture.User),
            auditLog,
            new LastAdministratorGuard(db, userStore));
    }

    private IPermissionChecker CreatePermissionChecker(AppDbContext db) => new PermissionChecker(db, _fixture.User);

    private async Task SignInUserAsync(string userName)
    {
        await using var db = _fixture.Database.CreateDbContext();
        var user = await db.User.SingleAsync(u => u.UserName == userName);
        _fixture.User.SignIn(user.Id, user.UserName, user.OfficerId, user.UserName);
    }

    private async Task<User> CreateUserAsync(string? roleCode)
    {
        await using var db = NewContext();
        // Outside the built-in admin, every account must belong to a staff member (CK_User_OfficerId).
        var officer = new Officer(
            "T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            "Cán bộ " + Guid.NewGuid().ToString("N")[..6],
            "Cán bộ",
            isSupervisingOfficer: false);
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

        if (roleCode is not null)
        {
            var roleId = await db.Role.Where(v => v.Code == roleCode).Select(v => v.Id).SingleAsync();
            db.UserRole.Add(new UserRole { UserId = user.Id, RoleId = roleId });
            await db.SaveChangesAsync();
        }

        return user;
    }

    private async Task<IReadOnlyList<string>> GetRolePermissionsAsync(string roleCode)
    {
        await using var db = NewContext();
        var roleId = await db.Role.Where(v => v.Code == roleCode).Select(v => v.Id).SingleAsync();
        return await CreateService(db).GetPermissionsAsync(roleId);
    }

    private async Task<List<AuditLog>> GetRoleAuditLogsAsync(int roleId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.AuditLog
            .Where(n => n.TableName == "Role" && n.RecordId == roleId)
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    [SqlServerFact]
    public async Task Permission_Seed_MatchesTheCatalogueExactly()
    {
        await using var db = _fixture.Database.CreateDbContext();
        var inDatabase = await db.Permission.AsNoTracking().OrderBy(q => q.Id).ToListAsync();

        inDatabase.Count.ShouldBe(PermissionCodes.All.Count);
        inDatabase.Select(q => (q.Id, q.Code, q.Name, q.Module))
            .ShouldBe(PermissionCodes.All.Select(q => (q.Id, q.Code, q.Name, q.Module)));
    }

    [SqlServerFact]
    public async Task Role_Seed_HasTheSixStandardRolesWithTheirCodes()
    {
        await using var db = _fixture.Database.CreateDbContext();
        var code = await db.Role.AsNoTracking().OrderBy(v => v.Id).Select(v => v.Code).ToListAsync();

        code.ShouldBe(
        [
            RoleCodes.Administrator,
            RoleCodes.CustodyOfficer,
            RoleCodes.CanteenOfficer,
            RoleCodes.SupervisingOfficer,
            RoleCodes.Leader,
            RoleCodes.Accountant,
        ]);
    }

    [SqlServerFact]
    public async Task RolePermission_Seed_UsesTheDefaultPermissions()
    {
        var administrator = await GetRolePermissionsAsync(RoleCodes.Administrator);
        administrator.Count.ShouldBe(12);
        administrator.ShouldContain(PermissionCodes.Administration.Update);
        administrator.ShouldContain(PermissionCodes.MasterData.Create);
        administrator.ShouldNotContain(PermissionCodes.CustodyIncrease.View);

        var custody = await GetRolePermissionsAsync(RoleCodes.CustodyOfficer);
        custody.Count.ShouldBe(16);
        custody.ShouldContain(PermissionCodes.MasterData.View);
        custody.ShouldNotContain(PermissionCodes.CustodyIncrease.Approve);
        custody.ShouldNotContain(PermissionCodes.CustodyDecrease.Approve);
        custody.ShouldNotContain(PermissionCodes.CustodyReporting.Approve);

        var canteen = await GetRolePermissionsAsync(RoleCodes.CanteenOfficer);
        canteen.Count.ShouldBe(19);
        canteen.ShouldContain(PermissionCodes.GoodsReceipt.Approve);
        canteen.ShouldContain(PermissionCodes.Sales.Print);
        canteen.ShouldContain(PermissionCodes.CustodyReporting.View);

        (await GetRolePermissionsAsync(RoleCodes.SupervisingOfficer)).ShouldBe([PermissionCodes.CustodyReporting.View]);

        var leader = await GetRolePermissionsAsync(RoleCodes.Leader);
        leader.Count.ShouldBe(6);
        leader.ShouldBe(
        [
            PermissionCodes.Administration.View,
            PermissionCodes.CustodyIncrease.Approve,
            PermissionCodes.CustodyDecrease.Approve,
            PermissionCodes.GoodsReceipt.Approve,
            PermissionCodes.CustodyReporting.View,
            PermissionCodes.InventoryReporting.View,
        ], ignoreOrder: true);

        (await GetRolePermissionsAsync(RoleCodes.Accountant)).ShouldBe([PermissionCodes.CustodyReporting.View, PermissionCodes.InventoryReporting.View]);
    }

    [SqlServerFact]
    public async Task UserRole_AdminIsAssignedTheAdministratorRole()
    {
        await using var db = _fixture.Database.CreateDbContext();
        var adminId = await db.User.Where(u => u.UserName == "admin").Select(u => u.Id).SingleAsync();
        var roleCode = await (from userRole in db.UserRole
                              join role in db.Role on userRole.RoleId equals role.Id
                              where userRole.UserId == adminId
                              select role.Code).ToListAsync();

        roleCode.ShouldBe([RoleCodes.Administrator]);
    }

    [SqlServerFact]
    public async Task UpdatePermissions_SavesTheChangeAndOneUpdateRowWithBothSortedLists()
    {
        await SignInUserAsync("admin");
        await using var db = NewContext();
        var service = CreateService(db);
        var role = (await service.GetAllAsync()).Single(v => v.Code == RoleCodes.SupervisingOfficer);

        await service.UpdatePermissionsAsync(role.Id, [PermissionCodes.CustodyReporting.Print, PermissionCodes.CustodyReporting.View], role.RowVer);

        (await service.GetPermissionsAsync(role.Id)).ShouldBe([PermissionCodes.CustodyReporting.Print, PermissionCodes.CustodyReporting.View], ignoreOrder: true);
        var log = await GetRoleAuditLogsAsync(role.Id);
        log.Count.ShouldBe(1);
        log[0].Action.ShouldBe(AuditAction.Update);
        log[0].OldValues.ShouldBe("""{"Permissions":["LK-BC.Xem"]}""");
        log[0].NewValues.ShouldBe("""{"Permissions":["LK-BC.In","LK-BC.Xem"]}""");

        // Put the seeded grant back for the other tests in this class.
        var after = (await service.GetAllAsync()).Single(v => v.Code == RoleCodes.SupervisingOfficer);
        await service.UpdatePermissionsAsync(after.Id, [PermissionCodes.CustodyReporting.View], after.RowVer);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task UpdatePermissions_StaleRowVer_Conflicts()
    {
        await SignInUserAsync("admin");
        await using var db = NewContext();
        var service = CreateService(db);
        var role = (await service.GetAllAsync()).Single(v => v.Code == RoleCodes.CanteenOfficer);
        var staleRowVer = role.RowVer;

        // A first save touches the role; the row version the caller still holds is now stale.
        await service.UpdatePermissionsAsync(role.Id, await GetRolePermissionsAsync(RoleCodes.CanteenOfficer), staleRowVer);

        await Should.ThrowAsync<ConcurrencyConflictException>(
            () => service.UpdatePermissionsAsync(role.Id, [PermissionCodes.CustodyReporting.Print, PermissionCodes.CustodyReporting.View], staleRowVer));

        // The stale call changed nothing.
        (await GetRolePermissionsAsync(RoleCodes.CanteenOfficer)).ShouldNotContain(PermissionCodes.CustodyReporting.Print);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task UpdatePermissions_WithoutPermission_WritesNothing()
    {
        var user = await CreateUserAsync(roleCode: null);
        _fixture.User.SignIn(user.Id, user.UserName, user.OfficerId, user.UserName);

        await using var db = NewContext();
        var service = CreateService(db);
        var role = (await service.GetAllAsync()).Single(v => v.Code == RoleCodes.Accountant);
        var logsBefore = await GetRoleAuditLogsAsync(role.Id);

        await Should.ThrowAsync<PermissionDeniedException>(
            () => service.UpdatePermissionsAsync(role.Id, [PermissionCodes.Administration.Update], role.RowVer));

        (await service.GetPermissionsAsync(role.Id)).ShouldBe([PermissionCodes.CustodyReporting.View, PermissionCodes.InventoryReporting.View]);
        (await GetRoleAuditLogsAsync(role.Id)).Count.ShouldBe(logsBefore.Count);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task UpdatePermissions_CodeNotInCatalogue_IsRejected()
    {
        await SignInUserAsync("admin");
        await using var db = NewContext();
        var service = CreateService(db);
        var role = (await service.GetAllAsync()).Single(v => v.Code == RoleCodes.Accountant);

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => service.UpdatePermissionsAsync(role.Id, ["HT.KhongCoThat"], role.RowVer));

        error.Message.ShouldContain("HT.KhongCoThat");
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Require_AdminHasPermission_Allows()
    {
        await SignInUserAsync("admin");
        await using var db = NewContext();

        await Should.NotThrowAsync(() => CreatePermissionChecker(db).RequireAsync(PermissionCodes.Administration.Update));
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Require_WithoutRole_IsDenied()
    {
        var user = await CreateUserAsync(roleCode: null);
        _fixture.User.SignIn(user.Id, user.UserName, user.OfficerId, user.UserName);
        await using var db = NewContext();

        await Should.ThrowAsync<PermissionDeniedException>(() => CreatePermissionChecker(db).RequireAsync(PermissionCodes.Administration.Update));
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Require_InactiveAccount_IsDenied()
    {
        var user = await CreateUserAsync(RoleCodes.Administrator);
        await using (var db = NewContext())
        {
            var tracked = await db.User.SingleAsync(u => u.Id == user.Id);
            tracked.IsActive = false;
            await db.SaveChangesAsync();
        }

        _fixture.User.SignIn(user.Id, user.UserName, user.OfficerId, user.UserName);
        await using var checkDb = NewContext();

        await Should.ThrowAsync<PermissionDeniedException>(() => CreatePermissionChecker(checkDb).RequireAsync(PermissionCodes.Administration.Update));
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Require_RoleRevoked_LosesPermissionImmediately()
    {
        var user = await CreateUserAsync(RoleCodes.Administrator);
        _fixture.User.SignIn(user.Id, user.UserName, user.OfficerId, user.UserName);
        await using (var db = NewContext())
            await Should.NotThrowAsync(() => CreatePermissionChecker(db).RequireAsync(PermissionCodes.Administration.Update));

        // The check reads the database every time: removing the role takes effect at the next call.
        await using (var db = NewContext())
            await db.UserRole.Where(v => v.UserId == user.Id).ExecuteDeleteAsync();

        await using (var db = NewContext())
            await Should.ThrowAsync<PermissionDeniedException>(() => CreatePermissionChecker(db).RequireAsync(PermissionCodes.Administration.Update));

        _fixture.User.SignOut();
    }
}
