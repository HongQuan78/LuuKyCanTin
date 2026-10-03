using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using WinFormsApp = System.Windows.Forms.Application;

namespace LuuKyCanTin.WinForms.Shell;

public sealed class MainPresenter : IDisposable
{
    private const int LockAuditTimeoutSeconds = 5;
    private const string LockScreenErrorMessage = "Không mở được màn hình khoá.";

    private readonly IMainView _view;
    private readonly AppOptions _options;
    private readonly IServiceScopeFactory _scopes;
    private readonly IClock _clock;
    private readonly WorkstationInfo _workstation;
    private readonly ICurrentUser _currentUser;
    private readonly INavigator _navigator;
    private readonly NavigationModel _navigation;
    private readonly IdleMonitor _idleMonitor;
    private string _displayName;
    private string _initials;
    private bool _isSignedOut;
    private bool _isLocked;
    private bool _isDisposed;

    public MainPresenter(
        IMainView view,
        IOptions<AppOptions> options,
        INavigator navigator,
        IServiceScopeFactory scopes,
        IClock clock,
        WorkstationInfo workstation,
        ICurrentUser currentUser,
        IdleMonitor idleMonitor)
    {
        _view = view;
        _options = options.Value;
        _scopes = scopes;
        _clock = clock;
        _workstation = workstation;
        _currentUser = currentUser;
        _navigator = navigator;
        // The user query fills these in on load; the session snapshot is the fallback when it fails.
        _displayName = currentUser.FullName ?? currentUser.UserName ?? "";
        _initials = UserInitials.Create(_displayName);
        _navigation = ShellNavigation.Create(
            navigator, ShowHome, OnSignOut, () => OnLockRequested(SessionLockKind.Manual));
        _idleMonitor = idleMonitor;
        _idleMonitor.IdleTimeout += (_, _) => OnLockRequested(SessionLockKind.Automatic);
        // Ctrl+L arrives through the app-level filter, so it also works while a modal dialog has the focus.
        _idleMonitor.LockRequested += (_, _) => OnLockRequested(SessionLockKind.Manual);
        _view.Loaded += OnLoaded;
        _view.NavigationRequested += OnNavigationRequested;
    }

    /// <summary>True after a successful sign-out, so the application context returns to the login form.</summary>
    public bool IsSignedOut => _isSignedOut;

    private async void OnLoaded(object? sender, EventArgs e)
    {
        _view.Title = _options.Title;
        // The cached permissions decide what the sidebar and the Trang chủ tiles show; writes re-check the database.
        _view.ShowNavigation(ShellNavigation.BuildVisible(_navigation, _currentUser.HasPermission));
        _view.ShowWorkstation(_workstation);
        // Watch input only while a session is signed in; the monitor is removed when the shell closes.
        WinFormsApp.AddMessageFilter(_idleMonitor);
        _idleMonitor.Start();

        try
        {
            using var scope = _scopes.CreateScope();
            var user = await scope.ServiceProvider.GetRequiredService<ISignedInUserQuery>().GetAsync();
            if (user is not null)
            {
                _displayName = user.DisplayName;
                _initials = UserInitials.Create(user.DisplayName);
                _view.ShowUser(_initials, user.DisplayName, user.RoleNames);
                _view.FacilityName = user.FacilityName;
            }
        }
        catch (Exception ex)
        {
            _view.ShowError($"Không tải được thông tin người dùng: {ex.Message}");
        }

        ShowHome();
    }

    private void OnNavigationRequested(object? sender, NavItem item)
    {
        try
        {
            item.Open();
        }
        catch (Exception ex)
        {
            _view.ShowError($"Không mở được màn hình: {ex.Message}");
        }
    }

    private void ShowHome() =>
        _view.ShowHome(HomeGreeting.CreateGreeting(_clock.Now, _displayName), HomeGreeting.CreateDateLine(_clock.Today));

    private async void OnSignOut()
    {
        if (_isSignedOut)
            return;

        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<SignInService>().SignOutAsync();
            _isSignedOut = true;
            _view.CloseShell();
        }
        catch (Exception ex)
        {
            _view.ShowError($"Không đăng xuất được: {ex.Message}");
        }
    }

    private async void OnLockRequested(SessionLockKind kind)
    {
        if (_isLocked || _isSignedOut)
            return;

        _isLocked = true;
        _idleMonitor.Pause();
        var isDisplayed = false;
        try
        {
            // Cover the screen before touching the database: a slow or failed audit write must never leave the
            // workstation interactive, and only a displayed overlay gets a lock row.
            _navigator.OpenLockScreen(_options.Title, _initials, _displayName, OnUnlocked, OnLockedSignOut);
            isDisplayed = true;
            await WriteLockAuditAsync(kind);
        }
        catch (Exception ex)
        {
            if (isDisplayed)
                Log.Error(ex, "Failed to write the lock audit row.");
            else
                _view.ShowError(LockScreenErrorMessage);
        }
        finally
        {
            if (!isDisplayed)
            {
                _isLocked = false;
                _idleMonitor.Resume();
            }
        }
    }

    private async Task WriteLockAuditAsync(SessionLockKind kind)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(LockAuditTimeoutSeconds));
        using var scope = _scopes.CreateScope();
        await scope.ServiceProvider.GetRequiredService<SignInService>().LockSessionAsync(kind, timeout.Token);
    }

    private void OnUnlocked()
    {
        _isLocked = false;
        // The time spent locked never counts as idle.
        _idleMonitor.Resume();
    }

    private void OnLockedSignOut()
    {
        // The lock presenter already cleared the session and wrote the sign-out event; just leave the shell.
        _isLocked = false;
        _isSignedOut = true;
        _view.CloseShell();
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        WinFormsApp.RemoveMessageFilter(_idleMonitor);
        _idleMonitor.Dispose();
    }
}
