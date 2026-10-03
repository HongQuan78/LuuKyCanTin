using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.Administration;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

public class SignInServiceTests
{
    private const string ValidHash = "PBKDF2-SHA256$1$abc$def";
    private static readonly DateTime Now = new(2026, 10, 2, 9, 0, 0);

    private readonly IUserStore _store = Substitute.For<IUserStore>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserSession _session = Substitute.For<ICurrentUserSession>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();
    private readonly FakeClock _clock = new(Now);
    private readonly SignInOptions _options = new() { LockoutMinutes = 15 };
    private readonly SignInService _service;
    private User _user = null!;

    public SignInServiceTests()
    {
        var failedSignIns = new FailedSignInService(_store, _clock, _options, _auditLog);
        _service = new SignInService(_store, _hasher, _session, _auditLog, _clock, failedSignIns);
    }

    private void SeedAccount(byte failedAttemptCount = 0, DateTime? lockedUntil = null, bool mustChangePassword = false)
    {
        _user = new User
        {
            Id = 3,
            UserName = "admin",
            PasswordHash = ValidHash,
            IsActive = true,
            MustChangePassword = mustChangePassword,
            FailedAttemptCount = failedAttemptCount,
            LockedUntil = lockedUntil,
        };
        _store.FindByUserNameAsync("admin", Arg.Any<CancellationToken>()).Returns(_user);
        _store.SaveAsync(_user, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CorrectPassword_ResetsTheCountersSetsTheSessionAndLogs()
    {
        SeedAccount(failedAttemptCount: 3, lockedUntil: Now.AddMinutes(-1));
        _hasher.Verify("LuuKy@2026", ValidHash).Returns(true);

        var result = await _service.SignInAsync(" admin ", "LuuKy@2026");

        result.Succeeded.ShouldBeTrue();
        result.MustChangePassword.ShouldBeFalse();
        _user.FailedAttemptCount.ShouldBe((byte)0);
        _user.LockedUntil.ShouldBeNull();
        await _store.Received(1).SaveAsync(_user, Arg.Any<CancellationToken>());
        _session.Received(1).SignIn(3, "admin");
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 3, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CorrectPasswordWithForcedChange_IsSuccessfulButAsksForANewPassword()
    {
        SeedAccount(mustChangePassword: true);
        _hasher.Verify("LuuKy@2026", ValidHash).Returns(true);

        var result = await _service.SignInAsync("admin", "LuuKy@2026");

        result.Succeeded.ShouldBeTrue();
        result.MustChangePassword.ShouldBeTrue();
        _session.Received(1).SignIn(3, "admin");
    }

    [Fact]
    public async Task WrongPassword_CountsTheAttemptAndWritesTheEvent()
    {
        SeedAccount();
        _hasher.Verify("sai", ValidHash).Returns(false);

        var result = await _service.SignInAsync("admin", "sai");

        result.Status.ShouldBe(SignInStatus.InvalidCredentials);
        result.Message.ShouldBe(SignInService.InvalidCredentialsMessage);
        _user.FailedAttemptCount.ShouldBe((byte)1);
        await _store.Received(1).SaveAsync(_user, Arg.Any<CancellationToken>());
        _session.DidNotReceive().SignIn(Arg.Any<int>(), Arg.Any<string>());
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 3,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"Event = {SignInEvent.FailedSignIn}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FifthWrongPassword_LocksTheAccountAndSaysSo()
    {
        SeedAccount(failedAttemptCount: User.MaxFailedAttempts - 1);
        _hasher.Verify("sai", ValidHash).Returns(false);

        var result = await _service.SignInAsync("admin", "sai");

        result.Status.ShouldBe(SignInStatus.AccountLocked);
        result.Message.ShouldBe(SignInService.AccountLockedMessage);
        _user.LockedUntil.ShouldBe(Now.AddMinutes(15));
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 3,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"Event = {SignInEvent.AccountLocked}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConcurrencyConflict_ReloadsAndReappliesTheFailedAttemptOnce()
    {
        SeedAccount();
        _hasher.Verify("sai", ValidHash).Returns(false);
        var saveCount = 0;
        _store.SaveAsync(_user, Arg.Any<CancellationToken>()).Returns(_ =>
        {
            saveCount++;
            if (saveCount == 1)
                throw new ConcurrencyConflictException(new Exception("row version"));
            return Task.CompletedTask;
        });
        // The store's reload would restore what the other workstation wrote; emulate it.
        _store.ReloadAsync(_user, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                _user.FailedAttemptCount = 0;
                _user.LockedUntil = null;
                return Task.CompletedTask;
            });

        var result = await _service.SignInAsync("admin", "sai");

        result.Status.ShouldBe(SignInStatus.InvalidCredentials);
        _user.FailedAttemptCount.ShouldBe((byte)1);
        await _store.Received(1).ReloadAsync(_user, Arg.Any<CancellationToken>());
        await _store.Received(2).SaveAsync(_user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ALockedAccount_IsRefusedWithoutCheckingThePassword()
    {
        SeedAccount(lockedUntil: Now.AddMinutes(5));

        var result = await _service.SignInAsync("admin", "LuuKy@2026");

        result.Status.ShouldBe(SignInStatus.AccountLocked);
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        await _store.DidNotReceive().SaveAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnInactiveAccount_IsRefusedWithoutCheckingThePassword_AndUsesTheLockedMessage()
    {
        SeedAccount();
        _user.IsActive = false;

        var result = await _service.SignInAsync("admin", "LuuKy@2026");

        result.Status.ShouldBe(SignInStatus.AccountInactive);
        result.Message.ShouldBe(SignInService.AccountLockedMessage);
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task AnExpiredLock_LetsTheRightPasswordThrough()
    {
        SeedAccount(lockedUntil: Now.AddMinutes(-1));
        _hasher.Verify("LuuKy@2026", ValidHash).Returns(true);
        _clock.Advance(TimeSpan.FromMinutes(16));

        var result = await _service.SignInAsync("admin", "LuuKy@2026");

        result.Succeeded.ShouldBeTrue();
        _user.LockedUntil.ShouldBeNull();
    }

    [Fact]
    public async Task AnUnknownUser_StillVerifiesAgainstTheDummyHash_AndFailsTheSameWay()
    {
        _store.FindByUserNameAsync("khong-ton-tai", Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _service.SignInAsync("khong-ton-tai", "bất kỳ");

        result.Status.ShouldBe(SignInStatus.InvalidCredentials);
        result.Message.ShouldBe(SignInService.InvalidCredentialsMessage);
        _hasher.Received(1).Verify("bất kỳ", Arg.Any<string>());
    }

    [Fact]
    public async Task SignOut_WritesTheEventThenClearsTheSession()
    {
        _session.UserId.Returns(3);
        _session.UserName.Returns("admin");

        await _service.SignOutAsync();

        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 3,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"Event = {SignInEvent.SignOut}")),
            Arg.Any<CancellationToken>());
        Received.InOrder(() =>
        {
            _auditLog.WriteAsync(Arg.Any<AuditAction>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
            _session.SignOut();
        });
    }

    [Fact]
    public async Task SignOut_WithoutASession_WritesNothing()
    {
        _session.UserId.Returns((int?)null);

        await _service.SignOutAsync();

        await _auditLog.DidNotReceive().WriteAsync(
            Arg.Any<AuditAction>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
        _session.DidNotReceive().SignOut();
    }
}
