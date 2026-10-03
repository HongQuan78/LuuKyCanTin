using LuuKyCanTin.Domain.MasterData;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.MasterData;

public class OfficerTests
{
    [Fact]
    public void New_TrimsInput_UpperCasesTheCode_AndIsActive()
    {
        var officer = new Officer("  cb-01 ", " Nguyễn Văn An  ", " Cán bộ quản giáo ", isSupervisingOfficer: true);

        officer.OfficerCode.ShouldBe("CB-01");
        officer.FullName.ShouldBe("Nguyễn Văn An");
        officer.Position.ShouldBe("Cán bộ quản giáo");
        officer.IsSupervisingOfficer.ShouldBeTrue();
        officer.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankPosition_IsStoredAsNull(string? position)
    {
        new Officer("CB01", "Lê Thị Bình", position, isSupervisingOfficer: false).Position.ShouldBeNull();
    }

    [Theory]
    [InlineData("", "Lê Thị Bình")]
    [InlineData("CB01", "  ")]
    public void BlankCodeOrName_IsRejected(string officerCode, string fullName)
    {
        Should.Throw<ArgumentException>(() => new Officer(officerCode, fullName, null, isSupervisingOfficer: false));
    }

    [Fact]
    public void Update_NormalizesLikeTheConstructor()
    {
        var officer = new Officer("CB01", "Lê Thị Bình", null, isSupervisingOfficer: false);

        officer.Update(" cb02 ", " Lê Thị Bình Minh ", "  ", isSupervisingOfficer: true);

        officer.OfficerCode.ShouldBe("CB02");
        officer.FullName.ShouldBe("Lê Thị Bình Minh");
        officer.Position.ShouldBeNull();
        officer.IsSupervisingOfficer.ShouldBeTrue();
    }

    [Fact]
    public void IsActive_CanBeTurnedOffAndBackOn()
    {
        var officer = new Officer("CB01", "Lê Thị Bình", null, isSupervisingOfficer: false) { IsActive = false };
        officer.IsActive.ShouldBeFalse();

        officer.IsActive = true;
        officer.IsActive.ShouldBeTrue();
    }
}
