using LuuKyCanTin.Domain.LuuKy;

namespace LuuKyCanTin.Application.LuuKy;

/// <summary>The inputs of one cash receipt posting; the service fills number, words and balances itself.</summary>
public sealed record GhiSoBienNhanThuRequest
{
    public int DoiTuongId { get; init; }

    public DateOnly NgayChungTu { get; init; }

    public NghiepVu NghiepVu { get; init; } = NghiepVu.NguoiThanGui;

    public HinhThuc HinhThuc { get; init; } = HinhThuc.TienMat;

    public string? NguoiGuiHoTen { get; init; }

    public string? QuanHe { get; init; }

    public string? SoPhieuGoc { get; init; }

    public string? SoTaiKhoanNguoiGui { get; init; }

    public DateOnly? NgayNhan { get; init; }

    public string? NoiDung { get; init; }

    public decimal SoTien { get; init; }
}
