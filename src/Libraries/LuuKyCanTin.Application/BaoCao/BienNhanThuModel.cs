using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.LuuKy;

namespace LuuKyCanTin.Application.BaoCao;

/// <summary>
/// Everything the skeleton's receipt print shows. Values come from the stored document and the unit row, so
/// the print matches the posting even years later.
/// </summary>
public sealed class BienNhanThuModel : IReportModel
{
    public const string MaMau = "BIEN_NHAN_THU";

    public string MaMauIn => MaMau;

    public string? TenCoQuanChuQuan { get; init; }

    public string TenDonVi { get; init; } = "";

    public string DiaChi { get; init; } = "";

    public string SoChungTu { get; init; } = "";

    public DateOnly NgayChungTu { get; init; }

    public string HoTenDoiTuong { get; init; } = "";

    public LoaiDoiTuong LoaiDoiTuong { get; init; }

    public string? NguoiGuiHoTen { get; init; }

    public string? QuanHe { get; init; }

    public HinhThuc HinhThuc { get; init; }

    public string? SoTaiKhoanNguoiGui { get; init; }

    public string? NoiDung { get; init; }

    public decimal SoTien { get; init; }

    public string SoTienBangChu { get; init; } = "";
}
