using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.HeThong;

/// <summary>
/// Thin sign-in for the walking skeleton: verify the password, set the session, log the event. Lockout,
/// the password policy and the forced change at first login are Epic 2 (FR1).
/// </summary>
public sealed class DangNhapService(
    INguoiDungStore nguoiDungStore,
    IMatKhauHasher matKhauHasher,
    ICurrentUserSession phien,
    IGhiNhatKy ghiNhatKy)
{
    public const string SaiThongTin = "Tên đăng nhập hoặc mật khẩu không đúng.";

    public async Task<KetQuaDangNhap> DangNhapAsync(string tenDangNhap, string matKhau, CancellationToken ct = default)
    {
        var nguoiDung = await nguoiDungStore.TimTheoDangNhapAsync(tenDangNhap.Trim(), ct);

        // The message never says which part was wrong, and an inactive account gets the same one.
        if (nguoiDung is null || !nguoiDung.DangHoatDong || !matKhauHasher.Verify(matKhau, nguoiDung.MatKhauHash))
            return KetQuaDangNhap.Loi(SaiThongTin);

        phien.DangNhap(nguoiDung.Id, nguoiDung.TenDangNhap);
        await ghiNhatKy.GhiAsync(
            HanhDong.DangNhap, "NguoiDung", nguoiDung.Id, new { nguoiDung.TenDangNhap }, ct);

        return KetQuaDangNhap.Ok();
    }
}
