using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

public class ChangePasswordPresenterTests
{
    private readonly IUserStore _store = Substitute.For<IUserStore>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserSession _currentUser = Substitute.For<ICurrentUserSession>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();
    private readonly IChangePasswordView _view = Substitute.For<IChangePasswordView>();
    private User _user = null!;

    private ChangePasswordPresenter CreatePresenter(bool isForced)
    {
        var clock = new FakeClock(new DateTime(2026, 10, 2, 9, 0, 0));
        var failedSignIns = new FailedSignInService(_store, clock, new SignInOptions(), _auditLog);
        var changePassword = new ChangePasswordService(_store, _hasher, _currentUser, _auditLog, failedSignIns);
        var signIn = new SignInService(_store, _hasher, _currentUser, _auditLog, clock, failedSignIns);
        return new ChangePasswordPresenter(_view, FakeScopeFactory.Create(changePassword, signIn), isForced);
    }

    private void SeedUser()
    {
        _currentUser.UserId.Returns(7);
        _user = new User { Id = 7, UserName = "luuky", PasswordHash = "hash" };
        _store.FindByIdAsync(7, Arg.Any<CancellationToken>()).Returns(_user);
        _store.SaveAsync(_user, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _view.CurrentPassword.Returns("LuuKy@2026");
        _view.NewPassword.Returns("Moi@2026a");
        _view.Confirmation.Returns("Moi@2026a");
    }

    [Fact]
    public async Task Save_WithAValidChange_ClosesWithOk()
    {
        SeedUser();
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);
        _hasher.Hash("Moi@2026a").Returns("hash-moi");
        var closed = new TaskCompletionSource();
        _view.When(v => v.CloseWithResult(Arg.Any<bool>())).Do(_ => closed.TrySetResult());

        CreatePresenter(isForced: false);
        _view.SaveClicked += Raise.Event();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).CloseWithResult(true);
        _view.DidNotReceive().ShowError(Arg.Any<string>());
        _view.DidNotReceive().ShowFieldError(Arg.Any<PasswordField>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Save_WithAPolicyViolation_ShowsTheMessagesUnderTheNewPasswordAndStaysOpen()
    {
        SeedUser();
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);
        _view.NewPassword.Returns("abc");
        _view.Confirmation.Returns("abc");
        var errorShown = new TaskCompletionSource();
        _view.When(v => v.ShowFieldError(Arg.Any<PasswordField>(), Arg.Any<string>())).Do(_ => errorShown.TrySetResult());

        CreatePresenter(isForced: false);
        _view.SaveClicked += Raise.Event();
        await errorShown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowFieldError(PasswordField.New, Arg.Is<string>(s => s.Contains(PasswordPolicy.TooShortMessage)));
        _view.DidNotReceive().ShowError(Arg.Any<string>());
        _view.DidNotReceive().CloseWithResult(Arg.Any<bool>());
        await _store.DidNotReceive().SaveAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Save_WithAWrongCurrentPassword_ShowsTheErrorUnderTheCurrentPassword()
    {
        SeedUser();
        _hasher.Verify("LuuKy@2026", "hash").Returns(false);
        var errorShown = new TaskCompletionSource();
        _view.When(v => v.ShowFieldError(Arg.Any<PasswordField>(), Arg.Any<string>())).Do(_ => errorShown.TrySetResult());

        CreatePresenter(isForced: false);
        _view.SaveClicked += Raise.Event();
        await errorShown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowFieldError(PasswordField.Current, SignInService.InvalidCredentialsMessage);
    }

    [Fact]
    public async Task Save_WithAMismatchedConfirmation_ShowsTheErrorUnderTheConfirmation()
    {
        SeedUser();
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);
        _view.Confirmation.Returns("Khac@2026a");
        var errorShown = new TaskCompletionSource();
        _view.When(v => v.ShowFieldError(Arg.Any<PasswordField>(), Arg.Any<string>())).Do(_ => errorShown.TrySetResult());

        CreatePresenter(isForced: false);
        _view.SaveClicked += Raise.Event();
        await errorShown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowFieldError(PasswordField.Confirmation, PasswordPolicy.ConfirmationMismatchMessage);
    }

