using System.Globalization;
using System.Security.Cryptography;
using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.Infrastructure.HeThong;

/// <summary>
/// Self-describing PBKDF2-SHA256 hashes: <c>PBKDF2-SHA256$&lt;iterations&gt;$&lt;saltB64&gt;$&lt;hashB64&gt;</c>.
/// Iterations live in the string so they can be raised later without invalidating old hashes.
/// </summary>
internal sealed class Pbkdf2MatKhauHasher : IMatKhauHasher
{
    private const string TienTo = "PBKDF2-SHA256";
    private const int SoVongLap = 600_000;

    // Current OWASP guidance for PBKDF2-SHA256; changing it only affects new hashes.
    private const int DoDaiSalt = 16;
    private const int DoDaiHash = 32;

    // A tampered hash must not be able to force an arbitrarily expensive computation.
    private const int SoVongLapToiDa = 1_000_000;

    public string Hash(string matKhau)
    {
        var salt = RandomNumberGenerator.GetBytes(DoDaiSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(matKhau, salt, SoVongLap, HashAlgorithmName.SHA256, DoDaiHash);
        return $"{TienTo}${SoVongLap}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string matKhau, string hash)
    {
        var phan = hash.Split('$');
        if (phan.Length != 4 || phan[0] != TienTo)
            return false;

        if (!int.TryParse(phan[1], CultureInfo.InvariantCulture, out var soVongLap)
            || soVongLap is < 1 or > SoVongLapToiDa)
            return false;

        byte[] salt;
        byte[] mongDoi;
        try
        {
            salt = Convert.FromBase64String(phan[2]);
            mongDoi = Convert.FromBase64String(phan[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length == 0 || mongDoi.Length == 0)
            return false;

        var tinhDuoc = Rfc2898DeriveBytes.Pbkdf2(matKhau, salt, soVongLap, HashAlgorithmName.SHA256, mongDoi.Length);
        return CryptographicOperations.FixedTimeEquals(tinhDuoc, mongDoi);
    }
}
