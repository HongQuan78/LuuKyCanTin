using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.Infrastructure.HeThong;

/// <summary>The workstation's sign-in state: one per process, set on sign-in and cleared on sign-out.</summary>
public sealed class CurrentUserSession : ICurrentUser
{
    private sealed record Phien(int NguoiDungId, string TenDangNhap);

    // One reference swap, so a background save never sees the id of one user with the name of another.
    private volatile Phien? _phien;

    public int? NguoiDungId => _phien?.NguoiDungId;

    public string? TenDangNhap => _phien?.TenDangNhap;

    public bool DaDangNhap => _phien is not null;

    public void DangNhap(int nguoiDungId, string tenDangNhap) => _phien = new Phien(nguoiDungId, tenDangNhap);

    public void DangXuat() => _phien = null;
}