    [Fact]
    public async Task Save_WhenTheAccountGetsLocked_ShowsTheBanner()
    {
        SeedUser();
        _user.FailedAttemptCount = User.MaxFailedAttempts - 1;
        _hasher.Verify("LuuKy@2026", "hash").Returns(false);
        var errorShown = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => errorShown.TrySetResult());

        CreatePresenter(isForced: false);
        _view.SaveClicked += Raise.Event();
        await errorShown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowError(SignInService.AccountLockedMessage);
        _view.DidNotReceive().ShowFieldError(Arg.Any<PasswordField>(), Arg.Any<string>());
    }

    [Fact]
    public void Constructor_EmptyDialog_ShowsEveryRuleUnmetAndDisablesSave()
    {
        _view.CurrentPassword.Returns("");
        _view.NewPassword.Returns("");
        _view.Confirmation.Returns("");

        CreatePresenter(isForced: true);

        _view.Received(1).ShowRuleResults(Arg.Is<IReadOnlyList<PasswordRuleResult>>(r => r.Count == 5 && r.All(v => !v.IsSatisfied)));
        _view.Received(1).CanSave = false;
    }

    [Fact]
    public void InputChanged_EveryRuleMetAndConfirmed_EnablesSave()
    {
        SeedUser();
        CreatePresenter(isForced: false);
        _view.ClearReceivedCalls();

        _view.InputChanged += Raise.Event();

        _view.Received(1).ShowRuleResults(Arg.Is<IReadOnlyList<PasswordRuleResult>>(r => r.All(v => v.IsSatisfied)));
        _view.Received(1).CanSave = true;
    }

    [Fact]
    public void InputChanged_RulesMetButConfirmationDiffers_KeepsSaveDisabled()
    {
        SeedUser();
        _view.Confirmation.Returns("Moi@2026");
        CreatePresenter(isForced: false);
        _view.ClearReceivedCalls();

        _view.InputChanged += Raise.Event();

        _view.Received(1).CanSave = false;
    }

    [Fact]
    public void InputChanged_NewPasswordSameAsCurrent_FailsThatRule()
    {
        SeedUser();
        _view.NewPassword.Returns("LuuKy@2026");
        _view.Confirmation.Returns("LuuKy@2026");
        CreatePresenter(isForced: false);
        _view.ClearReceivedCalls();

        _view.InputChanged += Raise.Event();

        _view.Received(1).ShowRuleResults(Arg.Is<IReadOnlyList<PasswordRuleResult>>(
            r => r.Single(v => !v.IsSatisfied).Rule == PasswordRule.DifferentFromCurrent));
        _view.Received(1).CanSave = false;
    }

    [Fact]
    public async Task Cancel_InForcedMode_SignsOut()
    {
        SeedUser();
        _currentUser.UserName.Returns("luuky");
        var closed = new TaskCompletionSource();
        _view.When(v => v.CloseWithResult(Arg.Any<bool>())).Do(_ => closed.TrySetResult());

        CreatePresenter(isForced: true);
        _view.CancelClicked += Raise.Event();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).CloseWithResult(false);
        _currentUser.Received(1).SignOut();
    }

    [Fact]
    public async Task Cancel_InVoluntaryMode_KeepsTheSession()
    {
        SeedUser();
        var closed = new TaskCompletionSource();
        _view.When(v => v.CloseWithResult(Arg.Any<bool>())).Do(_ => closed.TrySetResult());

        CreatePresenter(isForced: false);
        _view.CancelClicked += Raise.Event();
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).CloseWithResult(false);
        _currentUser.DidNotReceive().SignOut();
    }
}
