using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.WinForms.HeThong;

/// <summary>The create-account dialog: one active staff member without an account, plus at least one role.</summary>
public interface ITaoTaiKhoanView : IDisposable
{
    event EventHandler Loaded;

    event EventHandler TaoClicked;

    string TenDangNhap { get; }

    int? CanBoId { get; }

    IReadOnlyList<int> VaiTroDaChon { get; }

    void HienDanhSachCanBo(IReadOnlyList<CanBoDto> danhSach);

    void HienDanhSachVaiTro(IReadOnlyList<VaiTroDto> danhSach);

    /// <summary>Shows the dialog modally; true if it closed after a successful create.</summary>
    bool HienThi();

    void HienLoi(string thongBao);

    void DongDaLuu();
}
