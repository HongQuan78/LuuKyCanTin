namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The change-password dialog, used forced after a first sign-in and voluntary from the shell menu.</summary>
public interface IDoiMatKhauView : IDisposable
{
    event EventHandler LuuClicked;

    event EventHandler HuyClicked;

    string TieuDe { set; }

    /// <summary>In forced mode Cancel signs out instead of returning to the shell.</summary>
    bool BatBuoc { set; }

    string MatKhauHienTai { get; }

    string MatKhauMoi { get; }

    string XacNhan { get; }

    void HienLoi(string thongBao);

    /// <summary>Closes the dialog; true means the password was changed.</summary>
    void DongVoiKetQua(bool thanhCong);

    /// <summary>Shows the dialog modally; true if it closed after a successful change.</summary>
    bool HienThi();
}
