using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

public class LockScreenPresenterTests
{
    private readonly IUserStore _store = Substitute.For<IUserStore>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserSession _session = Substitute.For<ICurrentUserSession>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();
    private readonly ILockScreenView _view = Substitute.For<ILockScreenView>();
    private User _user = null!;
    private int _unlockedCount;
    private int _signedOutCount;

    private LockScreenPresenter CreatePresenter()
    {
        var clock = new FakeClock(new DateTime(2026, 10, 2, 9, 0, 0));
        var failedSignIns = new FailedSignInService(_store, clock, new SignInOptions(), _auditLog);
        var signIn = new SignInService(_store, _hasher, _session, _auditLog, clock, failedSignIns);
        return new LockScreenPresenter(
            _view,
            FakeScopeFactory.Create(signIn),
            "Lưu ký – Căn tin",
            "NL",
            "Nguyễn Thị Lan",
            () => _unlockedCount++,
            () => _signedOutCount++);
    }

    private void SeedUser()
    {
        _session.UserId.Returns(7);
        _session.UserName.Returns("luuky");
        _user = new User { Id = 7, UserName = "luuky", PasswordHash = "hash", IsActive = true };
        _store.FindByIdAsync(7, Arg.Any<CancellationToken>()).Returns(_user);
        _store.SaveAsync(_user, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _view.Password.Returns("LuuKy@2026");
    }

    private TaskCompletionSource CloseSignal()
    {
        var closed = new TaskCompletionSource();
        // The real form raises Closed from OnFormClosed; the presenter completes the exit there.
        _view.When(v => v.CloseLock()).Do(_ =>
        {
            _view.Closed += Raise.Event();
            closed.TrySetResult();
        });
        return closed;
    }

    [Fact]
    public void Constructor_ShowsTheSignedInUser()
    {
        CreatePresenter();

        _view.Received(1).ShowUser("NL", "Nguyễn Thị Lan");
    }

    [Fact]
    public async Task Unlock_CorrectPassword_ClosesTheLockResumesAndLogsTheEvent()
    {
        SeedUser();
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);
        var closed = CloseSignal();
        CreatePresenter();

        _view.UnlockClicked += Raise.Event();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).CloseLock();
        _view.DidNotReceive().ShowError(Arg.Any<string>());
        _unlockedCount.ShouldBe(1);
        _signedOutCount.ShouldBe(0);
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 7,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"Event = {SignInEvent.UnlockSession}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unlock_WrongPassword_ShowsTheMessageAndStaysLocked()
    {
        SeedUser();
        _view.Password.Returns("sai");
        _hasher.Verify("sai", "hash").Returns(false);
        var shown = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => shown.TrySetResult());
        CreatePresenter();

        _view.UnlockClicked += Raise.Event();
        await shown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowError(SignInService.InvalidCredentialsMessage);
        _view.DidNotReceive().CloseLock();
        _user.FailedAttemptCount.ShouldBe((byte)1);
        _unlockedCount.ShouldBe(0);
        _signedOutCount.ShouldBe(0);
    }

    [Fact]
    public async Task Unlock_WhenTheAttemptLocksTheAccount_ShowsTheLockedMessageAndSignsOut()
    {
        SeedUser();
        _user.FailedAttemptCount = User.MaxFailedAttempts - 1;
        _view.Password.Returns("sai");
        _hasher.Verify("sai", "hash").Returns(false);
        var closed = CloseSignal();
        CreatePresenter();

        _view.UnlockClicked += Raise.Event();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowLockedMessage(SignInService.AccountLockedMessage);
        _view.Received(1).CloseLock();
        _session.Received(1).SignOut();
        _unlockedCount.ShouldBe(0);
        _signedOutCount.ShouldBe(1);
    }

    [Fact]
    public async Task Unlock_WhenAChangeIsForced_ShowsTheMessageAndSignsOut()
    {
        SeedUser();
        _user.MustChangePassword = true;
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);
        var closed = CloseSignal();
        CreatePresenter();

        _view.UnlockClicked += Raise.Event();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowLockedMessage(SignInService.PasswordChangeRequiredMessage);
        _view.Received(1).CloseLock();
        _session.Received(1).SignOut();
        _unlockedCount.ShouldBe(0);
        _signedOutCount.ShouldBe(1);
    }

    [Fact]
    public async Task Unlock_WhenTheAccountWasDeactivated_SignsOutWithoutCheckingThePassword()
    {
        SeedUser();
        _user.IsActive = false;
        var closed = CloseSignal();
        CreatePresenter();

        _view.UnlockClicked += Raise.Event();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowLockedMessage(SignInService.AccountLockedMessage);
        _view.Received(1).CloseLock();
        _session.Received(1).SignOut();
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        _signedOutCount.ShouldBe(1);
    }

    [Fact]
    public async Task SignOut_Confirmed_ClearsTheSessionAndLeaves()
    {
        SeedUser();
        _view.ConfirmSignOut().Returns(true);
        var closed = CloseSignal();
        CreatePresenter();

        _view.SignOutClicked += Raise.Event();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ConfirmSignOut();
        _view.Received(1).CloseLock();
        _session.Received(1).SignOut();
        _signedOutCount.ShouldBe(1);
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 7,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"Event = {SignInEvent.SignOut}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SignOut_Declined_KeepsTheLockScreen()
    {
        SeedUser();
        _view.ConfirmSignOut().Returns(false);
        CreatePresenter();

        _view.SignOutClicked += Raise.Event();

        _view.Received(1).ConfirmSignOut();
        _view.DidNotReceive().CloseLock();
        _session.DidNotReceive().SignOut();
        _signedOutCount.ShouldBe(0);
    }
}
