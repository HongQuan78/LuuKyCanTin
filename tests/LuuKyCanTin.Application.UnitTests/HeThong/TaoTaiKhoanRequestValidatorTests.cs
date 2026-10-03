using LuuKyCanTin.Application.HeThong;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

public class TaoTaiKhoanRequestValidatorTests
{
    private static readonly TaoTaiKhoanRequestValidator Validator = new();

    private static TaoTaiKhoanRequest Request(
        string tenDangNhap = "thuquy01", int canBoId = 1, IReadOnlyCollection<int>? vaiTroIds = null) =>
        new(tenDangNhap, canBoId, vaiTroIds ?? [1]);

    [Theory]
    [InlineData("thuquy01")]
    [InlineData("nguyen.van_a")]
    [InlineData("abc")]
    [InlineData("A1_2.3")]
    public void ValidSignInName_IsAccepted(string tenDangNhap)
    {
        Validator.Validate(Request(tenDangNhap)).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("thu quy")]
    [InlineData("thuỷ")]
    [InlineData("thu-quy")]
    public void InvalidSignInName_IsRejected(string tenDangNhap)
    {
        Validator.Validate(Request(tenDangNhap)).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void TooLongSignInName_IsRejected()
    {
        Validator.Validate(Request(new string('a', TaoTaiKhoanRequestValidator.DoDaiTenDangNhapToiDa + 1)))
            .IsValid.ShouldBeFalse();
    }

    [Fact]
    public void WithoutAStaffMember_IsRejected()
    {
        Validator.Validate(Request(canBoId: 0)).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void WithoutARole_IsRejected()
    {
        Validator.Validate(Request(vaiTroIds: [])).IsValid.ShouldBeFalse();
    }
}
