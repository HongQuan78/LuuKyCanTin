namespace LuuKyCanTin.Application.Administration;

/// <summary>One account row of the "Tài khoản" screen, with the role names already joined for the grid.</summary>
/// <param name="RoleNames">The role names in catalogue order, comma-separated; empty when the account has no role.</param>
/// <param name="RoleIds">Role ids for the role dialog, in catalogue order.</param>
/// <param name="IsLocked">True while the lock has not expired.</param>
public sealed record AccountDto(
    int Id,
    string UserName,
    string? OfficerFullName,
    string RoleNames,
    IReadOnlyList<int> RoleIds,
    bool IsActive,
    bool IsLocked,
    bool MustChangePassword);
