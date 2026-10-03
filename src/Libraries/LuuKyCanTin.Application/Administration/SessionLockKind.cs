namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// Why a session locked. The audit log stores each value's fixed code through
/// <see cref="SessionLockKindExtensions.ToCode"/>, so the codes are data and never follow a rename of these members.
/// </summary>
public enum SessionLockKind : byte
{
    Automatic = 1,
    Manual = 2,
}
