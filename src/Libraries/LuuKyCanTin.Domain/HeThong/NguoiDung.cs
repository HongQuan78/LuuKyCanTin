using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.HeThong;

/// <summary>An account that can sign in. Lockout and the forced first change are FR1 rules.</summary>
public sealed class NguoiDung : AuditableEntity, IAuditable
{
    /// <summary>The spec fixes the threshold, so it is not a setting.</summary>
    public const byte SoLanSaiToiDa = 5;

    // A null lock period means "an administrator has to unlock it"; store a far-future date because KhoaDen is a date.
    private static readonly DateTime VoThoiHan = new(9999, 12, 31);

    public int Id { get; set; }

    public string TenDangNhap { get; set; } = "";

    /// <summary>Self-describing PBKDF2 string; never appears in the audit log JSON.</summary>
    [KhongGhiNhatKy]
    public string MatKhauHash { get; set; } = "";

    public bool DangHoatDong { get; set; } = true;

    /// <summary>True for the seeded admin and for every newly created or reset account.</summary>
    public bool PhaiDoiMatKhau { get; set; }

    public byte SoLanSai { get; set; }

    public DateTime? KhoaDen { get; set; }

    /// <summary>An expired lock is no lock: sign-in may try the password again.</summary>
    public bool DangBiKhoa(DateTime now) => KhoaDen is { } khoaDen && khoaDen > now;

    /// <summary>Counts one failed attempt and locks at the threshold.</summary>
    /// <param name="thoiGianKhoa">Null locks until an administrator unlocks it.</param>
    /// <returns>True when this attempt just locked the account.</returns>
    public bool GhiNhanDangNhapSai(DateTime now, TimeSpan? thoiGianKhoa)
    {
        SoLanSai++;
        if (SoLanSai < SoLanSaiToiDa)
            return false;

        KhoaDen = thoiGianKhoa is { } thoiGian ? now + thoiGian : VoThoiHan;
        return true;
    }

    public void GhiNhanDangNhapDung()
    {
        SoLanSai = 0;
        KhoaDen = null;
    }

    public void DoiMatKhau(string hashMoi)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hashMoi);

        MatKhauHash = hashMoi;
        PhaiDoiMatKhau = false;
    }
}
