namespace LuuKyCanTin.Application.HeThong;

/// <summary>One account row of the "Tài khoản" screen, with the role names already joined for the grid.</summary>
/// <param name="TenVaiTro">The role names in catalogue order, comma-separated; empty when the account has no role.</param>
/// <param name="VaiTroIds">Role ids for the role dialog, in catalogue order.</param>
/// <param name="DangBiKhoa">True while the lock has not expired.</param>
public sealed record TaiKhoanDto(
    int Id,
    string TenDangNhap,
    string? HoTenCanBo,
    string TenVaiTro,
    IReadOnlyList<int> VaiTroIds,
    bool DangHoatDong,
    bool DangBiKhoa,
    bool PhaiDoiMatKhau);
