namespace LuuKyCanTin.Domain.Administration;

/// <summary>Assigns one role to one account. A join row, not audited; account administration logs it explicitly.</summary>
public sealed class UserRole
{
    public int UserId { get; set; }

    public int RoleId { get; set; }
}
