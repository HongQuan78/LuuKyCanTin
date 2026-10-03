using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

// Presenters are tested against a fake View: no Form is ever created.
public class MainPresenterTests
{
    private readonly IMainView _view = Substitute.For<IMainView>();
    private readonly INavigator _navigator = Substitute.For<INavigator>();
    private readonly IUserStore _store = Substitute.For<IUserStore>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserSession _session = Substitute.For<ICurrentUserSession>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();

    private MainPresenter NewPresenter(string title = "X")
    {
        var clock = new FakeClock(new DateTime(2026, 10, 2, 9, 0, 0));
        var failedSignIns = new FailedSignInService(_store, clock, new SignInOptions(), _auditLog);
        var signIn = new SignInService(_store, _hasher, _session, _auditLog, clock, failedSignIns);
        return new MainPresenter(
            _view, Options.Create(new AppOptions { Title = title }), _navigator, FakeScopeFactory.Create(signIn));
    }

    [Fact]
    public void OnLoaded_ViewLoaded_SetsTitleFromOptions()
    {
        NewPresenter("Tiêu đề từ cấu hình");

        _view.Loaded += Raise.Event();

        _view.Received(1).Title = "Tiêu đề từ cấu hình";
    }

    [Fact]
    public void Constructor_BeforeLoaded_DoesNotTouchView()
    {
        NewPresenter();

        _view.DidNotReceive().Title = Arg.Any<string>();
    }

    [Fact]
    public void StaffMenu_OpensTheStaffRegister()
    {
        NewPresenter();

        _view.OfficersClicked += Raise.Event();

        _navigator.Received(1).OpenOfficers();
    }

    [Fact]
    public void RoleMenu_OpensTheRoleScreen()
    {
        NewPresenter();

        _view.RolesClicked += Raise.Event();

        _navigator.Received(1).OpenRoles();
    }

    [Fact]
    public void ChangePasswordMenu_OpensTheDialog()
    {
        NewPresenter();

        _view.ChangePasswordClicked += Raise.Event();

        _navigator.Received(1).OpenChangePassword();
    }

    [Fact]
    public async Task SignOutMenu_ClearsTheSessionAndClosesTheShell()
    {
        _session.UserId.Returns(3);
        _session.UserName.Returns("admin");
        var closed = new TaskCompletionSource();
        _view.When(v => v.CloseShell()).Do(_ => closed.TrySetResult());

        var presenter = NewPresenter();
        _view.SignOutClicked += Raise.Event();

        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        presenter.IsSignedOut.ShouldBeTrue();
        _session.Received(1).SignOut();
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 3, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructing_OpensNothing()
    {
        NewPresenter();

        _navigator.ReceivedCalls().ShouldBeEmpty();
    }
}
