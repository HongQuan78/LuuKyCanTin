using System.Data;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

public sealed class AccountServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private readonly AppDatabaseFixture _fixture;
    private readonly IPasswordHasher _hasher = new Pbkdf2PasswordHasher();
    private readonly List<AppDbContext> _contexts = [];

    public AccountServiceTests(AppDatabaseFixture fixture) => _fixture = fixture;

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

    private AccountService CreateService(AppDbContext db)
    {
        var userStore = new UserStore(db);
        return new AccountService(
            db,
            userStore,
            new PermissionChecker(db, _fixture.User),
            new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User)),
            _hasher,
            _fixture.Clock,
            _fixture.User,
            new CreateAccountRequestValidator(),
            new LastAdministratorGuard(db, userStore));
    }

    private async Task SignInAsync(string userName)
    {
        await using var db = _fixture.Database.CreateDbContext();
        var user = await db.User.SingleAsync(u => u.UserName == userName);
        _fixture.User.SignIn(user.Id, user.UserName, user.OfficerId, user.UserName);
    }

    private static string NewUserName() => "u" + Guid.NewGuid().ToString("N")[..12];

    private async Task<Officer> CreateOfficerAsync()
    {
        await using var db = NewContext();
        var officer = new Officer(
            "T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            "Cán bộ " + Guid.NewGuid().ToString("N")[..6],
            "Cán bộ",
            isSupervisingOfficer: false);
        db.Officer.Add(officer);
        await db.SaveChangesAsync();
        return officer;
    }

    private async Task<int> GetRoleIdAsync(string roleCode)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.Role.Where(v => v.Code == roleCode).Select(v => v.Id).SingleAsync();
    }

    private async Task<User> ReadUserAsync(int userId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.User.AsNoTracking().SingleAsync(u => u.Id == userId);
    }

    private async Task<List<string>> GetUserRoleCodesAsync(int userId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await (from userRole in db.UserRole
                      join role in db.Role on userRole.RoleId equals role.Id
                      where userRole.UserId == userId
                      select role.Code).ToListAsync();
    }

    private async Task<List<AuditLog>> GetUserAuditLogsAsync(int userId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.AuditLog
            .Where(log => log.TableName == "User" && log.RecordId == userId)
            .OrderBy(log => log.Id)
            .ToListAsync();
    }

    private async Task SetActiveAsync(int userId, bool isActive)
    {
        await using var db = _fixture.Database.CreateDbContext();
        var user = await db.User.SingleAsync(u => u.Id == userId);
        user.IsActive = isActive;
        await db.SaveChangesAsync();
    }

    private async Task<int> CountUsersAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.User.CountAsync();
    }

    private async Task<int> CountAuditLogsAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.AuditLog.CountAsync();
    }

    [SqlServerFact]
    public async Task Create_NewAccount_ForcesChange_SavesRoles_AndWritesCreateAuditWithoutSecrets()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var roleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);
        var userName = NewUserName();

        await using var db = NewContext();
        var result = await CreateService(db).CreateAsync(new CreateAccountRequest(userName, officer.Id, [roleId]));

        result.TemporaryPassword.Length.ShouldBe(TemporaryPassword.Length);
        PasswordPolicy.Validate(result.TemporaryPassword).ShouldBeEmpty();

        var user = await ReadUserAsync(result.UserId);
        user.UserName.ShouldBe(userName);
        user.OfficerId.ShouldBe(officer.Id);
        user.IsActive.ShouldBeTrue();
        user.MustChangePassword.ShouldBeTrue();
        (await GetUserRoleCodesAsync(result.UserId)).ShouldBe([RoleCodes.CustodyOfficer]);

        var logs = await GetUserAuditLogsAsync(result.UserId);
        logs.ShouldContain(log => log.Action == AuditAction.Create);
        logs.ShouldAllBe(log => !(log.OldValues ?? "").Contains("PBKDF2")
            && !(log.NewValues ?? "").Contains("PBKDF2")
            && !(log.OldValues ?? "").Contains(result.TemporaryPassword)
            && !(log.NewValues ?? "").Contains(result.TemporaryPassword));
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Create_DuplicateUserName_IsRejected()
    {
        await SignInAsync("admin");
        var firstOfficer = await CreateOfficerAsync();
        var secondOfficer = await CreateOfficerAsync();
        var roleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);
        var userName = NewUserName();

        await using var db = NewContext();
        var service = CreateService(db);
        await service.CreateAsync(new CreateAccountRequest(userName, firstOfficer.Id, [roleId]));

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => service.CreateAsync(new CreateAccountRequest(userName, secondOfficer.Id, [roleId])));

        error.Message.ShouldBe(AccountService.DuplicateUserNameMessage);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Create_OfficerAlreadyHasActiveAccount_IsRejected()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var roleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);

        await using var db = NewContext();
        var service = CreateService(db);
        await service.CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [roleId]));

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => service.CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [roleId])));

        error.Message.ShouldBe(AccountService.OfficerAlreadyHasActiveAccountMessage);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Create_SecondActiveAccountForTheSameOfficer_IsRejectedByTheFilteredIndex()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var roleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);

        await using (var db = NewContext())
            await CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [roleId]));

        // The service's pre-check is bypassed on purpose: the filtered unique index is the backstop.
        await using var raw = NewContext();
        raw.User.Add(new User
        {
            UserName = NewUserName(),
            OfficerId = officer.Id,
            PasswordHash = "PBKDF2-SHA256$1$abc$def",
            IsActive = true,
        });

        await Should.ThrowAsync<UniqueConstraintException>(() => ((IAppDbContext)raw).SaveChangesAsync());
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task GetAll_ShowsOfficerRolesAndState()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var roleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);
        var userName = NewUserName();

        int userId;
        await using (var db = NewContext())
            userId = (await CreateService(db).CreateAsync(new CreateAccountRequest(userName, officer.Id, [roleId]))).UserId;

        await using (var db = _fixture.Database.CreateDbContext())
        {
            var user = await db.User.SingleAsync(u => u.Id == userId);
            user.LockedUntil = _fixture.Clock.Now.AddMinutes(10);
            await db.SaveChangesAsync();
        }

        await using (var db = NewContext())
        {
            var account = (await CreateService(db).GetAllAsync()).Single(a => a.Id == userId);

            account.UserName.ShouldBe(userName);
            account.OfficerFullName.ShouldBe(officer.FullName);
            account.RoleNames.ShouldBe("Cán bộ theo dõi tiền lưu ký");
            account.RoleIds.ShouldBe([roleId]);
            account.IsActive.ShouldBeTrue();
            account.IsLocked.ShouldBeTrue();
            account.MustChangePassword.ShouldBeTrue();
        }

        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task GetOfficersForAccountCreation_ExcludesThoseWithAnActiveAccount()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var newOfficer = await CreateOfficerAsync();
        var roleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);

        await using var db = NewContext();
        var service = CreateService(db);
        await service.CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [roleId]));

        var officers = await service.GetOfficersForAccountCreationAsync();

        officers.ShouldNotContain(o => o.Id == officer.Id);
        officers.ShouldContain(o => o.Id == newOfficer.Id);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Deactivate_TwoAdministrators_OneCanBeDeactivated()
    {
        await SignInAsync("admin");
        var firstOfficer = await CreateOfficerAsync();
        var secondOfficer = await CreateOfficerAsync();
        var administratorRoleId = await GetRoleIdAsync(RoleCodes.Administrator);
        var firstUserName = NewUserName();
        var secondUserName = NewUserName();

        int firstId;
        int secondId;
        await using (var db = NewContext())
        {
            var service = CreateService(db);
            firstId = (await service.CreateAsync(new CreateAccountRequest(firstUserName, firstOfficer.Id, [administratorRoleId]))).UserId;
            secondId = (await service.CreateAsync(new CreateAccountRequest(secondUserName, secondOfficer.Id, [administratorRoleId]))).UserId;
        }

        // The actor is the first account; the seeded admin plus it stay, so the second may be deactivated.
        await SignInAsync(firstUserName);
        await using (var db = NewContext())
            await CreateService(db).DeactivateAsync(secondId);

        (await ReadUserAsync(secondId)).IsActive.ShouldBeFalse();
        var logs = await GetUserAuditLogsAsync(secondId);
        logs.ShouldContain(log => log.Action == AuditAction.Update
            && log.OldValues!.Contains("IsActive") && log.NewValues!.Contains("IsActive"));

        await SetActiveAsync(firstId, false);
        await SetActiveAsync(secondId, false);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Deactivate_OwnAccount_IsRejected()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var administratorRoleId = await GetRoleIdAsync(RoleCodes.Administrator);
        var userName = NewUserName();

        int userId;
        await using (var db = NewContext())
            userId = (await CreateService(db).CreateAsync(new CreateAccountRequest(userName, officer.Id, [administratorRoleId]))).UserId;

        await SignInAsync(userName);
        await using (var db = NewContext())
        {
            var error = await Should.ThrowAsync<BusinessRuleException>(
                () => CreateService(db).DeactivateAsync(userId));
            error.Message.ShouldBe(AccountService.CannotDeactivateSelfMessage);
        }

        (await ReadUserAsync(userId)).IsActive.ShouldBeTrue();
        await SetActiveAsync(userId, false);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Reactivate_OfficerAlreadyHasANewerAccount_IsRejected()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var roleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);
        var oldUserName = NewUserName();

        int oldUserId;
        await using (var db = NewContext())
            oldUserId = (await CreateService(db).CreateAsync(new CreateAccountRequest(oldUserName, officer.Id, [roleId]))).UserId;
        await SetActiveAsync(oldUserId, false);

        int newUserId;
        await using (var db = NewContext())
            newUserId = (await CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [roleId]))).UserId;

        await using (var db = NewContext())
        {
            var error = await Should.ThrowAsync<BusinessRuleException>(
                () => CreateService(db).ReactivateAsync(oldUserId));
            error.Message.ShouldBe(AccountService.OfficerAlreadyHasActiveAccountMessage);
        }

        (await ReadUserAsync(oldUserId)).IsActive.ShouldBeFalse();
        await SetActiveAsync(newUserId, false);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Unlock_ClearsFailedAttemptsAndLock()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var roleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);

        int userId;
        await using (var db = NewContext())
            userId = (await CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [roleId]))).UserId;

        await using (var db = _fixture.Database.CreateDbContext())
        {
            var user = await db.User.SingleAsync(u => u.Id == userId);
            user.FailedAttemptCount = User.MaxFailedAttempts;
            user.LockedUntil = _fixture.Clock.Now.AddMinutes(15);
            await db.SaveChangesAsync();
        }

        await using (var db = NewContext())
            await CreateService(db).UnlockAsync(userId);

        var after = await ReadUserAsync(userId);
        after.FailedAttemptCount.ShouldBe((byte)0);
        after.LockedUntil.ShouldBeNull();
        var logs = await GetUserAuditLogsAsync(userId);
        logs.ShouldContain(log => log.Action == AuditAction.Update
            && log.OldValues!.Contains("FailedAttemptCount") && log.NewValues!.Contains("FailedAttemptCount"));
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task ResetPassword_NewTemporaryPassword_ForcesChange_ClearsLock_AndStaysOutOfAudit()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var roleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);
        var userName = NewUserName();

        int userId;
        await using (var db = NewContext())
            userId = (await CreateService(db).CreateAsync(new CreateAccountRequest(userName, officer.Id, [roleId]))).UserId;

        // A reset after the first change, with the account locked, is the realistic case.
        await using (var db = _fixture.Database.CreateDbContext())
        {
            var user = await db.User.SingleAsync(u => u.Id == userId);
            user.MustChangePassword = false;
            user.FailedAttemptCount = User.MaxFailedAttempts;
            user.LockedUntil = _fixture.Clock.Now.AddMinutes(15);
            await db.SaveChangesAsync();
        }

        string temporaryPassword;
        await using (var db = NewContext())
            temporaryPassword = await CreateService(db).ResetPasswordAsync(userId);

        PasswordPolicy.Validate(temporaryPassword).ShouldBeEmpty();
        var after = await ReadUserAsync(userId);
        after.MustChangePassword.ShouldBeTrue();
        after.FailedAttemptCount.ShouldBe((byte)0);
        after.LockedUntil.ShouldBeNull();

        var logs = await GetUserAuditLogsAsync(userId);
        logs.ShouldContain(log => log.NewValues != null
            && log.NewValues.Contains($"\"Event\":\"{AccountEvent.ResetPassword}\""));
        logs.ShouldAllBe(log => !(log.OldValues ?? "").Contains("PBKDF2")
            && !(log.NewValues ?? "").Contains("PBKDF2")
            && !(log.OldValues ?? "").Contains(temporaryPassword)
            && !(log.NewValues ?? "").Contains(temporaryPassword));

        // The next sign-in with the temporary password is accepted but must change it.
        _fixture.User.SignOut();
        await using (var db = NewContext())
        {
            var store = new UserStore(db);
            var auditLog = new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User));
            var failedSignIns = new FailedSignInService(
                store, _fixture.Clock, new SignInOptions { LockoutMinutes = 15 }, auditLog);
            var signIn = new SignInService(store, _hasher, _fixture.User, auditLog, _fixture.Clock, failedSignIns);

            var result = await signIn.SignInAsync(userName, temporaryPassword);

            result.Succeeded.ShouldBeTrue();
            result.MustChangePassword.ShouldBeTrue();
        }

        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task UpdateRoles_ReplacesRoles_WritesOneUpdateRowWithTwoSortedLists()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var custodyRoleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);
        var accountantRoleId = await GetRoleIdAsync(RoleCodes.Accountant);

        int userId;
        await using (var db = NewContext())
            userId = (await CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [custodyRoleId]))).UserId;

        await using (var db = NewContext())
            await CreateService(db).UpdateRolesAsync(userId, [accountantRoleId]);

        (await GetUserRoleCodesAsync(userId)).ShouldBe([RoleCodes.Accountant]);
        var logs = await GetUserAuditLogsAsync(userId);
        var update = logs.Single(log => log.Action == AuditAction.Update && log.OldValues?.Contains("\"Roles\"") == true);
        update.OldValues.ShouldBe("""{"Roles":["LUU_KY"]}""");
        update.NewValues.ShouldBe("""{"Roles":["KE_TOAN"]}""");
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task UpdateRoles_EmptyRoleSet_IsRejected()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var custodyRoleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);

        int userId;
        await using (var db = NewContext())
            userId = (await CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [custodyRoleId]))).UserId;

        await using (var db = NewContext())
        {
            var error = await Should.ThrowAsync<BusinessRuleException>(
                () => CreateService(db).UpdateRolesAsync(userId, []));
            error.Message.ShouldBe(AccountService.InvalidRoleMessage);
        }

        (await GetUserRoleCodesAsync(userId)).ShouldBe([RoleCodes.CustodyOfficer]);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task UpdateRoles_GuardedWrite_RunsInASerializableTransaction()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var custodyRoleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);
        var accountantRoleId = await GetRoleIdAsync(RoleCodes.Accountant);

        int userId;
        await using (var db = NewContext())
            userId = (await CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [custodyRoleId]))).UserId;

        await using (var db = NewContext())
        {
            var userStore = new UserStore(db);
            var probe = new TransactionProbeAuditLogWriter(
                db, new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User)));
            var service = new AccountService(
                db, userStore, new PermissionChecker(db, _fixture.User), probe, _hasher,
                _fixture.Clock, _fixture.User, new CreateAccountRequestValidator(), new LastAdministratorGuard(db, userStore));

            await service.UpdateRolesAsync(userId, [accountantRoleId]);

            probe.ObservedIsolationLevel.ShouldBe(IsolationLevel.Serializable);
        }

        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Reactivate_Success_IsActive()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();
        var custodyRoleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);

        int userId;
        await using (var db = NewContext())
            userId = (await CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [custodyRoleId]))).UserId;
        await SetActiveAsync(userId, false);

        await using (var db = NewContext())
            await CreateService(db).ReactivateAsync(userId);

        (await ReadUserAsync(userId)).IsActive.ShouldBeTrue();
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Create_InvalidOfficer_IsRejected()
    {
        await SignInAsync("admin");
        var custodyRoleId = await GetRoleIdAsync(RoleCodes.CustodyOfficer);

        await using var db = NewContext();
        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), int.MaxValue, [custodyRoleId])));

        error.Message.ShouldBe(AccountService.InvalidOfficerMessage);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Create_InvalidRole_IsRejected()
    {
        await SignInAsync("admin");
        var officer = await CreateOfficerAsync();

        await using var db = NewContext();
        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [int.MaxValue])));

        error.Message.ShouldBe(AccountService.InvalidRoleMessage);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task UnlockOrReset_UnknownAccount_IsRejected()
    {
        await SignInAsync("admin");
        await using var db = NewContext();
        var service = CreateService(db);

        var unlockError = await Should.ThrowAsync<BusinessRuleException>(() => service.UnlockAsync(int.MaxValue));
        unlockError.Message.ShouldBe(AccountService.AccountNotFoundMessage);

        var resetError = await Should.ThrowAsync<BusinessRuleException>(() => service.ResetPasswordAsync(int.MaxValue));
        resetError.Message.ShouldBe(AccountService.AccountNotFoundMessage);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task GetOfficersForAccountCreation_ExcludesInactiveOfficers()
    {
        await SignInAsync("admin");
        var activeOfficer = await CreateOfficerAsync();
        var inactiveOfficer = await CreateOfficerAsync();
        await using (var officerDb = _fixture.Database.CreateDbContext())
        {
            var officer = await officerDb.Officer.SingleAsync(o => o.Id == inactiveOfficer.Id);
            officer.IsActive = false;
            await officerDb.SaveChangesAsync();
        }

        await using var db = NewContext();
        var officers = await CreateService(db).GetOfficersForAccountCreationAsync();

        officers.ShouldContain(o => o.Id == activeOfficer.Id);
        officers.ShouldNotContain(o => o.Id == inactiveOfficer.Id);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task UpdateRoles_RemovingTheAdminRoleFromTheOnlyAdministrator_IsRejected()
    {
        // A database of its own: the seeded admin is then the only administrator, deterministically.
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateDbContext();
        var admin = await db.User.SingleAsync(u => u.UserName == "admin");
        _fixture.User.SignIn(admin.Id, admin.UserName, admin.OfficerId, admin.UserName);

        var accountantRoleId = await db.Role.Where(v => v.Code == RoleCodes.Accountant).Select(v => v.Id).SingleAsync();
        var service = CreateService(db);

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => service.UpdateRolesAsync(admin.Id, [accountantRoleId]));

        error.Message.ShouldBe(LastAdministratorGuard.LastAdministratorRequiredMessage);
        var roleCodesAfter = await (from userRole in db.UserRole
                                    join role in db.Role on userRole.RoleId equals role.Id
                                    where userRole.UserId == admin.Id
                                    select role.Code).ToListAsync();
        roleCodesAfter.ShouldBe([RoleCodes.Administrator]);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Guard_DeactivatingTheOnlyAdministrator_IsRejected()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateDbContext();
        var adminId = await db.User.Where(u => u.UserName == "admin").Select(u => u.Id).SingleAsync();

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => new LastAdministratorGuard(db, new UserStore(db)).EnsureDeactivationAllowedAsync(adminId));

        error.Message.ShouldBe(LastAdministratorGuard.LastAdministratorRequiredMessage);
    }

    [SqlServerFact]
    public async Task RoleService_RemovingAdminPermissionFromTheOnlyAdministratorRole_IsRejected()
    {
        // Story 2.3's role editor must refuse removing HT.Sua from the last administrator role.
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateDbContext();
        var admin = await db.User.SingleAsync(u => u.UserName == "admin");
        _fixture.User.SignIn(admin.Id, admin.UserName, admin.OfficerId, admin.UserName);

        var guard = new LastAdministratorGuard(db, new UserStore(db));
        var auditLog = new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User));
        var roleService = new RoleService(db, new PermissionChecker(db, _fixture.User), auditLog, guard);
        var role = (await roleService.GetAllAsync()).Single(v => v.Code == RoleCodes.Administrator);

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => roleService.UpdatePermissionsAsync(role.Id, [PermissionCodes.Administration.View], role.RowVer));

        error.Message.ShouldBe(LastAdministratorGuard.LastAdministratorRequiredMessage);
        (await roleService.GetPermissionsAsync(role.Id)).ShouldContain(PermissionCodes.Administration.Update);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Create_WithoutPermission_WritesNothing()
    {
        // The leadership role has HT.Xem but not HT.Sua.
        var officer = await CreateOfficerAsync();
        var leaderRoleId = await GetRoleIdAsync(RoleCodes.Leader);
        var userName = NewUserName();

        int userId;
        await using (var db = NewContext())
        {
            var user = new User
            {
                UserName = userName,
                OfficerId = officer.Id,
                PasswordHash = "PBKDF2-SHA256$1$abc$def",
                IsActive = true,
            };
            db.User.Add(user);
            await db.SaveChangesAsync();
            userId = user.Id;
            db.UserRole.Add(new UserRole { UserId = userId, RoleId = leaderRoleId });
            await db.SaveChangesAsync();
        }

        _fixture.User.SignIn(userId, userName, officer.Id, officer.FullName);
        var usersBefore = await CountUsersAsync();
        var auditLogsBefore = await CountAuditLogsAsync();

        await using (var db = NewContext())
        {
            await Should.ThrowAsync<PermissionDeniedException>(
                () => CreateService(db).CreateAsync(new CreateAccountRequest(NewUserName(), officer.Id, [leaderRoleId])));
        }

        (await CountUsersAsync()).ShouldBe(usersBefore);
        (await CountAuditLogsAsync()).ShouldBe(auditLogsBefore);
        _fixture.User.SignOut();
    }

    /// <summary>An audit writer that records the isolation level of the transaction it is called inside.</summary>
    private sealed class TransactionProbeAuditLogWriter(AppDbContext db, IAuditLogWriter inner) : IAuditLogWriter
    {
        public IsolationLevel? ObservedIsolationLevel { get; private set; }

        public Task WriteAsync(
            AuditAction action, string? tableName, long? recordId, object? newValues = null, CancellationToken ct = default)
        {
            Capture();
            return inner.WriteAsync(action, tableName, recordId, newValues, ct);
        }

        public Task WriteAsync(
            AuditAction action, string? tableName, long? recordId, object? oldValues, object? newValues, CancellationToken ct = default)
        {
            Capture();
            return inner.WriteAsync(action, tableName, recordId, oldValues, newValues, ct);
        }

        private void Capture() =>
            ObservedIsolationLevel = db.Database.CurrentTransaction?.GetDbTransaction().IsolationLevel;
    }
}
