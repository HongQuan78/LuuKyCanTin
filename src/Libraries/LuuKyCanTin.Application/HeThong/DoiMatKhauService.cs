using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.HeThong;

/// <summary>Changes the password of the signed-in user (FR1). The current password is always required.</summary>
public sealed class DoiMatKhauService(
    INguoiDungStore nguoiDungStore,
    IMatKhauHasher matKhauHasher,
    ICurrentUser currentUser,
    IGhiNhatKy ghiNhatKy,
    GhiNhanDangNhapSaiService ghiNhanSai)
{
    public const string LoiChuaDangNhap = "Bạn chưa đăng nhập.";
    public const string LoiKhongTimThayTaiKhoan = "Không tìm thấy tài khoản.";

    /// <exception cref="LoiNghiepVuException">A wrong current password, a weak new password or a mismatch.</exception>
    public async Task DoiMatKhauAsync(string matKhauHienTai, string matKhauMoi, string xacNhan, CancellationToken ct = default)
    {
        if (currentUser.NguoiDungId is not { } nguoiDungId)
            throw new LoiNghiepVuException(LoiChuaDangNhap);

        var nguoiDung = await nguoiDungStore.TimTheoIdAsync(nguoiDungId, ct)
            ?? throw new LoiNghiepVuException(LoiKhongTimThayTaiKhoan);

        if (!matKhauHasher.Verify(matKhauHienTai, nguoiDung.MatKhauHash))
        {
            // A wrong current password counts toward lockout, exactly as a wrong password at sign-in.
            var vuaKhoa = await ghiNhanSai.GhiNhanAsync(nguoiDung, ct);
            throw new LoiNghiepVuException(vuaKhoa ? DangNhapService.LoiTaiKhoanBiKhoa : DangNhapService.SaiThongTin);
        }

        var viPham = ChinhSachMatKhau.KiemTra(matKhauMoi);
        if (viPham.Count > 0)
            throw new LoiNghiepVuException(string.Join('\n', viPham));
        if (string.Equals(matKhauMoi, matKhauHienTai, StringComparison.Ordinal))
            throw new LoiNghiepVuException(ChinhSachMatKhau.LoiTrungMatKhauCu);
        if (!string.Equals(matKhauMoi, xacNhan, StringComparison.Ordinal))
            throw new LoiNghiepVuException(ChinhSachMatKhau.LoiXacNhanKhongKhop);

        nguoiDung.DoiMatKhau(matKhauHasher.Hash(matKhauMoi));
        await nguoiDungStore.LuuAsync(nguoiDung, ct);
        await ghiNhatKy.GhiAsync(
            HanhDong.DangNhap, "NguoiDung", nguoiDung.Id,
            new { SuKien = SuKienDangNhap.DoiMatKhau, TenDangNhap = nguoiDung.TenDangNhap }, ct);
    }
}
