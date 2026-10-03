using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.Administration;

public sealed class RoleService(
    IAppDbContext db,
    IPermissionChecker permissionChecker,
    IAuditLogWriter auditLog,
    LastAdministratorGuard lastAdministratorGuard) : IRoleService
{
    public const string RoleNotFoundMessage = "Không tìm thấy vai trò.";
    public const string InvalidPermissionCodeMessage = "Mã quyền không hợp lệ: ";

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default) =>
        await db.Role
            .AsNoTracking()
            .OrderBy(v => v.Id)
            .Select(v => new RoleDto(v.Id, v.Code, v.Name, v.RowVer))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(int roleId, CancellationToken ct = default) =>
        await (from rolePermission in db.RolePermission
               join permission in db.Permission on rolePermission.PermissionId equals permission.Id
               where rolePermission.RoleId == roleId
               orderby permission.Id
               select permission.Code).ToListAsync(ct);

    public async Task UpdatePermissionsAsync(
        int roleId, IReadOnlyCollection<string> permissionCode, byte[] rowVer, CancellationToken ct = default)
    {
        // Authorization first, before any read or transaction, so a refused call writes nothing at all.
        await permissionChecker.RequireAsync(PermissionCodes.Administration.Update, ct);

        // The catalogue check is in memory, so it may run before the transaction.
        var invalidCodes = permissionCode.Where(code => PermissionCodes.All.All(q => q.Code != code)).ToList();
        if (invalidCodes.Count > 0)
            throw new BusinessRuleException(InvalidPermissionCodeMessage + string.Join(", ", invalidCodes));

        // Serializable: the last-administrator guard and the write must be one atomic unit, so two administrators
        // cannot remove each other's administration permission at the same moment. The rows involved are few.
        await using var transaction = await db.BeginTransactionAsync(TransactionIsolation.Serializable, ct);

        var role = await db.Role.SingleOrDefaultAsync(v => v.Id == roleId, ct)
            ?? throw new BusinessRuleException(RoleNotFoundMessage);

        var current = await db.RolePermission.Where(v => v.RoleId == roleId).ToListAsync(ct);
        var codeById = await db.Permission.AsNoTracking().ToDictionaryAsync(q => q.Id, q => q.Code, ct);
        var oldCodes = current.Select(v => codeById[v.PermissionId]).Order(StringComparer.Ordinal).ToList();
        var newCodes = permissionCode.ToHashSet(StringComparer.Ordinal);
        var idByCode = PermissionCodes.All.ToDictionary(q => q.Code, q => q.Id, StringComparer.Ordinal);

        await lastAdministratorGuard.EnsureRolePermissionChangeAllowedAsync(
            roleId, newCodes.Order(StringComparer.Ordinal).ToList(), ct);

        db.RolePermission.RemoveRange(current.Where(v => !newCodes.Contains(codeById[v.PermissionId])));
        foreach (var code in newCodes.Where(code => current.All(v => codeById[v.PermissionId] != code)))
            db.RolePermission.Add(new RolePermission { RoleId = roleId, PermissionId = idByCode[code] });

        // Touch the role so its row version changes: two administrators editing the same role then conflict.
        // Flag only a bookkeeping column; marking the whole entity Modified looks like a detached Update() to the
        // audit interceptor, which refuses it.
        var entry = db.Entry(role);
        entry.Property(v => v.RowVer).OriginalValue = rowVer;
        entry.Property(v => v.ModifiedAt).IsModified = true;

        await db.SaveChangesAsync(ct);
        await auditLog.WriteAsync(
            AuditAction.Update, "Role", roleId,
            new { Permissions = oldCodes },
            new { Permissions = newCodes.Order(StringComparer.Ordinal).ToList() }, ct);
        await transaction.CommitAsync(ct);
    }
}
