namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// Grants one permission to one role. A join row: permission edits insert and delete rows, so it is deliberately
/// not <see cref="Common.IAuditable"/>, because the audit interceptor refuses deleted audited rows. The role
/// service writes one explicit audit row for the whole before/after set instead.
/// </summary>
public sealed class RolePermission
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }
}
