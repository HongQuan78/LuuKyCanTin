namespace LuuKyCanTin.Domain.HeThong;

/// <summary>What an audit-log row records. Stored as its name (varchar), so the log stays readable in plain SQL.</summary>
public enum HanhDong : byte
{
    Them = 1,
    Sua = 2,
    Huy = 3,
    In = 4,
    Duyet = 5,
    DangNhap = 6,
}
