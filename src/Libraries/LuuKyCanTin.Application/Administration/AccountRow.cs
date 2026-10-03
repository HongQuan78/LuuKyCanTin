using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// One account with its officer's full name and roles, as the account screen and the last-administrator guard
/// read it. The user is untracked: callers that change an account load it again for update.
/// </summary>
public sealed record AccountRow(User User, string? OfficerFullName, IReadOnlyList<Role> Roles);
