using LuuKyCanTin.Application.Administration;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

public class CreateAccountRequestValidatorTests
{
    private static readonly CreateAccountRequestValidator Validator = new();

    private static CreateAccountRequest Request(
        string userName = "thuquy01", int officerId = 1, IReadOnlyCollection<int>? roleIds = null) =>
        new(userName, officerId, roleIds ?? [1]);

    [Theory]
    [InlineData("thuquy01")]
    [InlineData("nguyen.van_a")]
    [InlineData("abc")]
    [InlineData("A1_2.3")]
    public void ValidSignInName_IsAccepted(string userName)
    {
        Validator.Validate(Request(userName)).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("thu quy")]
    [InlineData("thuỷ")]
    [InlineData("thu-quy")]
    public void InvalidSignInName_IsRejected(string userName)
    {
        Validator.Validate(Request(userName)).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void TooLongSignInName_IsRejected()
    {
        Validator.Validate(Request(new string('a', CreateAccountRequestValidator.MaxUserNameLength + 1)))
            .IsValid.ShouldBeFalse();
    }

    [Fact]
    public void WithoutAStaffMember_IsRejected()
    {
        Validator.Validate(Request(officerId: 0)).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void WithoutARole_IsRejected()
    {
        Validator.Validate(Request(roleIds: [])).IsValid.ShouldBeFalse();
    }
}
