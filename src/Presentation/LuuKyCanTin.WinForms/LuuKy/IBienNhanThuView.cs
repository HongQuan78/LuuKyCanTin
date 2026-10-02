using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Domain.LuuKy;

namespace LuuKyCanTin.WinForms.LuuKy;

public interface IBienNhanThuView
{
    event EventHandler? Tai;

    event EventHandler? SoTienThayDoi;

    event EventHandler? GhiSoBam;

    event EventHandler? InBam;

    IReadOnlyList<DoiTuongChon> DanhSachDoiTuong { set; }

    int? DoiTuongId { get; }

    NghiepVu NghiepVu { get; }

    string? NguoiGuiHoTen { get; }

    string? QuanHe { get; }

    HinhThuc HinhThuc { get; }

    string? SoTaiKhoanNguoiGui { get; }

    DateOnly NgayChungTu { get; }

    string? NoiDung { get; }

    decimal? SoTien { get; }

    /// <summary>Live preview of the amount in words.</summary>
    string SoTienBangChu { set; }

    void HienLoi(string thongBao);

    /// <summary>Posting succeeded: show the number, the new balance and enable Print.</summary>
    void DaGhiSo(string soChungTu, decimal soDuSau);

    void HienThiBanIn(byte[] pdf, string tenTep);
}
