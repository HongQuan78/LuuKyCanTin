using LuuKyCanTin.Application.HeThong;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

public class SchemaVersionCheckResultTests
{
    private const string Expected = "20261001000000_InitialCreate";

    [Fact]
    public void Compare_SameMigration_Matches()
    {
        var result = SchemaVersionCheckResult.Compare(Expected, Expected);

        result.Status.ShouldBe(SchemaVersionStatus.Matches);
        result.Matches.ShouldBeTrue();
    }

    [Fact]
    public void Compare_DifferentMigration_IsMismatch()
    {
        var result = SchemaVersionCheckResult.Compare(Expected, "20260901000000_Older");

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Matches.ShouldBeFalse();
        result.Expected.ShouldBe(Expected);
        result.Actual.ShouldBe("20260901000000_Older");
    }

    [Fact]
    public void Compare_NeverMigratedDatabase_IsMismatch()
    {
        var result = SchemaVersionCheckResult.Compare(Expected, null);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBeNull();
    }

    [Fact]
    public void Compare_IsCaseSensitive()
    {
        // Migration ids are compiled identifiers; a differently cased id is a different migration.
        SchemaVersionCheckResult.Compare(Expected, Expected.ToUpperInvariant()).Matches.ShouldBeFalse();
    }

    [Fact]
    public void ConnectionFailed_IsNeitherMatchNorMismatch()
    {
        var result = SchemaVersionCheckResult.ConnectionFailed(Expected);

        result.Status.ShouldBe(SchemaVersionStatus.ConnectionFailed);
        result.Matches.ShouldBeFalse();
        result.Actual.ShouldBeNull();
    }
}
