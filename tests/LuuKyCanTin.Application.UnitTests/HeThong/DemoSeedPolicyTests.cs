using LuuKyCanTin.Application.HeThong;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

public class DemoSeedPolicyTests
{
    [Fact]
    public void Development_IsAllowedWithoutConfirmation()
    {
        DemoSeedPolicy.Decide(isDevelopment: true, confirmedDatabaseName: null, targetDatabaseName: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.Allowed);
    }

    [Fact]
    public void OutsideDevelopment_WithoutConfirmation_IsRefused()
    {
        DemoSeedPolicy.Decide(isDevelopment: false, confirmedDatabaseName: null, targetDatabaseName: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.NotDevelopment);
    }

    [Theory]
    [InlineData("")]
    [InlineData("LuuKyCanTin_Test")]
    public void OutsideDevelopment_WithWrongDatabaseName_IsRefused(string confirmed)
    {
        DemoSeedPolicy.Decide(isDevelopment: false, confirmedDatabaseName: confirmed, targetDatabaseName: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.DatabaseNameMismatch);
    }

    [Theory]
    [InlineData("LuuKyCanTin")]
    [InlineData("luukycantin")]
    public void OutsideDevelopment_WithMatchingDatabaseName_IsAllowed(string confirmed)
    {
        // SQL Server database names are case-insensitive.
        DemoSeedPolicy.Decide(isDevelopment: false, confirmedDatabaseName: confirmed, targetDatabaseName: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.Allowed);
    }
}
