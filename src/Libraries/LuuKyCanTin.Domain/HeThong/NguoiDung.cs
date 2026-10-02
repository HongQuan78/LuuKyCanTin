using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.HeThong;

/// <summary>An account that can sign in. Lockout and the password policy arrive in Epic 2.</summary>
public sealed class NguoiDung : AuditableEntity
{
    public int Id { get; set; }

    public string TenDangNhap { get; set; } = "";

    /// <summary>Self-describing PBKDF2 string; never appears in the audit log JSON.</summary>
    [KhongGhiNhatKy]
    public string MatKhauHash { get; set; } = "";

    public bool DangHoatDong { get; set; } = true;

    public byte SoLanSai { get; set; }

    public DateTime? KhoaDen { get; set; }
}
