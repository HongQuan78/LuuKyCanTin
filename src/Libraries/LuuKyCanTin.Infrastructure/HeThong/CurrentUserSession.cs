using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.Infrastructure.HeThong;

/// <summary>The workstation's sign-in state: one per process, set on sign-in and cleared on sign-out.</summary>
public sealed class CurrentUserSession : ICurrentUserSession
{
    private sealed record Phien(int NguoiDungId, string TenDangNhap, int? CanBoId, string? HoTen);

    // One reference swap, so a background save never sees the id of one user with the name of another.
    private volatile Phien? _phien;

    public int? NguoiDungId => _phien?.NguoiDungId;

    public string? TenDangNhap => _phien?.TenDangNhap;

    public int? CanBoId => _phien?.CanBoId;

    public string? HoTen => _phien?.HoTen;

    public bool DaDangNhap => _phien is not null;

    public void DangNhap(int nguoiDungId, string tenDangNhap, int? canBoId, string? hoTen) =>
        _phien = new Phien(nguoiDungId, tenDangNhap, canBoId, hoTen);

    public void DangXuat() => _phien = null;
}
