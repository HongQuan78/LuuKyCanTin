using LuuKyCanTin.Application.HeThong;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

public sealed class DemoSeedPolicyTests
{
    [Fact]
    public void KiemTra_DevelopmentWithoutConfirmation_IsAllowed()
    {
        DemoSeedPolicy.KiemTra(laMoiTruongPhatTrien: true, tenCoSoDuLieuXacNhan: null, tenCoSoDuLieuDich: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.Allowed);
    }

    [Fact]
    public void KiemTra_OutsideDevelopmentWithoutConfirmation_IsNotDevelopment()
    {
        DemoSeedPolicy.KiemTra(laMoiTruongPhatTrien: false, tenCoSoDuLieuXacNhan: null, tenCoSoDuLieuDich: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.NotDevelopment);
    }

    [Theory]
    [InlineData("", "LuuKyCanTin")]
    [InlineData("LuuKyCanTin_Test", "LuuKyCanTin")]
    // A connection string without Database= reports an empty name; a bare --force must not count as matching it.
    [InlineData("", "")]
    [InlineData("  ", "")]
    public void KiemTra_OutsideDevelopmentWrongOrEmptyConfirmation_IsDatabaseNameMismatch(string xacNhan, string dich)
    {
        DemoSeedPolicy.KiemTra(laMoiTruongPhatTrien: false, tenCoSoDuLieuXacNhan: xacNhan, tenCoSoDuLieuDich: dich)
            .ShouldBe(DemoSeedDecision.DatabaseNameMismatch);
    }

    [Theory]
    [InlineData("LuuKyCanTin")]
    [InlineData("luukycantin")]
    public void KiemTra_OutsideDevelopmentMatchingDatabaseName_IsAllowed(string xacNhan)
    {
        // SQL Server database names are case-insensitive.
        DemoSeedPolicy.KiemTra(laMoiTruongPhatTrien: false, tenCoSoDuLieuXacNhan: xacNhan, tenCoSoDuLieuDich: "LuuKyCanTin")
            .ShouldBe(DemoSeedDecision.Allowed);
    }
}
