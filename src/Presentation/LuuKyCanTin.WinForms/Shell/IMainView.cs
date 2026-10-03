namespace LuuKyCanTin.WinForms.Shell;

public interface IMainView
{
    event EventHandler Loaded;

    event EventHandler OfficersClicked;

    /// <summary>The "Hệ thống › Vai trò" menu item.</summary>
    event EventHandler RolesClicked;

    /// <summary>The "Hệ thống › Đổi mật khẩu" menu item.</summary>
    event EventHandler ChangePasswordClicked;

    /// <summary>The "Hệ thống › Đăng xuất" menu item.</summary>
    event EventHandler SignOutClicked;

    string Title { set; }

    /// <summary>Closes the shell so the application context can show the login form again.</summary>
    void CloseShell();

    void ShowError(string message);
}
