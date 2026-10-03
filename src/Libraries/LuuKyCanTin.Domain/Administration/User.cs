using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.Administration;

/// <summary>An account that can sign in. Lockout and the forced first change are FR1 rules.</summary>
public sealed class User : AuditableEntity, IAuditable
{
    /// <summary>The spec fixes the threshold, so it is not a setting.</summary>
    public const byte MaxFailedAttempts = 5;

    // A null lock period means "an administrator has to unlock it"; store a far-future date because LockedUntil is a date.
    private static readonly DateTime NoExpiry = new(9999, 12, 31);

    public int Id { get; set; }

    public string UserName { get; set; } = "";

    /// <summary>Self-describing PBKDF2 string; never appears in the audit log JSON.</summary>
    [NotAudited]
    public string PasswordHash { get; set; } = "";

    public bool IsActive { get; set; } = true;

    /// <summary>True for the seeded admin and for every newly created or reset account.</summary>
    public bool MustChangePassword { get; set; }

    public byte FailedAttemptCount { get; set; }

    public DateTime? LockedUntil { get; set; }

    /// <summary>An expired lock is no lock: sign-in may try the password again.</summary>
    public bool IsLocked(DateTime now) => LockedUntil is { } lockedUntil && lockedUntil > now;

    /// <summary>Counts one failed attempt and locks at the threshold.</summary>
    /// <param name="lockPeriod">Null locks until an administrator unlocks it.</param>
    /// <returns>True when this attempt just locked the account.</returns>
    public bool RecordFailedSignIn(DateTime now, TimeSpan? lockPeriod)
    {
        FailedAttemptCount++;
        if (FailedAttemptCount < MaxFailedAttempts)
            return false;

        LockedUntil = lockPeriod is { } period ? now + period : NoExpiry;
        return true;
    }

    public void RecordSuccessfulSignIn()
    {
        FailedAttemptCount = 0;
        LockedUntil = null;
    }

    public void ChangePassword(string newHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newHash);

        PasswordHash = newHash;
        MustChangePassword = false;
    }
}
