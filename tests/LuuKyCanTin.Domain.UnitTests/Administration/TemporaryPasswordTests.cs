using LuuKyCanTin.Domain.Administration;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.Administration;

public class TemporaryPasswordTests
{
    // The look-alike characters a temporary password read from a screen must never contain.
    private static readonly char[] LookAlikeCharacters = ['0', 'O', 'o', '1', 'l', 'I'];

    [Fact]
    public void Generate_HasTheFixedLength()
    {
        TemporaryPassword.Generate().Length.ShouldBe(TemporaryPassword.Length);
    }

    [Fact]
    public void Generate_AlwaysPassesThePasswordPolicy()
    {
        for (var round = 0; round < 1_000; round++)
            PasswordPolicy.Validate(TemporaryPassword.Generate()).ShouldBeEmpty();
    }

    [Fact]
    public void Generate_ContainsNoLookAlikeCharacter()
    {
        for (var round = 0; round < 1_000; round++)
            TemporaryPassword.Generate().ShouldAllBe(character => !LookAlikeCharacters.Contains(character));
    }

    [Fact]
    public void Generate_TwoCallsDiffer()
    {
        TemporaryPassword.Generate().ShouldNotBe(TemporaryPassword.Generate());
    }
}
