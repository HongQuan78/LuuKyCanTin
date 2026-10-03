namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// Session settings bound from the "Session" section of appsettings.json. The idle lock is per workstation, so every
/// machine may use its own value; <c>0</c> turns auto-lock off in Development only.
/// </summary>
public sealed class SessionOptions
{
    public const string SectionName = "Session";

    /// <summary>Minutes without keyboard or mouse input before the session locks. 0 turns auto-lock off.</summary>
    public int IdleLockMinutes { get; set; } = 5;

    /// <summary>
    /// The idle timeout, or null when auto-lock is off. A value outside 1–60 yields no timer, so options built
    /// outside DI can never produce a negative or absurd interval.
    /// </summary>
    public TimeSpan? IdleTimeout =>
        IdleLockMinutes is >= 1 and <= 60 ? TimeSpan.FromMinutes(IdleLockMinutes) : null;

    /// <summary>Valid values are 1–60 minutes; <c>0</c> (off) is allowed in Development only.</summary>
    public static bool IsValidIdleLockMinutes(int minutes, bool isDevelopment) =>
        minutes is >= 1 and <= 60 || (isDevelopment && minutes == 0);
}
