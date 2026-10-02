using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Application.LuuKy;

namespace LuuKyCanTin.Application.BaoCao;

/// <summary>
/// Builds the receipt print model from the stored snapshot columns plus the unit header. It never reads the
/// detainee master, so a reprint matches the original.
/// </summary>
public sealed class LayBienNhanThuDeInQuery(IChungTuLuuKyStore chungTuStore, IThongTinDonViStore thongTinDonViStore)
{
    public async Task<BienNhanThuModel?> LayAsync(long chungTuId, CancellationToken ct = default)
    {
        var chungTu = await chungTuStore.TimTheoIdAsync(chungTuId, ct);
        if (chungTu is null)
            return null;

        var donVi = await thongTinDonViStore.LayAsync(ct);

        return new BienNhanThuModel
        {
            TenCoQuanChuQuan = donVi?.TenCoQuanChuQuan,
            TenDonVi = donVi?.TenDonVi ?? "",
            DiaChi = donVi?.DiaChi ?? "",
            SoChungTu = chungTu.SoChungTu,
            NgayChungTu = chungTu.NgayChungTu,
            HoTenDoiTuong = chungTu.HoTenDoiTuong,
            LoaiDoiTuong = chungTu.LoaiDoiTuong,
            NguoiGuiHoTen = chungTu.NguoiGuiHoTen,
            QuanHe = chungTu.QuanHe,
            HinhThuc = chungTu.HinhThuc,
            SoTaiKhoanNguoiGui = chungTu.SoTaiKhoanNguoiGui,
            NoiDung = chungTu.NoiDung,
            SoTien = chungTu.SoTien,
            SoTienBangChu = chungTu.SoTienBangChu,
        };
    }
}
