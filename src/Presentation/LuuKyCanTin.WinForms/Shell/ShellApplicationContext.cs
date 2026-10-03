using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// Owns the login → shell → sign-out loop without restarting the app. Each session opens a fresh MainForm, so
/// nothing from the previous user stays on screen; closing the login form exits the app.
/// </summary>
internal sealed class ShellApplicationContext(IServiceProvider services) : ApplicationContext
{
    public void Start() => ShowLogin();

    private void ShowLogin()
    {
        var login = services.GetRequiredService<LoginForm>();
        var presenter = ActivatorUtilities.CreateInstance<LoginPresenter>(services, login);
        login.FormClosed += (_, _) =>
        {
            var succeeded = login.DialogResult == DialogResult.OK;
            login.Dispose();

            if (!succeeded)
            {
                ExitThread();
                return;
            }

            if (presenter.MustChangePassword)
                ShowForcedPasswordChange();
            else
                ShowShell();
        };
        login.Show();
    }

    private void ShowForcedPasswordChange()
    {
        using var form = new ChangePasswordForm();
        _ = new ChangePasswordPresenter(form, services.GetRequiredService<IServiceScopeFactory>(), isForced: true);
        if (form.ShowDialog() == DialogResult.OK)
            ShowShell();
        else
            ShowLogin();
    }

    private void ShowShell()
    {
        var main = services.GetRequiredService<MainForm>();
        var presenter = ActivatorUtilities.CreateInstance<MainPresenter>(services, main);
        main.FormClosed += (_, _) =>
        {
            var isSignedOut = presenter.IsSignedOut;
            main.Dispose();

            if (isSignedOut)
                ShowLogin();
            else
                ExitThread();
        };
        main.Show();
    }
}
