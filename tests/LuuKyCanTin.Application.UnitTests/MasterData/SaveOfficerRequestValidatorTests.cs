using LuuKyCanTin.Application.MasterData;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.MasterData;

public class SaveOfficerRequestValidatorTests
{
    private readonly SaveOfficerRequestValidator _validator = new();

    private static SaveOfficerRequest Request(string officerCode = "CB-01", string fullName = "Nguyễn Văn An", string? position = null) =>
        new(officerCode, fullName, position, IsSupervisingOfficer: false);

    private IEnumerable<string> Errors(SaveOfficerRequest request) =>
        _validator.Validate(request).Errors.Select(e => e.ErrorMessage);

    [Theory]
    [InlineData("CB01")]
    [InlineData("cb_01-a")]
    [InlineData("  CB01  ")]
    [InlineData("12345678901234567890")]
    public void ValidCode_Passes(string officerCode)
    {
        _validator.Validate(Request(officerCode)).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankCode_IsRejected(string officerCode)
    {
        Errors(Request(officerCode)).ShouldBe(["Mã cán bộ không được để trống."]);
    }

    [Fact]
    public void CodeLongerThan20_IsRejected()
    {
        Errors(Request(new string('A', 21))).ShouldBe(["Mã cán bộ tối đa 20 ký tự."]);
    }

    [Theory]
    [InlineData("CB 01")]
    [InlineData("CBĐ01")]
    [InlineData("CB.01")]
    [InlineData("CBé")]
    public void CodeWithOtherCharacters_IsRejected(string officerCode)
    {
        Errors(Request(officerCode)).ShouldBe(["Mã cán bộ chỉ gồm chữ không dấu, chữ số, dấu '-' hoặc '_'."]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void BlankName_IsRejected(string fullName)
    {
        Errors(Request(fullName: fullName)).ShouldBe(["Họ tên không được để trống."]);
    }

    [Fact]
    public void NameLongerThan100_IsRejected()
    {
        Errors(Request(fullName: new string('a', 101))).ShouldBe(["Họ tên tối đa 100 ký tự."]);
    }

    [Fact]
    public void NameOf100AfterTrimming_Passes()
    {
        _validator.Validate(Request(fullName: $" {new string('a', 100)} ")).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void PositionLongerThan100_IsRejected()
    {
        Errors(Request(position: new string('a', 101))).ShouldBe(["Chức vụ tối đa 100 ký tự."]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Cán bộ quản giáo")]
    public void OptionalPosition_Passes(string? position)
    {
        _validator.Validate(Request(position: position)).IsValid.ShouldBeTrue();
    }
}
