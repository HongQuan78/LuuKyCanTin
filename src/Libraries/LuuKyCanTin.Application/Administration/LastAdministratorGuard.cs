using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// Refuses any change that would leave the system without an active administrator, where "administrator" means
/// an active account holding <see cref="PermissionCodes.Administration.Update"/> through one of its roles. The
/// caller opens the transaction (Serializable for the guarded writes), so the check and the change are one
/// atomic unit.
/// </summary>
public sealed class LastAdministratorGuard(IAppDbContext db, IUserStore userStore)
{
    public const string LastAdministratorRequiredMessage =
        "Không thể thực hiện: hệ thống phải còn ít nhất một tài khoản quản trị đang hoạt động.";

    /// <summary>An account and its role ids as the guard reads them before a change.</summary>
    public sealed record AccountState(int UserId, bool IsActive, IReadOnlyCollection<int> RoleIds);

    /// <summary>
    /// Pure decision: after the change, does at least one active account still hold HT.Sua? Unit-tested without
    /// a database, because the whole guard rests on it.
    /// </summary>
    public static bool AnyAdministratorRemains(
        IReadOnlyCollection<AccountState> accounts,
        IReadOnlyDictionary<int, IReadOnlyCollection<string>> permissionsByRole) =>
        accounts.Any(account => account.IsActive && account.RoleIds.Any(roleId =>
            permissionsByRole.TryGetValue(roleId, out var codes)
            && codes.Contains(PermissionCodes.Administration.Update, StringComparer.Ordinal)));

    /// <exception cref="BusinessRuleException">Deactivating the account would leave no administrator.</exception>
    public async Task EnsureDeactivationAllowedAsync(int userId, CancellationToken ct = default)
    {
        var (accounts, permissionsByRole) = await LoadAsync(ct);
        var after = accounts.Select(a => a.UserId == userId ? a with { IsActive = false } : a).ToList();
        Ensure(AnyAdministratorRemains(after, permissionsByRole));
    }

    /// <exception cref="BusinessRuleException">The new role set would leave no administrator.</exception>
    public async Task EnsureRoleChangeAllowedAsync(
        int userId, IReadOnlyCollection<int> newRoleIds, CancellationToken ct = default)
    {
        var (accounts, permissionsByRole) = await LoadAsync(ct);
        var after = accounts.Select(a => a.UserId == userId ? a with { RoleIds = newRoleIds } : a).ToList();
        Ensure(AnyAdministratorRemains(after, permissionsByRole));
    }

    /// <exception cref="BusinessRuleException">The new permission set would leave no administrator.</exception>
    public async Task EnsureRolePermissionChangeAllowedAsync(
        int roleId, IReadOnlyCollection<string> newPermissionCodes, CancellationToken ct = default)
    {
        var (accounts, permissionsByRole) = await LoadAsync(ct);
        var after = permissionsByRole.ToDictionary(
            pair => pair.Key,
            pair => pair.Key == roleId ? newPermissionCodes : pair.Value);
        Ensure(AnyAdministratorRemains(accounts, after));
    }

    private static void Ensure(bool remains)
    {
        if (!remains)
            throw new BusinessRuleException(LastAdministratorRequiredMessage);
    }

    private async Task<(
        List<AccountState> Accounts,
        Dictionary<int, IReadOnlyCollection<string>> PermissionsByRole)> LoadAsync(CancellationToken ct)
    {
        // Accounts without a role still matter for the "active" count, so their role list is simply empty.
        var accountRows = await userStore.GetAllWithRolesAsync(ct);
        var accounts = accountRows
            .Select(row => new AccountState(row.User.Id, row.User.IsActive, row.Roles.Select(role => role.Id).ToList()))
            .ToList();

        var permissionsByRole = (await (
                from role in db.Role.AsNoTracking()
                join rolePermission in db.RolePermission.AsNoTracking() on role.Id equals rolePermission.RoleId
                join permission in db.Permission.AsNoTracking() on rolePermission.PermissionId equals permission.Id
                select new { role.Id, permission.Code }).ToListAsync(ct))
            .GroupBy(pair => pair.Id)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<string>)group.Select(pair => pair.Code).ToList());

        return (accounts, permissionsByRole);
    }
}
