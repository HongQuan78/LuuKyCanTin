using System.Security.Cryptography;

namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// Generates the temporary password an administrator hands to a new or reset account. It always satisfies
/// <see cref="PasswordPolicy"/> and avoids look-alike characters (0/O, 1/l/I) so it can be read aloud or
/// copied from a screen without mistakes. A pure rule: no dependency outside the framework.
/// </summary>
public static class TemporaryPassword
{
    public const int Length = 12;

    private const string UpperCase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string LowerCase = "abcdefghijkmnpqrstuvwxyz";
    private const string Digits = "23456789";
    private const string AllCharacters = UpperCase + LowerCase + Digits;

    /// <summary>A 12-character password with at least one upper-case letter, one lower-case letter and one digit.</summary>
    public static string Generate()
    {
        // One character from each required class first, so the policy holds regardless of the fill below.
        var characters = new char[Length];
        characters[0] = PickRandom(UpperCase);
        characters[1] = PickRandom(LowerCase);
        characters[2] = PickRandom(Digits);
        for (var i = 3; i < Length; i++)
            characters[i] = PickRandom(AllCharacters);

        Shuffle(characters);
        return new string(characters);
    }

    private static char PickRandom(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];

    // Fisher-Yates with the cryptographic generator, so the position of the required classes is not predictable.
    private static void Shuffle(char[] characters)
    {
        for (var i = characters.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (characters[i], characters[j]) = (characters[j], characters[i]);
        }
    }
}
