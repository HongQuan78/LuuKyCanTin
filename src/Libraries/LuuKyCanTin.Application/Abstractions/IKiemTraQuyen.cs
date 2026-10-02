using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// Checks a permission against the database on every call, never a cache, so a revoked permission or a
/// deactivated account stops working immediately.
/// </summary>
public interface IKiemTraQuyen
{
    /// <exception cref="KhongCoQuyenException">Nobody is signed in, or the current user lacks the permission.</exception>
    Task YeuCauAsync(string maQuyen, CancellationToken ct = default);
}
