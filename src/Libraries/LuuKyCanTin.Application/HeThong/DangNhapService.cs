using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.HeThong;

/// <summary>
/// Sign-in with lockout and the forced first change (FR1). Returns a result instead of throwing for every
/// credential outcome; only unexpected infrastructure failures escape.
/// </summary>
public sealed class DangNhapService(
    INguoiDungStore nguoiDungStore,
    IMatKhauHasher matKhauHasher,
    ICurrentUserSession phien,
    IGhiNhatKy ghiNhatKy,
    IClock clock,
    GhiNhanDangNhapSaiService ghiNhanSai)
{
    public const string SaiThongTin = "Tên đăng nhập hoặc mật khẩu không đúng.";
    public const string LoiTaiKhoanBiKhoa = "Tài khoản đã bị khoá, liên hệ quản trị viên.";

    // A parseable PBKDF2 string that can never match a password. Verifying it for an unknown user keeps the
    // response time close to a wrong password, so the form reveals whether an account exists (AC 2).
    private const string MatKhauGiaHash =
        "PBKDF2-SHA256$600000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    public async Task<KetQuaDangNhap> DangNhapAsync(string tenDangNhap, string matKhau, CancellationToken ct = default)
    {
        var nguoiDung = await nguoiDungStore.TimTheoDangNhapAsync(tenDangNhap.Trim(), ct);
        if (nguoiDung is null)
        {
            matKhauHasher.Verify(matKhau, MatKhauGiaHash);
            return KetQuaDangNhap.Loi(TrangThaiDangNhap.SaiThongTin, SaiThongTin);
        }

        // A locked or inactive account is refused before the password is checked, so nobody can keep testing
        // passwords against it. Both get the locked message, so a deactivated person learns nothing new.
        if (!nguoiDung.DangHoatDong)
            return KetQuaDangNhap.Loi(TrangThaiDangNhap.TaiKhoanNgungHoatDong, LoiTaiKhoanBiKhoa);
        if (nguoiDung.DangBiKhoa(clock.Now))
            return KetQuaDangNhap.Loi(TrangThaiDangNhap.TaiKhoanBiKhoa, LoiTaiKhoanBiKhoa);

        if (!matKhauHasher.Verify(matKhau, nguoiDung.MatKhauHash))
        {
            return await ghiNhanSai.GhiNhanAsync(nguoiDung, ct)
                ? KetQuaDangNhap.Loi(TrangThaiDangNhap.TaiKhoanBiKhoa, LoiTaiKhoanBiKhoa)
                : KetQuaDangNhap.Loi(TrangThaiDangNhap.SaiThongTin, SaiThongTin);
        }

        // The session is set even when a change is forced: the shell stays closed until the change succeeds.
        nguoiDung.GhiNhanDangNhapDung();
        await nguoiDungStore.LuuAsync(nguoiDung, ct);

        // The built-in admin has no staff record; everywhere else HoTen is never null, so the fallback is rare.
        var hoTen = nguoiDung.CanBoId is { } canBoId ? await nguoiDungStore.LayHoTenCanBoAsync(canBoId, ct) : null;
        phien.DangNhap(nguoiDung.Id, nguoiDung.TenDangNhap, nguoiDung.CanBoId, hoTen ?? nguoiDung.TenDangNhap);
        await ghiNhatKy.GhiAsync(
            HanhDong.DangNhap, "NguoiDung", nguoiDung.Id,
            new { SuKien = SuKienDangNhap.DangNhap, TenDangNhap = nguoiDung.TenDangNhap }, ct);

        return nguoiDung.PhaiDoiMatKhau ? KetQuaDangNhap.DoiMatKhau() : KetQuaDangNhap.Ok();
    }

    /// <summary>Writes the sign-out event and clears the session; does nothing when nobody is signed in.</summary>
    public async Task DangXuatAsync(CancellationToken ct = default)
    {
        if (phien.NguoiDungId is not { } nguoiDungId)
            return;

        await ghiNhatKy.GhiAsync(
            HanhDong.DangNhap, "NguoiDung", nguoiDungId,
            new { SuKien = SuKienDangNhap.DangXuat, TenDangNhap = phien.TenDangNhap }, ct);
        phien.DangXuat();
    }
}
