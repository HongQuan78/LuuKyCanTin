using System.Collections.Frozen;
using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.Infrastructure.Administration;

/// <summary>The workstation's sign-in state: one per process, set on sign-in and cleared on sign-out.</summary>
public sealed class CurrentUserSession : ICurrentUserSession
{
    private sealed record Session(
        int UserId,
        string UserName,
        int? OfficerId,
        string? FullName,
        FrozenSet<string> PermissionCodes);

    // One reference swap, so a background save never sees the id of one user with the name of another.
    private volatile Session? _session;

    public int? UserId => _session?.UserId;

    public string? UserName => _session?.UserName;

    public int? OfficerId => _session?.OfficerId;

    public string? FullName => _session?.FullName;

    public bool IsSignedIn => _session is not null;

    public bool HasPermission(string permissionCode) =>
        _session?.PermissionCodes.Contains(permissionCode) == true;

    public void SignIn(int userId, string userName, int? officerId, string? fullName, IReadOnlyCollection<string> permissionCodes) =>
        // Frozen once, so the snapshot is immutable and one reference swap publishes it.
        _session = new Session(userId, userName, officerId, fullName, permissionCodes.ToFrozenSet(StringComparer.Ordinal));

    public void SignOut() => _session = null;
}
