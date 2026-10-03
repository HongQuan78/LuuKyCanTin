using LuuKyCanTin.Domain.Administration;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.Administration;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Abc123")]          // 6 characters: too short
    [InlineData("Abcdefgh")]        // no digit
    [InlineData("abcdefg1")]        // no upper-case letter
    [InlineData("ABCDEFG1")]        // no lower-case letter
    public void Validate_APasswordBreakingOneRule_ReturnsThatViolation(string password)
    {
        PasswordPolicy.Validate(password).ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("Abcdef12")]
    [InlineData("ĐổiMới2026")]      // Đ is upper-case, ổ/ớ are lower-case
    [InlineData("Đăc biệt 1")]      // Vietnamese letters count through char.IsUpper/IsLower
    public void Validate_AStrongPassword_IsAccepted(string password)
    {
        PasswordPolicy.Validate(password).ShouldBeEmpty();
    }

    [Fact]
    public void Validate_Null_ReportsEveryViolation()
    {
        PasswordPolicy.Validate(null).Count.ShouldBe(4);
    }
}
