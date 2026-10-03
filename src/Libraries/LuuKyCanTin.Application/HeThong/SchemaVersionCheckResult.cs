namespace LuuKyCanTin.Application.HeThong;

/// <param name="Expected">The last migration compiled into this build.</param>
/// <param name="Actual">The last migration applied to the database; null when it was never migrated or could not be read.</param>
public sealed record SchemaVersionCheckResult(string Expected, string? Actual, SchemaVersionStatus Status)
{
    public bool LaKhop => Status == SchemaVersionStatus.Matches;

    /// <summary>
    /// Matches only when the database has applied exactly the migrations compiled into this build. Comparing the last
    /// id alone would miss a merged migration whose timestamp sorts before one that is already applied.
    /// </summary>
    /// <param name="danhSachMigrationCuaBan">The build's migrations, oldest first; never empty.</param>
    /// <param name="danhSachMigrationDaApDung">The database's applied migrations, oldest first.</param>
    public static SchemaVersionCheckResult Tao(
        IReadOnlyList<string> danhSachMigrationCuaBan, IReadOnlyList<string> danhSachMigrationDaApDung)
    {
        var laKhop = danhSachMigrationCuaBan.ToHashSet(StringComparer.Ordinal).SetEquals(danhSachMigrationDaApDung);
        return new(
            danhSachMigrationCuaBan[^1],
            danhSachMigrationDaApDung.Count == 0 ? null : danhSachMigrationDaApDung[^1],
            laKhop ? SchemaVersionStatus.Matches : SchemaVersionStatus.Mismatch);
    }

    public static SchemaVersionCheckResult TaoLoiKetNoi(string expected) =>
        new(expected, null, SchemaVersionStatus.ConnectionFailed);
}
