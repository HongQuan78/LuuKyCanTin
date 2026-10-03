using System.Security.Cryptography;

namespace LuuKyCanTin.Domain.HeThong;

/// <summary>
/// Generates the temporary password an administrator hands to a new or reset account. It always satisfies
/// <see cref="ChinhSachMatKhau"/> and avoids look-alike characters (0/O, 1/l/I) so it can be read aloud or
/// copied from a screen without mistakes. A pure rule: no dependency outside the framework.
/// </summary>
public static class MatKhauTam
{
    public const int DoDai = 12;

    private const string ChuHoa = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string ChuThuong = "abcdefghijkmnpqrstuvwxyz";
    private const string ChuSo = "23456789";
    private const string TatCa = ChuHoa + ChuThuong + ChuSo;

    /// <summary>A 12-character password with at least one upper-case letter, one lower-case letter and one digit.</summary>
    public static string Tao()
    {
        // One character from each required class first, so the policy holds regardless of the fill below.
        var kyTu = new char[DoDai];
        kyTu[0] = LayNgauNhien(ChuHoa);
        kyTu[1] = LayNgauNhien(ChuThuong);
        kyTu[2] = LayNgauNhien(ChuSo);
        for (var i = 3; i < DoDai; i++)
            kyTu[i] = LayNgauNhien(TatCa);

        XaoTron(kyTu);
        return new string(kyTu);
    }

    private static char LayNgauNhien(string nguon) => nguon[RandomNumberGenerator.GetInt32(nguon.Length)];

    // Fisher-Yates with the cryptographic generator, so the position of the required classes is not predictable.
    private static void XaoTron(char[] kyTu)
    {
        for (var i = kyTu.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (kyTu[i], kyTu[j]) = (kyTu[j], kyTu[i]);
        }
    }
}
