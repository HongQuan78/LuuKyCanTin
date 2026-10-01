namespace LuuKyCanTin.Application.HeThong;

public enum SchemaVersionStatus
{
    Matches,
    Mismatch,
    ConnectionFailed,
}

/// <param name="Expected">The last migration compiled into this build.</param>
/// <param name="Actual">The last migration applied to the database; null when it was never migrated or could not be read.</param>
public sealed record SchemaVersionCheckResult(string Expected, string? Actual, SchemaVersionStatus Status)
{
    public bool Matches => Status == SchemaVersionStatus.Matches;

    public static SchemaVersionCheckResult Compare(string expected, string? actual) =>
        new(expected, actual, string.Equals(expected, actual, StringComparison.Ordinal)
            ? SchemaVersionStatus.Matches
            : SchemaVersionStatus.Mismatch);

    public static SchemaVersionCheckResult ConnectionFailed(string expected) =>
        new(expected, null, SchemaVersionStatus.ConnectionFailed);
}
