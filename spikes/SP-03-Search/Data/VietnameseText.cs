using System.Globalization;
using System.Text;

namespace SP03Search.Data;

internal static class VietnameseText
{
    /// <summary>
    /// NFC, trim, collapse runs of whitespace to one space. Mandatory before a LIKE,
    /// because text pasted from Word can arrive as NFD.
    /// </summary>
    public static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormC);
        var builder = new StringBuilder(normalized.Length);
        var pendingSpace = false;
        foreach (var ch in normalized)
        {
            if (char.IsWhiteSpace(ch))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(ch);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Diacritic-stripped, đ→d, in NFC. This is what the Application layer writes into the
    /// persisted search column on save (option (a) in Story 1.7).
    /// </summary>
    public static string RemoveDiacritics(string? value)
    {
        var source = NormalizeForSearch(value);
        if (source.Length == 0)
        {
            return string.Empty;
        }

        var decomposed = source.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category is UnicodeCategory.NonSpacingMark
                or UnicodeCategory.SpacingCombiningMark
                or UnicodeCategory.EnclosingMark)
            {
                continue;
            }

            builder.Append(ch switch
            {
                'đ' => 'd',
                'Đ' => 'D',
                _ => ch,
            });
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string EscapeLike(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal)
        .Replace("[", "\\[", StringComparison.Ordinal);

    public static string ToNfd(string value) => value.Normalize(NormalizationForm.FormD);
}
