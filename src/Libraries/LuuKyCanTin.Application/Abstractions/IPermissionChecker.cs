using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// Checks a permission against the database on every call, never a cache, so a revoked permission or a
/// deactivated account stops working immediately.
/// </summary>
public interface IPermissionChecker
{
    /// <exception cref="PermissionDeniedException">Nobody is signed in, or the current user lacks the permission.</exception>
    Task RequireAsync(string permissionCode, CancellationToken ct = default);
}
