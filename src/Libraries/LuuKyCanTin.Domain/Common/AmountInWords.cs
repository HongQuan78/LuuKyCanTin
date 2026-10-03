namespace LuuKyCanTin.Domain.Common;

/// <summary>
/// Writes an amount of whole đồng in Vietnamese words, as printed on receipts ("Năm trăm nghìn đồng").
/// The result is stored with the posted document, so a reprint never depends on this code staying the same.
/// </summary>
public static class AmountInWords
{
    private const decimal OneBillion = 1_000_000_000m;

    private static readonly string[] DigitWords = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];

    // The 3-digit groups below one tỷ, most significant first; "tỷ" itself repeats (nghìn tỷ, triệu tỷ, tỷ tỷ).
    private static readonly (int Divisor, string Unit)[] GroupsBelowOneBillion = [(1_000_000, "triệu"), (1_000, "nghìn"), (1, "")];

    public static string ToWords(decimal amount, ZeroTensStyle style = ZeroTensStyle.Southern)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (amount != decimal.Truncate(amount))
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Số tiền phải là số đồng nguyên.");
        }

        if (amount == 0)
        {
            return "Không đồng";
        }

        var words = new List<string>();
        AppendNumber(amount, isLeading: true, style, words);
        words.Add("đồng");

        var result = string.Join(' ', words);
        return char.ToUpperInvariant(result[0]) + result[1..];
    }

    /// <param name="isLeading">True when this is the most significant part of the whole amount, which reads without leading zeros.</param>
    private static void AppendNumber(decimal number, bool isLeading, ZeroTensStyle style, List<string> words)
    {
        if (number >= OneBillion)
        {
            // Remainder first, then an exact division, so huge decimals never round.
            var belowOneBillion = number % OneBillion;
            AppendNumber((number - belowOneBillion) / OneBillion, isLeading, style, words);
            words.Add("tỷ");
            if (belowOneBillion > 0)
            {
                AppendBelowOneBillion((int)belowOneBillion, isLeading: false, style, words);
            }

            return;
        }

        AppendBelowOneBillion((int)number, isLeading, style, words);
    }

    private static void AppendBelowOneBillion(int number, bool isLeading, ZeroTensStyle style, List<string> words)
    {
        foreach (var (divisor, unit) in GroupsBelowOneBillion)
        {
            var group = number / divisor % 1000;
            if (group == 0)
            {
                continue;
            }

            AppendGroup(group, mustReadHundreds: !isLeading, style, words);
            if (unit.Length > 0)
            {
                words.Add(unit);
            }

            isLeading = false;
        }
    }

    /// <param name="mustReadHundreds">Read the hundreds even when zero ("không trăm"); true for every group but the leading one.</param>
    private static void AppendGroup(int group, bool mustReadHundreds, ZeroTensStyle style, List<string> words)
    {
        var hundreds = group / 100;
        var tens = group / 10 % 10;
        var units = group % 10;
        var hasHundreds = mustReadHundreds || hundreds > 0;

        if (hasHundreds)
        {
            words.Add(DigitWords[hundreds]);
            words.Add("trăm");
        }

        switch (tens)
        {
            case 0:
                if (units > 0)
                {
                    if (hasHundreds)
                    {
                        words.Add(style == ZeroTensStyle.Northern ? "linh" : "lẻ");
                    }

                    words.Add(DigitWords[units]);
                }

                return;
            case 1:
                words.Add("mười");
                break;
            default:
                words.Add(DigitWords[tens]);
                words.Add("mươi");
                break;
        }

        switch (units)
        {
            case 0:
                break;
            case 1 when tens >= 2:
                words.Add("mốt");
                break;
            case 5:
                words.Add("lăm");
                break;
            default:
                words.Add(DigitWords[units]);
                break;
        }
    }
}
