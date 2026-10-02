using LuuKyCanTin.Application.DanhMuc;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.DanhMuc;

public class LuuCanBoRequestValidatorTests
{
    private readonly LuuCanBoRequestValidator _validator = new();

    private static LuuCanBoRequest Request(string maCanBo = "CB-01", string hoTen = "Nguyễn Văn An", string? chucVu = null) =>
        new(maCanBo, hoTen, chucVu, LaQuanGiao: false);

    private IEnumerable<string> Errors(LuuCanBoRequest request) =>
        _validator.Validate(request).Errors.Select(e => e.ErrorMessage);

    [Theory]
    [InlineData("CB01")]
    [InlineData("cb_01-a")]
    [InlineData("  CB01  ")]
    [InlineData("12345678901234567890")]
    public void ValidCode_Passes(string maCanBo)
    {
        _validator.Validate(Request(maCanBo)).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankCode_IsRejected(string maCanBo)
    {
        Errors(Request(maCanBo)).ShouldBe(["Mã cán bộ không được để trống."]);
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
    public void CodeWithOtherCharacters_IsRejected(string maCanBo)
    {
        Errors(Request(maCanBo)).ShouldBe(["Mã cán bộ chỉ gồm chữ không dấu, chữ số, dấu '-' hoặc '_'."]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void BlankName_IsRejected(string hoTen)
    {
        Errors(Request(hoTen: hoTen)).ShouldBe(["Họ tên không được để trống."]);
    }

    [Fact]
    public void NameLongerThan100_IsRejected()
    {
        Errors(Request(hoTen: new string('a', 101))).ShouldBe(["Họ tên tối đa 100 ký tự."]);
    }

    [Fact]
    public void NameOf100AfterTrimming_Passes()
    {
        _validator.Validate(Request(hoTen: $" {new string('a', 100)} ")).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void PositionLongerThan100_IsRejected()
    {
        Errors(Request(chucVu: new string('a', 101))).ShouldBe(["Chức vụ tối đa 100 ký tự."]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Cán bộ quản giáo")]
    public void OptionalPosition_Passes(string? chucVu)
    {
        _validator.Validate(Request(chucVu: chucVu)).IsValid.ShouldBeTrue();
    }
}
