using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

public sealed class SignInServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private const string InitialPassword = "LuuKy@2026";
    private const string NewPassword = "Moi@2026a";

    private readonly AppDatabaseFixture _fixture;
    private readonly IPasswordHasher _hasher = new Pbkdf2PasswordHasher();
    private readonly SignInOptions _options = new() { LockoutMinutes = 15 };
    private readonly List<AppDbContext> _contexts = [];

    public SignInServiceTests(AppDatabaseFixture fixture) => _fixture = fixture;

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

    private SignInService CreateSignInService(AppDbContext db)
    {
        var store = new UserStore(db);
        var auditLog = new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User));
        var failedSignIns = new FailedSignInService(store, _fixture.Clock, _options, auditLog);
        return new SignInService(store, _hasher, _fixture.User, auditLog, _fixture.Clock, failedSignIns);
    }

    private ChangePasswordService CreateChangePasswordService(AppDbContext db)
    {
        var store = new UserStore(db);
        var auditLog = new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User));
        var failedSignIns = new FailedSignInService(store, _fixture.Clock, _options, auditLog);
        return new ChangePasswordService(store, _hasher, _fixture.User, auditLog, failedSignIns);
    }

    private async Task<User> CreateAccountAsync(bool mustChangePassword = false)
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
            PasswordHash = _hasher.Hash(InitialPassword),
            IsActive = true,
            MustChangePassword = mustChangePassword,
        };
        db.User.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private async Task<List<AuditLog>> GetAccountAuditLogsAsync(int userId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.AuditLog
            .Where(n => n.TableName == "User" && n.RecordId == userId)
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    // "UserName" and "MustChangePassword" contain the event names as substrings, so match the JSON field exactly.
    private static bool HasEvent(AuditLog auditLog, string signInEvent) =>
        auditLog.NewValues?.Contains($"\"Event\":\"{signInEvent}\"") == true;

    [SqlServerFact]
    public async Task SignIn_CorrectPassword_SetsSessionAndLogsEvent()
    {
        var account = await CreateAccountAsync();
        await using var db = NewContext();

        var result = await CreateSignInService(db).SignInAsync(account.UserName, InitialPassword);

        result.Succeeded.ShouldBeTrue();
        _fixture.User.UserId.ShouldBe(account.Id);
        var log = await GetAccountAuditLogsAsync(account.Id);
        log.ShouldContain(n => n.Action == AuditAction.SignIn && HasEvent(n, SignInEvent.SignIn));
        await using var check = _fixture.Database.CreateDbContext();
        (await check.User.SingleAsync(u => u.Id == account.Id)).FailedAttemptCount.ShouldBe((byte)0);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task SignIn_StaffAccount_LoadsTheOfficerNameIntoTheSession()
    {
        var account = await CreateAccountAsync();
        var officerName = await GetOfficerNameAsync(account.OfficerId!.Value);
        await using var db = NewContext();

        var result = await CreateSignInService(db).SignInAsync(account.UserName, InitialPassword);

        result.Succeeded.ShouldBeTrue();
        _fixture.User.OfficerId.ShouldBe(account.OfficerId);
        _fixture.User.FullName.ShouldBe(officerName);
        _fixture.User.SignOut();
    }

    private async Task<string> GetOfficerNameAsync(int officerId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.Officer.Where(o => o.Id == officerId).Select(o => o.FullName).SingleAsync();
    }

    [SqlServerFact]
    public async Task SignIn_FiveFailures_LocksAccountAndLogsEvent()
    {
        var account = await CreateAccountAsync();
        await using var db = NewContext();
        var service = CreateSignInService(db);

        SignInResult? lastResult = null;
        for (var attempt = 0; attempt < User.MaxFailedAttempts; attempt++)
            lastResult = await service.SignInAsync(account.UserName, "sai-mat-khau");

        lastResult!.Status.ShouldBe(SignInStatus.AccountLocked);
        await using (var check = _fixture.Database.CreateDbContext())
        {
            var after = await check.User.SingleAsync(u => u.Id == account.Id);
            after.FailedAttemptCount.ShouldBe(User.MaxFailedAttempts);
            after.LockedUntil.ShouldBe(_fixture.Clock.Now.AddMinutes(15));
        }

        var log = await GetAccountAuditLogsAsync(account.Id);
        log.Count(n => n.Action == AuditAction.SignIn && HasEvent(n, SignInEvent.FailedSignIn)).ShouldBe(4);
        log.Count(n => n.Action == AuditAction.SignIn && HasEvent(n, SignInEvent.AccountLocked)).ShouldBe(1);
    }

    [SqlServerFact]
    public async Task SignIn_LockedAccount_RejectsEvenTheCorrectPassword()
    {
        var account = await CreateAccountAsync();
        await using (var lockDb = NewContext())
        {
            var lockUser = await lockDb.User.SingleAsync(u => u.Id == account.Id);
            lockUser.LockedUntil = _fixture.Clock.Now.AddMinutes(10);
            lockUser.FailedAttemptCount = User.MaxFailedAttempts;
            await lockDb.SaveChangesAsync();
        }

        await using var db = NewContext();
        var result = await CreateSignInService(db).SignInAsync(account.UserName, InitialPassword);

        result.Status.ShouldBe(SignInStatus.AccountLocked);
        _fixture.User.UserId.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task SignIn_LockExpired_SignsInAndResetsFailedAttempts()
    {
        var account = await CreateAccountAsync();
        await using (var lockDb = NewContext())
        {
            var lockUser = await lockDb.User.SingleAsync(u => u.Id == account.Id);
            lockUser.LockedUntil = _fixture.Clock.Now.AddMinutes(1);
            lockUser.FailedAttemptCount = User.MaxFailedAttempts;
            await lockDb.SaveChangesAsync();
        }

        _fixture.Clock.Advance(TimeSpan.FromMinutes(16));

        await using var db = NewContext();
        var result = await CreateSignInService(db).SignInAsync(account.UserName, InitialPassword);

        result.Succeeded.ShouldBeTrue();
        await using var check = _fixture.Database.CreateDbContext();
        var after = await check.User.SingleAsync(u => u.Id == account.Id);
        after.FailedAttemptCount.ShouldBe((byte)0);
        after.LockedUntil.ShouldBeNull();
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task SignIn_InactiveAccount_IsRejectedWithTheLockedMessage()
    {
        var account = await CreateAccountAsync();
        await using (var db = NewContext())
        {
            var user = await db.User.SingleAsync(u => u.Id == account.Id);
            user.IsActive = false;
            await db.SaveChangesAsync();
        }

        await using var checkDb = NewContext();
        var result = await CreateSignInService(checkDb).SignInAsync(account.UserName, InitialPassword);

        result.Status.ShouldBe(SignInStatus.AccountInactive);
        result.Message.ShouldBe(SignInService.AccountLockedMessage);
    }

    [SqlServerFact]
    public async Task SignIn_SeededAdmin_MustChangePasswordThenSignsInNormally()
    {
        // The admin that ships with the database is forced to change its password on the first sign-in.
        await using (var db = NewContext())
        {
            var admin = await db.User.SingleAsync(u => u.UserName == "admin");
            admin.MustChangePassword.ShouldBeTrue();

            var result = await CreateSignInService(db).SignInAsync("admin", InitialPassword);
            result.Succeeded.ShouldBeTrue();
            result.MustChangePassword.ShouldBeTrue();
            // The session snapshot carries the administrator role's grants from the one permission query.
            _fixture.User.HasPermission(PermissionCodes.MasterData.Create).ShouldBeTrue();
        }

        await using (var db = NewContext())
            await CreateChangePasswordService(db).ChangePasswordAsync(InitialPassword, NewPassword, NewPassword);

        await using (var db = NewContext())
        {
            var result = await CreateSignInService(db).SignInAsync("admin", NewPassword);
            result.Succeeded.ShouldBeTrue();
            result.MustChangePassword.ShouldBeFalse();
        }

        // Put the seeded admin back so other tests in this class still find it as shipped.
        await using (var db = NewContext())
        {
            var admin = await db.User.SingleAsync(u => u.UserName == "admin");
            admin.PasswordHash = _hasher.Hash(InitialPassword);
            admin.MustChangePassword = true;
            await db.SaveChangesAsync();
        }

        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Reauthenticate_WrongPassword_IncrementsFailedAttempts()
    {
        var account = await CreateAccountAsync();
        await _fixture.SignInAsync(account);
        await using var db = NewContext();

        var result = await CreateSignInService(db).ReauthenticateAsync("sai-mat-khau");

        result.Status.ShouldBe(SignInStatus.InvalidCredentials);
        await using var check = _fixture.Database.CreateDbContext();
        (await check.User.SingleAsync(u => u.Id == account.Id)).FailedAttemptCount.ShouldBe((byte)1);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Reauthenticate_FifthFailure_LocksTheAccount()
    {
        var account = await CreateAccountAsync();
        await using (var setup = NewContext())
        {
            var user = await setup.User.SingleAsync(u => u.Id == account.Id);
            user.FailedAttemptCount = User.MaxFailedAttempts - 1;
            await setup.SaveChangesAsync();
        }

        await _fixture.SignInAsync(account);
        await using var db = NewContext();

        var result = await CreateSignInService(db).ReauthenticateAsync("sai-mat-khau");

        result.Status.ShouldBe(SignInStatus.AccountLocked);
        await using var check = _fixture.Database.CreateDbContext();
        var after = await check.User.SingleAsync(u => u.Id == account.Id);
        after.LockedUntil.ShouldBe(_fixture.Clock.Now.AddMinutes(15));
        var log = await GetAccountAuditLogsAsync(account.Id);
        log.Count(n => n.Action == AuditAction.SignIn && HasEvent(n, SignInEvent.AccountLocked)).ShouldBe(1);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Reauthenticate_CorrectPassword_WritesTheUnlockEvent()
    {
        var account = await CreateAccountAsync();
        await _fixture.SignInAsync(account);
        await using var db = NewContext();

        var result = await CreateSignInService(db).ReauthenticateAsync(InitialPassword);

        result.Succeeded.ShouldBeTrue();
        var log = await GetAccountAuditLogsAsync(account.Id);
        log.Count(n => n.Action == AuditAction.SignIn && HasEvent(n, SignInEvent.UnlockSession)).ShouldBe(1);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task Reauthenticate_InactiveAccount_IsRefused()
    {
        var account = await CreateAccountAsync();
        await _fixture.SignInAsync(account);
        await using (var setup = NewContext())
        {
            var user = await setup.User.SingleAsync(u => u.Id == account.Id);
            user.IsActive = false;
            await setup.SaveChangesAsync();
        }

        await using var db = NewContext();
        var result = await CreateSignInService(db).ReauthenticateAsync(InitialPassword);

        result.Status.ShouldBe(SignInStatus.AccountInactive);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task LockSession_WritesTheLockEventWithTheKind()
    {
        var account = await CreateAccountAsync();
        await _fixture.SignInAsync(account);
        await using var db = NewContext();

        await CreateSignInService(db).LockSessionAsync(SessionLockKind.Manual);

        var log = await GetAccountAuditLogsAsync(account.Id);
        log.Count(n => n.Action == AuditAction.SignIn
            && HasEvent(n, SignInEvent.LockSession)
            && n.NewValues!.Contains($"\"Kind\":\"{SessionLockKind.Manual.ToCode()}\"")).ShouldBe(1);
        _fixture.User.SignOut();
    }

    [SqlServerFact]
    public async Task ChangePassword_NeverWritesThePasswordHashToTheAuditLog()
    {
        var account = await CreateAccountAsync(mustChangePassword: true);
        await using (var db = NewContext())
        {
            var result = await CreateSignInService(db).SignInAsync(account.UserName, InitialPassword);
            result.MustChangePassword.ShouldBeTrue();
        }

        await using (var db = NewContext())
            await CreateChangePasswordService(db).ChangePasswordAsync(InitialPassword, NewPassword, NewPassword);

        var log = await GetAccountAuditLogsAsync(account.Id);
        log.Count(n => HasEvent(n, SignInEvent.PasswordChanged)).ShouldBe(1);
        log.ShouldAllBe(n => !(n.OldValues ?? "").Contains("PBKDF2") && !(n.NewValues ?? "").Contains("PBKDF2"));
        _fixture.User.SignOut();
    }
}
