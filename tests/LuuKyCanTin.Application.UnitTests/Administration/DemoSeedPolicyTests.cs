using LuuKyCanTin.Application.Administration;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

public sealed class DemoSeedPolicyTests
{
    [Fact]
    public void Decide_DevelopmentWithoutConfirmation_IsAllowed()
    {
        DemoSeedPolicy.Decide(isDevelopment: true, confirmedDatabaseName: null, targetDatabaseName: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.Allowed);
    }

    [Fact]
    public void Decide_OutsideDevelopmentWithoutConfirmation_IsNotDevelopment()
    {
        DemoSeedPolicy.Decide(isDevelopment: false, confirmedDatabaseName: null, targetDatabaseName: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.NotDevelopment);
    }

    [Theory]
    [InlineData("", "LuuKyCanTin")]
    [InlineData("LuuKyCanTin_Test", "LuuKyCanTin")]
    // A connection string without Database= reports an empty name; a bare --force must not count as matching it.
    [InlineData("", "")]
    [InlineData("  ", "")]
    public void Decide_OutsideDevelopmentWrongOrEmptyConfirmation_IsDatabaseNameMismatch(string confirmation, string target)
    {
        DemoSeedPolicy.Decide(isDevelopment: false, confirmedDatabaseName: confirmation, targetDatabaseName: target)
            .ShouldBe(DemoSeedDecision.DatabaseNameMismatch);
    }

    [Theory]
    [InlineData("LuuKyCanTin")]
    [InlineData("luukycantin")]
    public void Decide_OutsideDevelopmentMatchingDatabaseName_IsAllowed(string confirmation)
    {
        // SQL Server database names are case-insensitive.
        DemoSeedPolicy.Decide(isDevelopment: false, confirmedDatabaseName: confirmation, targetDatabaseName: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.Allowed);
    }
}
