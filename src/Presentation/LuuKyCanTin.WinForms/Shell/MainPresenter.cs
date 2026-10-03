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
    private bool _isSignedOut;

    public MainPresenter(IMainView view, IOptions<AppOptions> options, INavigator navigator, IServiceScopeFactory scopes)
    {
        _view = view;
        _options = options.Value;
        _scopes = scopes;
        _view.Loaded += OnLoaded;
        _view.OfficersClicked += (_, _) => navigator.OpenOfficers();
        _view.RolesClicked += (_, _) => navigator.OpenRoles();
        _view.ChangePasswordClicked += (_, _) => navigator.OpenChangePassword();
        _view.SignOutClicked += OnSignOutClicked;
    }

    /// <summary>True after a successful sign-out, so the application context returns to the login form.</summary>
    public bool IsSignedOut => _isSignedOut;

    private void OnLoaded(object? sender, EventArgs e)
    {
        _view.Title = _options.Title;
    }

    private async void OnSignOutClicked(object? sender, EventArgs e)
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
