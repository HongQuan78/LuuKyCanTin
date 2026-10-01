namespace LuuKyCanTin.Application.Abstractions;

/// <summary>The user signed in on this workstation, if any.</summary>
public interface ICurrentUser
{
    int? NguoiDungId { get; }

    string? TenDangNhap { get; }

    bool DaDangNhap { get; }
}
