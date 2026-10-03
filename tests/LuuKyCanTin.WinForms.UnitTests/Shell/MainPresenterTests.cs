using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

// Presenters are tested against a fake View: no Form is ever created.
public class MainPresenterTests : IDisposable
{
    private static readonly WorkstationInfo Workstation = new("SRV / LuuKyCanTin", true, "QUAY-01", "v1.0.0");

    private readonly IMainView _view = Substitute.For<IMainView>();
    private readonly INavigator _navigator = Substitute.For<INavigator>();
    private readonly IUserStore _store = Substitute.For<IUserStore>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserSession _session = Substitute.For<ICurrentUserSession>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();
    private readonly ISignedInUserQuery _signedInUser = Substitute.For<ISignedInUserQuery>();
    private readonly FakeClock _clock = new(new DateTime(2026, 10, 2, 9, 0, 0));
    private readonly List<MainPresenter> _presenters = [];
    private long _elapsed;
    private NavigationModel? _navigation;

    public MainPresenterTests()
    {
        _view.When(v => v.ShowNavigation(Arg.Any<NavigationModel>())).Do(c => _navigation = c.Arg<NavigationModel>());
        _signedInUser.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new SignedInUserDto("lan.nt", "Nguyễn Thị Lan", "Kế toán", "TRẠI TẠM GIAM SỐ 1"));
        // Most tests care about the shell, not the menu: grant every permission unless a test narrows it.
        _session.HasPermission(Arg.Any<string>()).Returns(true);
    }

    private MainPresenter NewPresenter(string title = "X", IdleMonitor? idleMonitor = null)
    {
        var failedSignIns = new FailedSignInService(_store, _clock, new SignInOptions(), _auditLog);
        var signIn = new SignInService(_store, _hasher, _session, _auditLog, _clock, failedSignIns);
        var presenter = new MainPresenter(
            _view, Options.Create(new AppOptions { Title = title }), _navigator,
            FakeScopeFactory.Create(signIn, _signedInUser), _clock, Workstation, _session,
            idleMonitor ?? new IdleMonitor(TimeSpan.FromMinutes(5), () => _elapsed));
        _presenters.Add(presenter);
        return presenter;
    }

    public void Dispose()
    {
        foreach (var presenter in _presenters)
            presenter.Dispose();
    }

    private async Task LoadAsync()
    {
        var homeShown = new TaskCompletionSource();
        _view.When(v => v.ShowHome(Arg.Any<string>(), Arg.Any<string>())).Do(_ => homeShown.TrySetResult());
        _view.Loaded += Raise.Event();
        await homeShown.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private void Navigate(string key) =>
        _view.NavigationRequested += Raise.Event<EventHandler<NavItem>>(_view, _navigation!.AllItems.Single(i => i.Key == key));

    [Fact]
    public async Task OnLoaded_ViewLoaded_SetsTitleFromOptions()
    {
        NewPresenter("Tiêu đề từ cấu hình");

        await LoadAsync();

        _view.Received(1).Title = "Tiêu đề từ cấu hình";
    }

    [Fact]
    public async Task OnLoaded_UserWithOnlyMasterDataView_SeesStaffButNotAccountsOrRoles()
    {
        _session.HasPermission(Arg.Any<string>())
            .Returns(call => call.Arg<string>() == PermissionCodes.MasterData.View);
        NewPresenter();

        await LoadAsync();

        _navigation.ShouldNotBeNull();
        _navigation.AllItems.ShouldContain(i => i.Key == ShellNavigation.OfficersKey && i.Caption.Contains("Cán bộ"));
        _navigation.AllItems.ShouldNotContain(i => i.Key == ShellNavigation.AccountsKey);
        _navigation.AllItems.ShouldNotContain(i => i.Key == ShellNavigation.RolesKey);
        _navigation.Home.ShouldNotBeNull();
    }

    [Fact]
    public async Task OnLoaded_ViewLoaded_ShowsNavigationStatusUserAndHome()
    {
        NewPresenter();

        await LoadAsync();

        _navigation.ShouldNotBeNull();
        _view.Received(1).ShowWorkstation(Workstation);
        _view.Received(1).ShowUser("NL", "Nguyễn Thị Lan", "Kế toán");
        _view.Received(1).FacilityName = "TRẠI TẠM GIAM SỐ 1";
        _view.Received(1).ShowHome("Chào buổi sáng, Nguyễn Thị Lan", "Thứ Sáu, 02/10/2026");
        _view.DidNotReceive().ShowError(Arg.Any<string>());
    }

    [Fact]
    public async Task OnLoaded_UserQueryFails_ShowsTheErrorAndStillShowsHome()
    {
        _signedInUser.GetAsync(Arg.Any<CancellationToken>()).Returns<SignedInUserDto?>(_ => throw new InvalidOperationException("db down"));
        NewPresenter();

        await LoadAsync();

        _view.Received(1).ShowError(Arg.Is<string>(s => s.Contains("db down")));
    }

    [Fact]
    public void Constructor_BeforeLoaded_DoesNotTouchView()
    {
        NewPresenter();

        _view.DidNotReceive().Title = Arg.Any<string>();
        _view.DidNotReceive().ShowNavigation(Arg.Any<NavigationModel>());
    }

    [Theory]
    [InlineData(ShellNavigation.OfficersKey, nameof(INavigator.OpenOfficers))]
    [InlineData(ShellNavigation.AddInmateKey, nameof(INavigator.OpenAddInmate))]
    [InlineData(ShellNavigation.DepositReceiptKey, nameof(INavigator.OpenDepositReceipt))]
    [InlineData(ShellNavigation.AccountsKey, nameof(INavigator.OpenAccounts))]
    [InlineData(ShellNavigation.RolesKey, nameof(INavigator.OpenRoles))]
    [InlineData(ShellNavigation.ChangePasswordKey, nameof(INavigator.OpenChangePassword))]
    public async Task NavigationRequested_AScreenItem_OpensTheSameScreenAsTheOldMenu(string key, string navigatorMethod)
    {
        NewPresenter();
        await LoadAsync();

        Navigate(key);

        _navigator.ReceivedCalls().Select(c => c.GetMethodInfo().Name).ShouldBe([navigatorMethod]);
    }

    [Fact]
    public async Task NavigationRequested_Home_ShowsHomeAgainWithTheCurrentTime()
    {
        NewPresenter();
        await LoadAsync();
        _clock.Now = new DateTime(2026, 10, 2, 14, 0, 0);

        Navigate(ShellNavigation.HomeKey);

        _view.Received(1).ShowHome("Chào buổi chiều, Nguyễn Thị Lan", "Thứ Sáu, 02/10/2026");
    }

    [Fact]
    public async Task NavigationRequested_AScreenThatFails_ShowsTheError()
    {
        _navigator.When(n => n.OpenRoles()).Do(_ => throw new InvalidOperationException("boom"));
        NewPresenter();
        await LoadAsync();

        Navigate(ShellNavigation.RolesKey);

        _view.Received(1).ShowError(Arg.Is<string>(s => s.Contains("boom")));
    }

    [Fact]
    public async Task SignOut_ClearsTheSessionAndClosesTheShell()
    {
        _session.UserId.Returns(3);
        _session.UserName.Returns("admin");
        var closed = new TaskCompletionSource();
        _view.When(v => v.CloseShell()).Do(_ => closed.TrySetResult());
        var presenter = NewPresenter();
        await LoadAsync();

        Navigate(ShellNavigation.SignOutKey);

        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        presenter.IsSignedOut.ShouldBeTrue();
        _session.Received(1).SignOut();
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 3, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NavigationRequested_LockSession_ShowsTheLockScreenAndWritesTheAuditRow()
    {
        _session.UserId.Returns(3);
        _session.UserName.Returns("admin");
        var locked = new TaskCompletionSource();
        _navigator.When(n => n.OpenLockScreen(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Action>(), Arg.Any<Action>()))
            .Do(_ => locked.TrySetResult());
        NewPresenter("Tiêu đề từ cấu hình");
        await LoadAsync();

        Navigate(ShellNavigation.LockSessionKey);

        await locked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        _navigator.Received(1).OpenLockScreen(
            "Tiêu đề từ cấu hình", "NL", "Nguyễn Thị Lan", Arg.Any<Action>(), Arg.Any<Action>());
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 3,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"Event = {SignInEvent.LockSession}")
                && o.ToString()!.Contains($"Kind = {SessionLockKind.Manual.ToCode()}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IdleTimeout_OpensTheLockScreenWithAnAutomaticAuditRow()
    {
        _session.UserId.Returns(3);
        _session.UserName.Returns("admin");
        var locked = new TaskCompletionSource();
        _navigator.When(n => n.OpenLockScreen(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Action>(), Arg.Any<Action>()))
            .Do(_ => locked.TrySetResult());
        var auditWritten = new TaskCompletionSource();
        _auditLog.WriteAsync(
                Arg.Any<AuditAction>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                auditWritten.TrySetResult();
                return Task.CompletedTask;
            });
        var monitor = new IdleMonitor(TimeSpan.FromMinutes(5), () => _elapsed);
        NewPresenter(idleMonitor: monitor);
        await LoadAsync();

        _elapsed += (long)TimeSpan.FromMinutes(5).TotalMilliseconds;
        monitor.CheckIdle();

        await locked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await auditWritten.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await _auditLog.Received(1).WriteAsync(
            AuditAction.SignIn, "User", 3,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"Event = {SignInEvent.LockSession}")
                && o.ToString()!.Contains($"Kind = {SessionLockKind.Automatic.ToCode()}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LockSession_AuditWriteFails_StillLocksTheScreen()
    {
        _session.UserId.Returns(3);
        _session.UserName.Returns("admin");
        _auditLog.WriteAsync(
                Arg.Any<AuditAction>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("db down")));
        var locked = new TaskCompletionSource();
        _navigator.When(n => n.OpenLockScreen(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Action>(), Arg.Any<Action>()))
            .Do(_ => locked.TrySetResult());
        NewPresenter();
        await LoadAsync();

        Navigate(ShellNavigation.LockSessionKey);

        await locked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        _navigator.Received(1).OpenLockScreen(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Action>(), Arg.Any<Action>());
        _view.DidNotReceive().ShowError(Arg.Any<string>());
    }

    [Fact]
    public async Task LockScreen_SignOut_LeavesTheShell()
    {
        _session.UserId.Returns(3);
        _session.UserName.Returns("admin");
        Action? onSignedOut = null;
        var locked = new TaskCompletionSource();
        _navigator.When(n => n.OpenLockScreen(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Action>(), Arg.Do<Action>(a => onSignedOut = a)))
            .Do(_ => locked.TrySetResult());
        var closed = new TaskCompletionSource();
        _view.When(v => v.CloseShell()).Do(_ => closed.TrySetResult());
        var presenter = NewPresenter();
        await LoadAsync();

        Navigate(ShellNavigation.LockSessionKey);
        await locked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        onSignedOut!();

        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        presenter.IsSignedOut.ShouldBeTrue();
    }

    [Fact]
    public void Constructing_OpensNothing()
    {
        NewPresenter();

        _navigator.ReceivedCalls().ShouldBeEmpty();
    }
}
