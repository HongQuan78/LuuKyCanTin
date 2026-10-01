using System.Security.Cryptography;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace SP01QuestPdf;

/// <summary>
/// Headless mode: `SP-01-QuestPdf --render <outDir>` writes the three sample PDFs (A4 portrait,
/// A5 portrait, A5 landscape) plus determinism.txt. Each document is rendered twice with the same
/// fixed CreationDate; the SHA-256 pair must match. Exit code 0 = deterministic, 1 = mismatch.
/// </summary>
internal static class HeadlessRenderer
{
    public static int Run(string outDir)
    {
        try
        {
            SampleReportGenerator.RegisterFonts();
            Directory.CreateDirectory(outDir);

            var documents = new (MauInKichThuoc Kind, string FileName)[]
            {
                (MauInKichThuoc.A4Portrait, "mau-in-a4-portrait.pdf"),
                (MauInKichThuoc.A5Portrait, "mau-in-a5-portrait.pdf"),
                (MauInKichThuoc.A5Landscape, "mau-in-a5-landscape.pdf"),
            };

            var report = new StringBuilder();
            var allMatch = true;

            foreach (var (kind, fileName) in documents)
            {
                var document = SampleReportGenerator.CreateDocument(kind);
                var first = document.GeneratePdf();
                var second = document.GeneratePdf();

                var firstValid = IsValidPdf(first);
                var secondValid = IsValidPdf(second);
                var firstHash = Sha256(first);
                var secondHash = Sha256(second);
                var match = firstValid && secondValid && firstHash == secondHash;
                allMatch &= match;

                if (match)
                {
                    File.WriteAllBytes(Path.Combine(outDir, fileName), first);

                    // PNG previews (one per page, "-p<N>" suffix) so the layout can be
                    // reviewed without a PDF reader.
                    var pageIndex = 0;
                    foreach (var page in document.GenerateImages(new ImageGenerationSettings { RasterDpi = 110 }))
                    {
                        pageIndex++;
                        File.WriteAllBytes(
                            Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(fileName)}-p{pageIndex}.png"),
                            page);
                    }

                    report.AppendLine($"{fileName}: {firstHash} | {secondHash} | MATCH");
                }
                else
                {
                    report.AppendLine(
                        $"{fileName}: {firstHash} | {secondHash} | {(firstValid && secondValid ? "DIFF" : "INVALID")}");
                }
            }

            report.AppendLine();
            report.AppendLine($"Deterministic: {allMatch} (fixed CreationDate = {SampleReportGenerator.FixedCreationDate:u})");
            File.WriteAllText(Path.Combine(outDir, "determinism.txt"), report.ToString());

            Console.WriteLine(report);
            return allMatch ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"RENDER FAILED: {ex}");
            return 2;
        }
    }

    private static string Sha256(byte[] data) =>
        Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();

    private static bool IsValidPdf(byte[] data) =>
        data.Length > 0 && data.AsSpan().StartsWith("%PDF"u8);
}