using LuuKyCanTin.Application.Common;

namespace LuuKyCanTin.Application.Administration;

/// <summary>The role list and the permission set of one role. Roles are fixed; only their grants change.</summary>
public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>The permission codes granted to the role, in catalogue order.</summary>
    Task<IReadOnlyList<string>> GetPermissionsAsync(int roleId, CancellationToken ct = default);

    /// <summary>Replaces the role's permission set and writes one audit row with the before/after lists.</summary>
    /// <exception cref="BusinessRuleException">The role doesn't exist, or a code is not in the catalogue.</exception>
    /// <exception cref="ConcurrencyConflictException">Someone else changed the role after <paramref name="rowVer"/> was loaded.</exception>
    /// <exception cref="Domain.Administration.PermissionDeniedException">The current user lacks PermissionCodes.Administration.Update.</exception>
    Task UpdatePermissionsAsync(int roleId, IReadOnlyCollection<string> permissionCode, byte[] rowVer, CancellationToken ct = default);
}
