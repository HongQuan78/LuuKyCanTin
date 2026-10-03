using LuuKyCanTin.Application.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Sign-in with lockout and the forced first change (FR1). The shell opens only on a successful result.</summary>
public sealed class LoginPresenter
{
    private readonly ILoginView _view;
    private readonly IServiceScopeFactory _scopeFactory;

    public LoginPresenter(ILoginView view, IServiceScopeFactory scopeFactory)
    {
        _view = view;
        _scopeFactory = scopeFactory;
        _view.SignInClicked += async (_, _) =>
        {
            try
            {
                await SignInAsync();
            }
            catch (Exception ex)
            {
                // An unexpected failure (database down) must still reach the user, not the global handler.
                _view.ShowError($"Không đăng nhập được: {ex.Message}");
            }
        };
    }

    /// <summary>Set after a successful sign-in; true when the shell must wait for a password change.</summary>
    public bool MustChangePassword { get; private set; }

    public async Task SignInAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var signIn = scope.ServiceProvider.GetRequiredService<SignInService>();

        var result = await signIn.SignInAsync(_view.UserName, _view.Password);
        if (result.Succeeded)
        {
            MustChangePassword = result.MustChangePassword;
            _view.CloseWithResult(true);
            return;
        }

        _view.ShowError(result.Message ?? SignInService.InvalidCredentialsMessage);
        if (result.Status == SignInStatus.AccountLocked)
            _view.ClearPassword();
    }
}
