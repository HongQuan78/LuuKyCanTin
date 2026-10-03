namespace LuuKyCanTin.Application.Abstractions;

/// <summary>The user signed in on this workstation, if any.</summary>
public interface ICurrentUser
{
    int? NguoiDungId { get; }

    string? TenDangNhap { get; }

    /// <summary>The staff member behind the account; null for the built-in admin.</summary>
    int? CanBoId { get; }

    /// <summary>The staff name shown on screen; falls back to the sign-in name for the built-in admin.</summary>
    string? HoTen { get; }

    bool DaDangNhap { get; }
}
