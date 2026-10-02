namespace LuuKyCanTin.Domain.LuuKy;

/// <summary>
/// Why money moved. The tens digit is the <see cref="LoaiPhieu"/> (1 receipt, 2 payout), which the database
/// enforces with a CHECK constraint.
/// </summary>
public enum NghiepVu : byte
{
    MangTheoKhiVao = 11,
    NguoiThanGui = 12,
    PhieuGuiQua = 13,
    NhanTuDoiTuongKhac = 14,
    MuaHang = 21,
    ChoTien = 22,
    ChuyenVeNguoiThan = 23,
    ChuyenTrai = 24,
    ChapHanhXongAn = 25,
}
