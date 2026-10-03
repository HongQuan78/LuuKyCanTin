using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

public class LoginPresenterTests
{
    private readonly IUserStore _store = Substitute.For<IUserStore>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserSession _session = Substitute.For<ICurrentUserSession>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();
    private readonly ILoginView _view = Substitute.For<ILoginView>();
    private User? _user;

    private LoginPresenter CreatePresenter()
    {
        var clock = new FakeClock(new DateTime(2026, 10, 2, 9, 0, 0));
        var failedSignIns = new FailedSignInService(_store, clock, new SignInOptions(), _auditLog);
        var service = new SignInService(_store, _hasher, _session, _auditLog, clock, failedSignIns);
        return new LoginPresenter(_view, FakeScopeFactory.Create(service));
    }

    private void SeedAccount(bool mustChangePassword = false)
    {
        _user = new User
        {
            Id = 1,
            UserName = "admin",
            PasswordHash = "hash",
            IsActive = true,
            MustChangePassword = mustChangePassword,
        };
        _store.FindByUserNameAsync("admin", Arg.Any<CancellationToken>()).Returns(_user);
        _store.SaveAsync(_user, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _view.UserName.Returns("admin");
        _view.Password.Returns("LuuKy@2026");
    }

    [Fact]
    public async Task Success_ClosesTheViewAsOk()
    {
        SeedAccount();
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);

        var presenter = CreatePresenter();
        await presenter.SignInAsync();

        presenter.MustChangePassword.ShouldBeFalse();
        _view.Received(1).CloseWithResult(true);
        _view.DidNotReceive().ShowError(Arg.Any<string>());
    }

    [Fact]
    public async Task ForcedChange_ClosesTheViewAsOkAndFlagsTheChange()
    {
        SeedAccount(mustChangePassword: true);
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);

        var presenter = CreatePresenter();
        await presenter.SignInAsync();

        presenter.MustChangePassword.ShouldBeTrue();
        _view.Received(1).CloseWithResult(true);
    }

    [Fact]
    public async Task Failure_ShowsTheErrorAndKeepsTheFormOpen()
    {
        _store.FindByUserNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _view.UserName.Returns("admin");
        _view.Password.Returns("sai");

        await CreatePresenter().SignInAsync();

        _view.Received(1).ShowError(SignInService.InvalidCredentialsMessage);
        _view.DidNotReceive().CloseWithResult(Arg.Any<bool>());
    }

    [Fact]
    public async Task ALockedAccount_ShowsTheLockedMessageAndClearsThePasswordBox()
    {
        SeedAccount();
        _user!.LockedUntil = new DateTime(2026, 10, 2, 9, 5, 0);

        await CreatePresenter().SignInAsync();

        _view.Received(1).ShowError(SignInService.AccountLockedMessage);
        _view.Received(1).ClearPassword();
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }
}
