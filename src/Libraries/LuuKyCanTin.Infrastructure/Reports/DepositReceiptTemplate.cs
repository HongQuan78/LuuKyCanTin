using System.Globalization;
using System.Text;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Domain.Custody;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LuuKyCanTin.Infrastructure.Reports;

/// <summary>
/// The skeleton's receipt template, following process A.I.5 loosely. The shared frame, configured signers,
/// print counting and the reprint mark are Epic 4 (FR42/FR43/FR27), so they are not built here.
/// </summary>
internal sealed class DepositReceiptTemplate(IClock clock) : IReportTemplate<DepositReceiptModel>
{
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");

    public byte[] Render(DepositReceiptModel model)
    {
        ReportFonts.EnsureRegistered();

        // Text pasted from Word can arrive as NFD; normalize so the embedded font always has the glyph.
        var metadata = new DocumentMetadata
        {
            Title = $"Biên nhận thu tiền gửi lưu ký {model.VoucherNumber}",
            CreationDate = new DateTimeOffset(clock.Now),
            ModifiedDate = new DateTimeOffset(clock.Now),
        };

        return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(1f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(style => style.FontFamily(ReportFonts.Family).FontSize(10).LineHeight(1.3f));

                    page.Header().Element(c => Header(c, model));
                    page.Content().Element(c => Content(c, model));
                });
            })
            .WithMetadata(metadata)
            .GeneratePdf();
    }

    private static void Header(IContainer container, DepositReceiptModel model)
    {
        container.Column(column =>
        {
            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Style(TextStyle.Default.Bold());
            });
            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span("Độc lập – Tự do – Hạnh phúc");
            });
            column.Item().PaddingBottom(4).Text(text =>
            {
                text.AlignCenter();
                text.Span("---o0o---");
            });

            if (!string.IsNullOrWhiteSpace(model.ParentAgencyName))
            {
                column.Item().Text(text =>
                {
                    text.AlignCenter();
                    text.Span(Nfc(model.ParentAgencyName)).Style(TextStyle.Default.Bold());
                });
            }

            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span(Nfc(model.FacilityName)).Style(TextStyle.Default.Bold());
            });
            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span($"Địa chỉ: {Nfc(model.Address)}").FontSize(9).FontColor(GreyText);
            });
        });
    }

    private static void Content(IContainer container, DepositReceiptModel model)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(8).PaddingBottom(6).Text(text =>
            {
                text.AlignCenter();
                text.Span("BIÊN NHẬN THU TIỀN GỬI LƯU KÝ").Style(TextStyle.Default.Bold().Underline().FontSize(13));
            });

            column.Item().PaddingBottom(6).Text(text =>
            {
                text.AlignCenter();
                text.Span($"Số: {model.VoucherNumber}  ·  {FormatDate(model.VoucherDate)}").FontSize(9);
            });

            column.Item().PaddingBottom(2).Text($"Họ tên người nộp tiền: {Nfc(model.SenderFullName)}");
            if (!string.IsNullOrWhiteSpace(model.Relationship))
                column.Item().PaddingBottom(2).Text($"Quan hệ với đối tượng: {Nfc(model.Relationship)}");
            column.Item().PaddingBottom(2).Text($"Đối tượng nhận tiền: {Nfc(model.InmateFullName)} — {model.InmateType.ToDisplayText()}");
            column.Item().PaddingBottom(2).Text($"Hình thức: {PaymentMethodText(model)}");
            if (!string.IsNullOrWhiteSpace(model.Description))
                column.Item().PaddingBottom(2).Text($"Nội dung: {Nfc(model.Description)}");

            column.Item().PaddingTop(4).PaddingBottom(2).Text(text =>
            {
                text.Span("Số tiền: ").Style(TextStyle.Default.Bold());
                text.Span($"{model.Amount.ToString("N0", VietnameseCulture)} đồng").Style(TextStyle.Default.Bold());
            });
            column.Item().Text(text =>
            {
                text.Span("Viết bằng chữ: ").Style(TextStyle.Default.Italic());
                text.Span(Nfc(model.AmountInWords)).Style(TextStyle.Default.Italic());
            });

            column.Item().PaddingTop(12).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Element(c => SignatureCell(c, "NGƯỜI NỘP TIỀN"));
                table.Cell().Element(c => SignatureCell(c, "NGƯỜI NHẬN"));
                table.Cell().Element(c => SignatureCell(c, "THỦ TRƯỞNG ĐƠN VỊ"));
            });
        });
    }

    private static void SignatureCell(IContainer container, string title)
    {
        container.PaddingTop(20).Column(column =>
        {
            column.Item().AlignCenter().Text(text => text.Span(title).Style(TextStyle.Default.Bold().FontSize(9)));
            column.Item().PaddingTop(20).AlignCenter().Text(text =>
                text.Span("(Ký, ghi rõ họ tên)").FontSize(9).FontColor(GreyText));
        });
    }

    private static string PaymentMethodText(DepositReceiptModel model) => model.PaymentMethod == PaymentMethod.BankTransfer
        ? $"{model.PaymentMethod.ToDisplayText()} — STK: {model.SenderAccountNumber}"
        : model.PaymentMethod.ToDisplayText();

    private static string FormatDate(DateOnly date) => $"Ngày {date.Day:D2} tháng {date.Month:D2} năm {date.Year}";

    private static string Nfc(string? value) => (value ?? "").Normalize(NormalizationForm.FormC);

    private static Color GreyText => Color.FromHex("#666666");
}
