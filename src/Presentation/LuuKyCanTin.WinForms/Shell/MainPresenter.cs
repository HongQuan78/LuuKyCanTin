using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuuKyCanTin.WinForms.Shell;

public sealed class MainPresenter
{
    private readonly IMainView _view;
    private readonly AppOptions _options;
    private readonly IServiceScopeFactory _scopes;
    private readonly IClock _clock;
    private readonly WorkstationInfo _workstation;
    private readonly ICurrentUser _currentUser;
    private readonly NavigationModel _navigation;
    private string _displayName = "";
    private bool _isSignedOut;

    public MainPresenter(
        IMainView view,
        IOptions<AppOptions> options,
        INavigator navigator,
        IServiceScopeFactory scopes,
        IClock clock,
        WorkstationInfo workstation,
        ICurrentUser currentUser)
    {
        _view = view;
        _options = options.Value;
        _scopes = scopes;
        _clock = clock;
        _workstation = workstation;
        _currentUser = currentUser;
        _navigation = ShellNavigation.Create(navigator, ShowHome, OnSignOut);
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

        try
        {
            using var scope = _scopes.CreateScope();
            var user = await scope.ServiceProvider.GetRequiredService<ISignedInUserQuery>().GetAsync();
            if (user is not null)
            {
                _displayName = user.DisplayName;
                _view.ShowUser(UserInitials.Create(user.DisplayName), user.DisplayName, user.RoleNames);
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
}
