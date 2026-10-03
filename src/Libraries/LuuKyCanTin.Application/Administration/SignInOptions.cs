namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// Sign-in settings bound from the "SignIn" section of appsettings.json. Workstations share one database, so
/// every machine has to use the same lock period.
/// </summary>
public sealed class SignInOptions
{
    public const string SectionName = "SignIn";

    /// <summary>Minutes an account stays locked after 5 failed attempts. 0 means only an administrator can unlock it.</summary>
    public int LockoutMinutes { get; set; } = 15;

    public TimeSpan? LockoutPeriod => LockoutMinutes <= 0 ? null : TimeSpan.FromMinutes(LockoutMinutes);
}
