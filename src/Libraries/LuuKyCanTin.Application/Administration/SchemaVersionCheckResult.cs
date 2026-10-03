namespace LuuKyCanTin.Application.Administration;

/// <param name="Expected">The last migration compiled into this build.</param>
/// <param name="Actual">The last migration applied to the database; null when it was never migrated or could not be read.</param>
public sealed record SchemaVersionCheckResult(string Expected, string? Actual, SchemaVersionStatus Status)
{
    public bool IsMatch => Status == SchemaVersionStatus.Matches;

    /// <summary>
    /// Matches only when the database has applied exactly the migrations compiled into this build. Comparing the last
    /// id alone would miss a merged migration whose timestamp sorts before one that is already applied.
    /// </summary>
    /// <param name="buildMigrations">The build's migrations, oldest first; never empty.</param>
    /// <param name="appliedMigrations">The database's applied migrations, oldest first.</param>
    public static SchemaVersionCheckResult Create(
        IReadOnlyList<string> buildMigrations, IReadOnlyList<string> appliedMigrations)
    {
        var isMatch = buildMigrations.ToHashSet(StringComparer.Ordinal).SetEquals(appliedMigrations);
        return new(
            buildMigrations[^1],
            appliedMigrations.Count == 0 ? null : appliedMigrations[^1],
            isMatch ? SchemaVersionStatus.Matches : SchemaVersionStatus.Mismatch);
    }

    public static SchemaVersionCheckResult CreateConnectionFailed(string expected) =>
        new(expected, null, SchemaVersionStatus.ConnectionFailed);
}
