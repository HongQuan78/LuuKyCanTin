using System.Globalization;
using System.Security.Cryptography;
using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.Infrastructure.Administration;

/// <summary>
/// Self-describing PBKDF2-SHA256 hashes: <c>PBKDF2-SHA256$&lt;iterations&gt;$&lt;saltB64&gt;$&lt;hashB64&gt;</c>.
/// Iterations live in the string so they can be raised later without invalidating old hashes.
/// </summary>
internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "PBKDF2-SHA256";
    private const int Iterations = 600_000;

    // Current OWASP guidance for PBKDF2-SHA256; changing it only affects new hashes.
    private const int SaltLength = 16;
    private const int HashLength = 32;

    // A tampered hash must not be able to force an arbitrarily expensive computation.
    private const int MaxIterations = 1_000_000;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashLength);
        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix)
            return false;

        if (!int.TryParse(parts[1], CultureInfo.InvariantCulture, out var iterations)
            || iterations is < 1 or > MaxIterations)
            return false;

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length == 0 || expected.Length == 0)
            return false;

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
