namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// What an audit-log row records. Stored as a short text code (varchar), so the log stays readable in plain SQL; the
/// codes are fixed data that existing log rows already hold, so they never follow a rename of these members.
/// </summary>
public enum AuditAction : byte
{
    Create = 1,
    Update = 2,
    Cancel = 3,
    Print = 4,
    Approve = 5,
    SignIn = 6,
}
