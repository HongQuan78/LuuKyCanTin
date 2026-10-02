using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// Owns the login → shell → sign-out loop without restarting the app. Each session opens a fresh MainForm, so
/// nothing from the previous user stays on screen; closing the login form exits the app.
/// </summary>
internal sealed class ShellApplicationContext(IServiceProvider services) : ApplicationContext
{
    public void BatDau() => MoDangNhap();

    private void MoDangNhap()
    {
        var login = services.GetRequiredService<LoginForm>();
        var presenter = ActivatorUtilities.CreateInstance<LoginPresenter>(services, login);
        login.FormClosed += (_, _) =>
        {
            var thanhCong = login.DialogResult == DialogResult.OK;
            login.Dispose();

            if (!thanhCong)
            {
                ExitThread();
                return;
            }

            if (presenter.PhaiDoiMatKhau)
                MoDoiMatKhauBatBuoc();
            else
                MoMoShell();
        };
        login.Show();
    }

    private void MoDoiMatKhauBatBuoc()
    {
        using var form = new DoiMatKhauForm();
        _ = new DoiMatKhauPresenter(form, services.GetRequiredService<IServiceScopeFactory>(), batBuoc: true);
        if (form.ShowDialog() == DialogResult.OK)
            MoMoShell();
        else
            MoDangNhap();
    }

    private void MoMoShell()
    {
        var main = services.GetRequiredService<MainForm>();
        var presenter = ActivatorUtilities.CreateInstance<MainPresenter>(services, main);
        main.FormClosed += (_, _) =>
        {
            var daDangXuat = presenter.DaDangXuat;
            main.Dispose();

            if (daDangXuat)
                MoDangNhap();
            else
                ExitThread();
        };
        main.Show();
    }
}
