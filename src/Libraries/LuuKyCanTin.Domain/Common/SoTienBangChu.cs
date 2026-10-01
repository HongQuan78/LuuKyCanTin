namespace LuuKyCanTin.Domain.Common;

/// <summary>
/// Writes an amount of whole đồng in Vietnamese words, as printed on receipts ("Năm trăm nghìn đồng").
/// The result is stored with the posted document, so a reprint never depends on this code staying the same.
/// </summary>
public static class SoTienBangChu
{
    private const decimal MotTy = 1_000_000_000m;

    private static readonly string[] ChuSo = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];

    // The 3-digit groups below one tỷ, most significant first; "tỷ" itself repeats (nghìn tỷ, triệu tỷ, tỷ tỷ).
    private static readonly (int ChiaCho, string DonVi)[] NhomDuoiTy = [(1_000_000, "triệu"), (1_000, "nghìn"), (1, "")];

    public static string Doc(decimal soTien, KieuDocLe kieu = KieuDocLe.Le)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(soTien);
        if (soTien != decimal.Truncate(soTien))
        {
            throw new ArgumentOutOfRangeException(nameof(soTien), soTien, "Số tiền phải là số đồng nguyên.");
        }

        if (soTien == 0)
        {
            return "Không đồng";
        }

        var tu = new List<string>();
        DocSo(soTien, dauTien: true, kieu, tu);
        tu.Add("đồng");

        var ketQua = string.Join(' ', tu);
        return char.ToUpperInvariant(ketQua[0]) + ketQua[1..];
    }

    /// <param name="dauTien">True when this is the most significant part of the whole amount, which reads without leading zeros.</param>
    private static void DocSo(decimal so, bool dauTien, KieuDocLe kieu, List<string> tu)
    {
        if (so >= MotTy)
        {
            // Remainder first, then an exact division, so huge decimals never round.
            var duoiTy = so % MotTy;
            DocSo((so - duoiTy) / MotTy, dauTien, kieu, tu);
            tu.Add("tỷ");
            if (duoiTy > 0)
            {
                DocDuoiMotTy((int)duoiTy, dauTien: false, kieu, tu);
            }

            return;
        }

        DocDuoiMotTy((int)so, dauTien, kieu, tu);
    }

    private static void DocDuoiMotTy(int so, bool dauTien, KieuDocLe kieu, List<string> tu)
    {
        foreach (var (chiaCho, donVi) in NhomDuoiTy)
        {
            var nhom = so / chiaCho % 1000;
            if (nhom == 0)
            {
                continue;
            }

            DocNhom(nhom, docDuTram: !dauTien, kieu, tu);
            if (donVi.Length > 0)
            {
                tu.Add(donVi);
            }

            dauTien = false;
        }
    }

    /// <param name="docDuTram">Read the hundreds even when zero ("không trăm"); true for every group but the leading one.</param>
    private static void DocNhom(int nhom, bool docDuTram, KieuDocLe kieu, List<string> tu)
    {
        var tram = nhom / 100;
        var chuc = nhom / 10 % 10;
        var donVi = nhom % 10;
        var coTram = docDuTram || tram > 0;

        if (coTram)
        {
            tu.Add(ChuSo[tram]);
            tu.Add("trăm");
        }

        switch (chuc)
        {
            case 0:
                if (donVi > 0)
                {
                    if (coTram)
                    {
                        tu.Add(kieu == KieuDocLe.Linh ? "linh" : "lẻ");
                    }

                    tu.Add(ChuSo[donVi]);
                }

                return;
            case 1:
                tu.Add("mười");
                break;
            default:
                tu.Add(ChuSo[chuc]);
                tu.Add("mươi");
                break;
        }

        switch (donVi)
        {
            case 0:
                break;
            case 1 when chuc >= 2:
                tu.Add("mốt");
                break;
            case 5:
                tu.Add("lăm");
                break;
            default:
                tu.Add(ChuSo[donVi]);
                break;
        }
    }
}
