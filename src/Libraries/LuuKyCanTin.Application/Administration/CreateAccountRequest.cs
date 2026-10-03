namespace LuuKyCanTin.Application.Administration;

/// <param name="OfficerId">An active staff member without an active account.</param>
/// <param name="RoleIds">At least one role from the fixed catalogue.</param>
public sealed record CreateAccountRequest(string UserName, int OfficerId, IReadOnlyCollection<int> RoleIds);
