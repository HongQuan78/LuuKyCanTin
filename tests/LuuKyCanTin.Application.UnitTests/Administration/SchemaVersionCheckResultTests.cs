using LuuKyCanTin.Application.Administration;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

public sealed class SchemaVersionCheckResultTests
{
    private const string Initial = "20261001000000_InitialCreate";
    private const string Expected = "20261005000000_AddCanBo";

    [Fact]
    public void Create_SameMigrations_Matches()
    {
        var result = SchemaVersionCheckResult.Create([Initial, Expected], [Initial, Expected]);

        result.Status.ShouldBe(SchemaVersionStatus.Matches);
        result.IsMatch.ShouldBeTrue();
        result.Expected.ShouldBe(Expected);
        result.Actual.ShouldBe(Expected);
    }

    [Fact]
    public void Create_DatabaseBehind_IsMismatch()
    {
        var result = SchemaVersionCheckResult.Create([Initial, Expected], [Initial]);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.IsMatch.ShouldBeFalse();
        result.Expected.ShouldBe(Expected);
        result.Actual.ShouldBe(Initial);
    }

    [Fact]
    public void Create_NeverMigratedDatabase_IsMismatch()
    {
        var result = SchemaVersionCheckResult.Create([Initial], []);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBeNull();
    }

    [Fact]
    public void Create_DatabaseMigratedByNewerBuild_IsMismatch()
    {
        SchemaVersionCheckResult.Create([Initial], [Initial, Expected]).IsMatch.ShouldBeFalse();
    }

    [Fact]
    public void Create_MergedMigrationWithEarlierTimestampNotApplied_IsMismatch()
    {
        // A migration merged from another branch sorts before one already applied: the last ids agree, the sets don't.
        const string merged = "20261003000000_AddDoiTuong";

        var result = SchemaVersionCheckResult.Create([Initial, merged, Expected], [Initial, Expected]);

        result.Actual.ShouldBe(result.Expected);
        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
    }

    [Fact]
    public void Create_MigrationDiffersOnlyInCase_IsMismatch()
    {
        // Migration ids are compiled identifiers; a differently cased id is a different migration.
        SchemaVersionCheckResult.Create([Initial], [Initial.ToUpperInvariant()]).IsMatch.ShouldBeFalse();
    }

    [Fact]
    public void CreateConnectionFailed_AnyMigration_IsNeitherMatchNorMismatch()
    {
        var result = SchemaVersionCheckResult.CreateConnectionFailed(Expected);

        result.Status.ShouldBe(SchemaVersionStatus.ConnectionFailed);
        result.IsMatch.ShouldBeFalse();
        result.Actual.ShouldBeNull();
    }
}
