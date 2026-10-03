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

    [Fact]
    public void Evaluate_AnyInput_ReturnsEveryRuleOnceInDisplayOrder()
    {
        PasswordPolicy.Evaluate("x", "y").Select(r => r.Rule).ShouldBe(
        [
            PasswordRule.MinLength,
            PasswordRule.UpperCase,
            PasswordRule.LowerCase,
            PasswordRule.Digit,
            PasswordRule.DifferentFromCurrent,
        ]);
    }

    [Fact]
    public void Evaluate_AStrongNewPassword_SatisfiesEveryRule()
    {
        PasswordPolicy.Evaluate("Moi@2026a", "LuuKy@2026").ShouldAllBe(r => r.IsSatisfied);
    }

    [Theory]
    [InlineData("Abc123", PasswordRule.MinLength)]
    [InlineData("abcdefg1", PasswordRule.UpperCase)]
    [InlineData("ABCDEFG1", PasswordRule.LowerCase)]
    [InlineData("Abcdefgh", PasswordRule.Digit)]
    [InlineData("LuuKy@2026", PasswordRule.DifferentFromCurrent)]
    public void Evaluate_APasswordBreakingOneRule_FailsOnlyThatRule(string newPassword, PasswordRule broken)
    {
        var results = PasswordPolicy.Evaluate(newPassword, "LuuKy@2026");

        results.Where(r => !r.IsSatisfied).Select(r => r.Rule).ShouldBe([broken]);
    }

    [Fact]
    public void Evaluate_EmptyInput_FailsEveryRule()
    {
        PasswordPolicy.Evaluate("", "").ShouldAllBe(r => !r.IsSatisfied);
    }

    [Fact]
    public void Evaluate_Null_FailsEveryStrengthRule()
    {
        PasswordPolicy.Evaluate(null, "LuuKy@2026")
            .Where(r => r.Rule != PasswordRule.DifferentFromCurrent)
            .ShouldAllBe(r => !r.IsSatisfied);
    }
}
