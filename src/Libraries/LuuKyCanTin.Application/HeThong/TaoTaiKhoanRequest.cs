namespace LuuKyCanTin.Application.HeThong;

/// <param name="CanBoId">An active staff member without an active account.</param>
/// <param name="VaiTroIds">At least one role from the fixed catalogue.</param>
public sealed record TaoTaiKhoanRequest(string TenDangNhap, int CanBoId, IReadOnlyCollection<int> VaiTroIds);
