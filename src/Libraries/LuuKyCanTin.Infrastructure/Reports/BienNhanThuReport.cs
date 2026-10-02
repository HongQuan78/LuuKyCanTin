using System.Globalization;
using System.Text;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.BaoCao;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.LuuKy;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LuuKyCanTin.Infrastructure.Reports;

/// <summary>
/// The skeleton's receipt template, following process A.I.5 loosely. The shared frame, configured signers,
/// print counting and the reprint mark are Epic 4 (FR42/FR43/FR27), so they are not built here.
/// </summary>
internal sealed class BienNhanThuReport(IClock clock) : IReportTemplate<BienNhanThuModel>
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public byte[] Render(BienNhanThuModel model)
    {
        ReportFonts.EnsureRegistered();

        // Text pasted from Word can arrive as NFD; normalize so the embedded font always has the glyph.
        var metadata = new DocumentMetadata
        {
            Title = $"Biên nhận thu tiền gửi lưu ký {model.SoChungTu}",
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

    private static void Header(IContainer container, BienNhanThuModel model)
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

            if (!string.IsNullOrWhiteSpace(model.TenCoQuanChuQuan))
            {
                column.Item().Text(text =>
                {
                    text.AlignCenter();
                    text.Span(Nfc(model.TenCoQuanChuQuan)).Style(TextStyle.Default.Bold());
                });
            }

            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span(Nfc(model.TenDonVi)).Style(TextStyle.Default.Bold());
            });
            column.Item().Text(text =>
            {
                text.AlignCenter();
                text.Span($"Địa chỉ: {Nfc(model.DiaChi)}").FontSize(9).FontColor(GreyText);
            });
        });
    }

    private static void Content(IContainer container, BienNhanThuModel model)
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
                text.Span($"Số: {model.SoChungTu}  ·  {NgayChu(model.NgayChungTu)}").FontSize(9);
            });

            column.Item().PaddingBottom(2).Text($"Họ tên người nộp tiền: {Nfc(model.NguoiGuiHoTen)}");
            if (!string.IsNullOrWhiteSpace(model.QuanHe))
                column.Item().PaddingBottom(2).Text($"Quan hệ với đối tượng: {Nfc(model.QuanHe)}");
            column.Item().PaddingBottom(2).Text($"Đối tượng nhận tiền: {Nfc(model.HoTenDoiTuong)} — {TenLoaiDoiTuong(model.LoaiDoiTuong)}");
            column.Item().PaddingBottom(2).Text($"Hình thức: {TenHinhThuc(model)}");
            if (!string.IsNullOrWhiteSpace(model.NoiDung))
                column.Item().PaddingBottom(2).Text($"Nội dung: {Nfc(model.NoiDung)}");

            column.Item().PaddingTop(4).PaddingBottom(2).Text(text =>
            {
                text.Span("Số tiền: ").Style(TextStyle.Default.Bold());
                text.Span($"{model.SoTien.ToString("N0", Vi)} đồng").Style(TextStyle.Default.Bold());
            });
            column.Item().Text(text =>
            {
                text.Span("Viết bằng chữ: ").Style(TextStyle.Default.Italic());
                text.Span(Nfc(model.SoTienBangChu)).Style(TextStyle.Default.Italic());
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

    private static void SignatureCell(IContainer container, string chucDanh)
    {
        container.PaddingTop(20).Column(column =>
        {
            column.Item().AlignCenter().Text(text => text.Span(chucDanh).Style(TextStyle.Default.Bold().FontSize(9)));
            column.Item().PaddingTop(20).AlignCenter().Text(text =>
                text.Span("(Ký, ghi rõ họ tên)").FontSize(9).FontColor(GreyText));
        });
    }

    private static string TenLoaiDoiTuong(LoaiDoiTuong loai) => loai switch
    {
        LoaiDoiTuong.TamGiuTamGiam => "Tạm giữ/tạm giam",
        LoaiDoiTuong.PhamNhan => "Phạm nhân",
        _ => loai.ToString(),
    };

    private static string TenHinhThuc(BienNhanThuModel model) => model.HinhThuc switch
    {
        HinhThuc.TienMat => "Tiền mặt",
        HinhThuc.ChuyenKhoan => $"Chuyển khoản — STK: {model.SoTaiKhoanNguoiGui}",
        _ => model.HinhThuc.ToString(),
    };

    private static string NgayChu(DateOnly ngay) => $"Ngày {ngay.Day:D2} tháng {ngay.Month:D2} năm {ngay.Year}";

    private static string Nfc(string? value) => (value ?? "").Normalize(NormalizationForm.FormC);

    private static Color GreyText => Color.FromHex("#666666");
}
