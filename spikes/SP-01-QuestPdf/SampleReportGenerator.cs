using System.Reflection;
using System.Text;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SP01QuestPdf;

public enum MauInKichThuoc
{
    A4Portrait,
    A5Portrait,
    A5Landscape,
}

/// <summary>
/// Generates one sample "biên nhận thu tiền gửi lưu ký" document that covers every Vietnamese
/// diacritic, bold/italic, an amount-in-words line, the centred unit header block, a 3-column
/// signature block and the rotated semi-transparent "BẢN IN LẠI" watermark — the shape every
/// template reuses (FR42). All fonts are embedded; nothing depends on installed fonts.
/// </summary>
public sealed class SampleReportGenerator
{
    /// <summary>Fixed metadata so two renders of the same model are byte-identical (determinism check).</summary>
    public static readonly DateTime FixedCreationDate = new(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc);

    private const string FontFamily = "Tinos";
    private const string FontResourcePrefix = "SP01QuestPdf.Fonts.";

    private static bool _fontsRegistered;

    public static void RegisterFonts()
    {
        if (_fontsRegistered)
        {
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();
        foreach (var file in new[] { "Tinos-Regular.ttf", "Tinos-Bold.ttf", "Tinos-Italic.ttf" })
        {
            using var stream = assembly.GetManifestResourceStream(FontResourcePrefix + file)
                ?? throw new FileNotFoundException($"Embedded font resource missing: {file}");
            FontManager.RegisterFontFromStream(stream);
        }

        _fontsRegistered = true;
    }

    public static byte[] Generate(MauInKichThuoc kichThuoc) =>
        CreateDocument(kichThuoc).GeneratePdf();

    public static IDocument CreateDocument(MauInKichThuoc kichThuoc)
    {
        var pageSize = GetPageSize(kichThuoc);

        var metadata = new DocumentMetadata
        {
            Title = "Biên nhận thu tiền gửi lưu ký — mẫu in SP-01",
            CreationDate = new DateTimeOffset(FixedCreationDate),
            ModifiedDate = new DateTimeOffset(FixedCreationDate),
        };

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(pageSize);
                page.Margin(1.2f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style
                    .FontFamily(FontFamily)
                    .FontSize(11)
                    .LineHeight(1.35f));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);

                // Watermark (UX-DR6): drawn in front of everything, rotated around the page
                // centre, ~10 % opacity. Content below stays fully readable.
                page.Foreground()
                    .Rotate(-45)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(text => text
                        .Span("BẢN IN LẠI")
                        .Style(TextStyle.Default.FontSize(52).FontColor(Colors.Black.WithAlpha(0.1f))));
            });
        })
        .WithMetadata(metadata);
    }

    private static PageSize GetPageSize(MauInKichThuoc kichThuoc) => kichThuoc switch
    {
        MauInKichThuoc.A4Portrait => PageSizes.A4,
        MauInKichThuoc.A5Portrait => PageSizes.A5,
        MauInKichThuoc.A5Landscape => PageSizes.A5.Landscape(),
        _ => throw new ArgumentOutOfRangeException(nameof(kichThuoc), kichThuoc, null),
    };

    private static void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM")
                    .Style(TextStyle.Default.Bold().FontSize(12));
            });
            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span("Độc lập – Tự do – Hạnh phúc");
            });
            column.Item().PaddingTop(2).PaddingBottom(6).Text(text =>
            {
                text.AlignCenter();
                text.Span("---o0o---");
            });
            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span("TRẠI TẠM GIAM CÔNG AN TỈNH … – PHÂN TRẠI QUẢN LÝ PHẠM NHÂN")
                    .Style(TextStyle.Default.Bold().FontSize(12));
            });
            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span("Địa chỉ: xã …, huyện …, tỉnh … · Điện thoại: …")
                    .Style(TextStyle.Default.FontSize(10).FontColor(GreyText));
            });
        });
    }

    private static void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(10).PaddingBottom(2).Text(text =>
            {
                text.AlignCenter();
                text.Span("BIÊN NHẬN THU TIỀN GỬI LƯU KÝ")
                    .Style(TextStyle.Default.Bold().Underline().FontSize(14));
            });
            column.Item().PaddingBottom(10).Text(text =>
            {
                text.AlignCenter();
                text.Span("Số: BNT/2026/000123").Style(TextStyle.Default.FontSize(10));
            });

            column.Item().PaddingBottom(2).Text("Kính gửi: Ban Giám thị Trại tạm giam.");
            column.Item().PaddingBottom(2).Text("Họ tên người nộp tiền: Nguyễn Thị Ánh Tuyết");
            column.Item().PaddingBottom(2).Text("Đối tượng (người bị tạm giam): Đỗ Văn Bình – Buồng A3");
            column.Item().PaddingBottom(2).Text("Nội dung: Tiền gửi lưu ký");
            column.Item().PaddingBottom(2).Text("Số tiền: 500.000 đồng");
            column.Item().PaddingBottom(10).Text("Viết bằng chữ: Năm trăm nghìn đồng chẵn");

            column.Item().PaddingBottom(2).Text(text =>
            {
                text.Span("Kiểu chữ: ").Style(TextStyle.Default.SemiBold());
                text.Span("in đậm, ").Style(TextStyle.Default.Bold());
                text.Span("in nghiêng, ").Style(TextStyle.Default.Italic());
                text.Span("đậm + nghiêng, ").Style(TextStyle.Default.Bold().Italic());
                text.Span("và thường.").Style(TextStyle.Default.NormalWeight());
            });

            column.Item().PaddingBottom(2).Text(text =>
            {
                text.Span("Tất cả dấu tiếng Việt (thường): ").Style(TextStyle.Default.SemiBold());
                text.Span("á à ả ã ạ ắ ằ ẳ ẵ ặ ấ ầ ẩ ẫ ậ é è ẻ ẽ ẹ ế ề ể ễ ệ "
                          + "í ì ỉ ĩ ị ó ò ỏ õ ọ ố ồ ổ ỗ ộ ớ ờ ở ỡ ợ ú ù ủ ũ ụ ứ ừ ử ữ ự ý ỳ ỷ ỹ ỵ");
            });

            column.Item().PaddingBottom(2).Text(text =>
            {
                text.Span("Tất cả dấu tiếng Việt (hoa): ").Style(TextStyle.Default.SemiBold());
                text.Span("Á À Ả Ã Ạ Ắ Ằ Ẳ Ẵ Ặ Ấ Ầ Ẩ Ẫ Ậ É È Ẻ Ẽ Ẹ Ế Ề Ể Ễ Ệ "
                          + "Í Ì Ỉ Ĩ Ị Ó Ò Ỏ Õ Ọ Ố Ồ Ổ Ỗ Ộ Ớ Ờ Ở Ỡ Ợ Ú Ù Ủ Ũ Ụ Ứ Ừ Ử Ữ Ự Ý Ỳ Ỷ Ỹ Ỵ Đ đ");
            });

            column.Item().PaddingBottom(2).Text(text =>
            {
                text.Span("Ví dụ câu đầy đủ: ").Style(TextStyle.Default.SemiBold());
                text.Span("Nguyễn Thị Ánh Tuyết – Biên nhận thu tiền gửi lưu ký");
            });

            var tenNfc = "Nguyễn Thị Ánh Tuyết – Đặng Đình Đức";
            var tenNfd = tenNfc.Normalize(NormalizationForm.FormD);
            column.Item().PaddingBottom(2).Text($"Unicode NFC: {tenNfc}");
            column.Item().PaddingBottom(2).Text($"Unicode NFD: {tenNfd}");
            column.Item().PaddingBottom(2).Text(text => text
                .Span("Hai dòng NFC/NFD phải giống hệt nhau khi in. Khuyến nghị: chuẩn hóa NFC (FormC) trước khi render.")
                .Style(TextStyle.Default.FontSize(9).FontColor(GreyText)));

            column.Item().PaddingTop(14).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Element(c => SignatureCell(c, "NGƯỜI NỘP TIỀN"));
                table.Cell().Element(c => SignatureCell(c, "KẾ TOÁN"));
                table.Cell().Element(c => SignatureCell(c, "THỦ TRƯỞNG ĐƠN VỊ"));
            });
        });
    }

    private static void SignatureCell(IContainer container, string chucDanh)
    {
        container.PaddingTop(30).Column(column =>
        {
            column.Item().AlignCenter().Text(text => text
                .Span(chucDanh).Style(TextStyle.Default.Bold().FontSize(10)));
            column.Item().PaddingTop(26).AlignCenter().Text(text => text
                .Span("(Ký, ghi rõ họ tên)").Style(TextStyle.Default.FontSize(10)));
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text => text
            .Span("Mẫu in BIEN_NHAN_THU · SP-01 · Tinos (Apache 2.0) · In ngày 02/10/2026 · Bản in lần 2")
            .Style(TextStyle.Default.FontSize(8).FontColor(GreyText)));
    }

    private static QuestPDF.Infrastructure.Color GreyText => QuestPDF.Infrastructure.Color.FromHex("#666666");
}