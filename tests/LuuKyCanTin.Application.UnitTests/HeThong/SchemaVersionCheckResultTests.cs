using LuuKyCanTin.Application.HeThong;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

public sealed class SchemaVersionCheckResultTests
{
    private const string Initial = "20261001000000_InitialCreate";
    private const string Expected = "20261005000000_AddCanBo";

    [Fact]
    public void Tao_SameMigrations_Matches()
    {
        var result = SchemaVersionCheckResult.Tao([Initial, Expected], [Initial, Expected]);

        result.Status.ShouldBe(SchemaVersionStatus.Matches);
        result.LaKhop.ShouldBeTrue();
        result.Expected.ShouldBe(Expected);
        result.Actual.ShouldBe(Expected);
    }

    [Fact]
    public void Tao_DatabaseBehind_IsMismatch()
    {
        var result = SchemaVersionCheckResult.Tao([Initial, Expected], [Initial]);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.LaKhop.ShouldBeFalse();
        result.Expected.ShouldBe(Expected);
        result.Actual.ShouldBe(Initial);
    }

    [Fact]
    public void Tao_NeverMigratedDatabase_IsMismatch()
    {
        var result = SchemaVersionCheckResult.Tao([Initial], []);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBeNull();
    }

    [Fact]
    public void Tao_DatabaseMigratedByNewerBuild_IsMismatch()
    {
        SchemaVersionCheckResult.Tao([Initial], [Initial, Expected]).LaKhop.ShouldBeFalse();
    }

    [Fact]
    public void Tao_MergedMigrationWithEarlierTimestampNotApplied_IsMismatch()
    {
        // A migration merged from another branch sorts before one already applied: the last ids agree, the sets don't.
        const string merged = "20261003000000_AddDoiTuong";

        var result = SchemaVersionCheckResult.Tao([Initial, merged, Expected], [Initial, Expected]);

        result.Actual.ShouldBe(result.Expected);
        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
    }

    [Fact]
    public void Tao_MigrationDiffersOnlyInCase_IsMismatch()
    {
        // Migration ids are compiled identifiers; a differently cased id is a different migration.
        SchemaVersionCheckResult.Tao([Initial], [Initial.ToUpperInvariant()]).LaKhop.ShouldBeFalse();
    }

    [Fact]
    public void TaoLoiKetNoi_AnyMigration_IsNeitherMatchNorMismatch()
    {
        var result = SchemaVersionCheckResult.TaoLoiKetNoi(Expected);

        result.Status.ShouldBe(SchemaVersionStatus.ConnectionFailed);
        result.LaKhop.ShouldBeFalse();
        result.Actual.ShouldBeNull();
    }
}
