namespace LuuKyCanTin.WinForms.Shell;

public interface IMainView
{
    event EventHandler Loaded;

    event EventHandler DanhMucCanBoClicked;

    /// <summary>The "Hệ thống › Vai trò" menu item.</summary>
    event EventHandler VaiTroClicked;

    /// <summary>The "Hệ thống › Đổi mật khẩu" menu item.</summary>
    event EventHandler DoiMatKhauClicked;

    /// <summary>The "Hệ thống › Đăng xuất" menu item.</summary>
    event EventHandler DangXuatClicked;

    string TieuDe { set; }

    /// <summary>Closes the shell so the application context can show the login form again.</summary>
    void Dong();

    void HienLoi(string thongBao);
}
