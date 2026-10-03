namespace LuuKyCanTin.Application.Administration;

/// <summary>The stored audit codes of <see cref="SessionLockKind"/>, matching the rows existing logs already hold.</summary>
public static class SessionLockKindExtensions
{
    public static string ToCode(this SessionLockKind kind) => kind switch
    {
        SessionLockKind.Automatic => "TuDong",
        SessionLockKind.Manual => "ThuCong",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown session lock kind."),
    };
}
