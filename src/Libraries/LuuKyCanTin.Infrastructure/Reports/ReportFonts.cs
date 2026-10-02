using System.Reflection;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace LuuKyCanTin.Infrastructure.Reports;

/// <summary>
/// Registers the embedded Tinos fonts once per process (Apache 2.0, metric-compatible with Times New Roman).
/// The product never depends on fonts installed on the workstation; a missing glyph stops generation instead
/// of silently rendering a fallback.
/// </summary>
internal static class ReportFonts
{
    public const string Family = "Tinos";

    private const string ResourcePrefix = "LuuKyCanTin.Infrastructure.Reports.Fonts.";

    private static readonly string[] Files = ["Tinos-Regular.ttf", "Tinos-Bold.ttf", "Tinos-Italic.ttf"];

    private static bool _registered;
    private static readonly Lock Gate = new();

    public static void EnsureRegistered()
    {
        lock (Gate)
        {
            if (_registered)
                return;

            // Licence decision is pending with the PO (ADR 0001); the spike validated the technical path.
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = false;
            QuestPDF.Settings.ThrowOnMissingFontFamilies = true;
            QuestPDF.Settings.ThrowOnMissingTextGlyphs = true;

            var assembly = Assembly.GetExecutingAssembly();
            foreach (var file in Files)
            {
                using var stream = assembly.GetManifestResourceStream(ResourcePrefix + file)
                    ?? throw new FileNotFoundException($"Thiếu font nhúng: {file}");
                FontManager.RegisterFontFromStream(stream);
            }

            _registered = true;
        }
    }
}
