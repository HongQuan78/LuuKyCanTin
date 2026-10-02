using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.HeThong;

/// <summary>
/// Records one failed password attempt: counts it toward lockout and writes the audit event. Shared by sign-in
/// and by change-password, because a wrong current password counts as a failed attempt too.
/// </summary>
public sealed class GhiNhanDangNhapSaiService(
    INguoiDungStore nguoiDungStore,
    IClock clock,
    DangNhapOptions options,
    IGhiNhatKy ghiNhatKy)
{
    /// <returns>True when this attempt just locked the account.</returns>
    public async Task<bool> GhiNhanAsync(NguoiDung nguoiDung, CancellationToken ct = default)
    {
        var vuaKhoa = nguoiDung.GhiNhanDangNhapSai(clock.Now, options.ThoiGianKhoa);
        try
        {
            await nguoiDungStore.LuuAsync(nguoiDung, ct);
        }
        catch (XungDotDuLieuException)
        {
            // Two workstations failed the same account at once. Reload and re-apply this attempt once, so it is
            // never lost; a second conflict still reaches the caller.
            await nguoiDungStore.TaiLaiAsync(nguoiDung, ct);
            vuaKhoa = nguoiDung.GhiNhanDangNhapSai(clock.Now, options.ThoiGianKhoa);
            await nguoiDungStore.LuuAsync(nguoiDung, ct);
        }

        var suKien = vuaKhoa ? SuKienDangNhap.KhoaTaiKhoan : SuKienDangNhap.DangNhapSai;
        await ghiNhatKy.GhiAsync(
            HanhDong.DangNhap, "NguoiDung", nguoiDung.Id,
            new { SuKien = suKien, TenDangNhap = nguoiDung.TenDangNhap }, ct);

        return vuaKhoa;
    }
}
