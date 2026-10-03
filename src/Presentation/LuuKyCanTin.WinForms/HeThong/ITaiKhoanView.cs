using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.WinForms.HeThong;

/// <summary>The "Tài khoản" screen: the account grid and its administration actions.</summary>
public interface ITaiKhoanView
{
    event EventHandler Loaded;

    event EventHandler ThemClicked;

    event EventHandler PhanVaiTroClicked;

    /// <summary>Deactivates the selected account, or reactivates it when it is already off.</summary>
    event EventHandler NgungKichHoatClicked;

    event EventHandler MoKhoaClicked;

    event EventHandler DatLaiMatKhauClicked;

    TaiKhoanDto? TaiKhoanDangChon { get; }

    void HienDanhSach(IReadOnlyList<TaiKhoanDto> danhSach);

    /// <summary>Asks the user to confirm a destructive action; true means go ahead.</summary>
    bool CoDongY(string thongBao);

    void HienThongBao(string thongBao);

    void HienLoi(string thongBao);
}
