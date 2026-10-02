using QuestPDF.Infrastructure;
using SP01QuestPdf;

namespace SP01QuestPdf;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        // Evaluation only, as allowed by the QuestPDF Community licence v3.0 ("learning and
        // evaluation"). Production use by this unit is NOT covered by the Community licence —
        // see docs/decisions/0001-pdf-engine-questpdf.md (checked 2026-10-02).
        QuestPDF.Settings.License = LicenseType.Community;

        // Never fall back to installed workstation fonts: a missing glyph must fail the render,
        // not silently look right only on the dev machine.
        QuestPDF.Settings.UseSystemFonts = false;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = true;
        QuestPDF.Settings.ThrowOnMissingTextGlyphs = true;

        if (args.Length >= 1 && args[0] == "--render")
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: SP-01-QuestPdf --render <outDir>");
                return 2;
            }

            return HeadlessRenderer.Run(args[1]);
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new PreviewForm());
        return 0;
    }
}