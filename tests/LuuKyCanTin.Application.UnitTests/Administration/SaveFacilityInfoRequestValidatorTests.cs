using LuuKyCanTin.Application.Administration;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

public class SaveFacilityInfoRequestValidatorTests
{
    private static readonly byte[] RowVer = [1, 2, 3];

    private readonly SaveFacilityInfoRequestValidator _validator = new();

    private static SaveFacilityInfoRequest Request(
        string? parentAgencyName = null,
        string facilityName = "Trại tạm giam ABC",
        string address = "Xã ABC, huyện ABC, tỉnh ABC") =>
        new(parentAgencyName, facilityName, address, RowVer);

    private IEnumerable<string> Errors(SaveFacilityInfoRequest request) =>
        _validator.Validate(request).Errors.Select(e => e.ErrorMessage);

    private IReadOnlyList<string> FailureProperties(SaveFacilityInfoRequest request) =>
        _validator.Validate(request).Errors.Select(e => e.PropertyName).ToList();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankFacilityName_IsRejected(string facilityName)
    {
        Errors(Request(facilityName: facilityName)).ShouldBe([SaveFacilityInfoRequestValidator.FacilityNameRequiredMessage]);
    }

    [Fact]
    public void FacilityNameLongerThan200_IsRejected()
    {
        Errors(Request(facilityName: new string('a', 201)))
            .ShouldBe([SaveFacilityInfoRequestValidator.FacilityNameMaxLengthMessage]);
    }

    [Fact]
    public void FacilityNameOf200AfterTrimming_Passes()
    {
        _validator.Validate(Request(facilityName: $" {new string('a', 200)} ")).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankAddress_IsRejected(string address)
    {
        Errors(Request(address: address)).ShouldBe([SaveFacilityInfoRequestValidator.AddressRequiredMessage]);
    }

    [Fact]
    public void AddressLongerThan300_IsRejected()
    {
        Errors(Request(address: new string('a', 301)))
            .ShouldBe([SaveFacilityInfoRequestValidator.AddressMaxLengthMessage]);
    }

    [Fact]
    public void AddressOf300AfterTrimming_Passes()
    {
        _validator.Validate(Request(address: $" {new string('a', 300)} ")).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ParentAgencyNameLongerThan200_IsRejected()
    {
        Errors(Request(parentAgencyName: new string('a', 201)))
            .ShouldBe([SaveFacilityInfoRequestValidator.ParentAgencyNameMaxLengthMessage]);
    }

    [Fact]
    public void ParentAgencyNameOf200_Passes()
    {
        _validator.Validate(Request(parentAgencyName: new string('a', 200))).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Công an tỉnh ABC")]
    public void OptionalParentAgencyName_Passes(string? parentAgencyName)
    {
        _validator.Validate(Request(parentAgencyName: parentAgencyName)).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Failures_CarryTheRequestPropertyName()
    {
        FailureProperties(Request(parentAgencyName: new string('a', 201), facilityName: "", address: "   "))
            .ShouldBe(
            [
                nameof(SaveFacilityInfoRequest.ParentAgencyName),
                nameof(SaveFacilityInfoRequest.FacilityName),
                nameof(SaveFacilityInfoRequest.Address),
            ]);
    }
}
