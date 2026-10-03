namespace LuuKyCanTin.Application.Abstractions;

/// <summary>The user signed in on this workstation, if any.</summary>
public interface ICurrentUser
{
    int? UserId { get; }

    string? UserName { get; }

    /// <summary>The staff member linked to the account, or null for the built-in admin.</summary>
    int? OfficerId { get; }

    /// <summary>The officer's full name, falling back to the sign-in name for the built-in admin.</summary>
    string? FullName { get; }

    bool IsSignedIn { get; }

    /// <summary>
    /// True when the signed-in user's roles granted the permission at sign-in. <b>For the UI only</b>: menu
    /// entries and button states. A service that writes must call <see cref="IPermissionChecker"/>, which reads
    /// the database, so a revoked permission stops working immediately even when this cached answer is stale.
    /// </summary>
    bool HasPermission(string permissionCode);
}
