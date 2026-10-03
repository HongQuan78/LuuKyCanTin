namespace LuuKyCanTin.Application.Reporting;

/// <summary>
/// The stable codes of the 12 print templates: the only place they are spelled. The code values are spec-defined
/// data stored in <c>SignatoryConfiguration.TemplateCode</c>, so they never change; display names are user-visible
/// text kept next to each code for the signatory screen.
/// </summary>
public static class TemplateCodes
{
    public const string DepositReceipt = "BIEN_NHAN_THU";
    public const string Payout = "PHIEU_CHI";
    public const string InmateStatement = "BANG_KE_CA_NHAN";
    public const string CustodyLedgerBook = "SO_THEO_DOI";
    public const string RemittanceList = "BANG_KE_NOP";
    public const string GoodsReceipt = "PHIEU_NHAP";
    public const string PurchaseSlip = "PHIEU_MUA_HANG";
    public const string StockMovementReport = "BAO_CAO_NXT";
    public const string RevenueReport = "BAO_CAO_DOANH_THU";
    public const string PriceList = "BANG_NIEM_YET_GIA";
    public const string PurchaseHistory = "THEO_DOI_MUA_HANG";
    public const string ProductCatalogue = "DANH_MUC_HANG_HOA";

    public sealed record TemplateDefinition(string Code, string DisplayName);

    /// <summary>The 12 templates, in the order of the process doc; display names are the signatory screen's list.</summary>
    public static readonly IReadOnlyList<TemplateDefinition> All =
    [
        new(DepositReceipt, "Biên nhận thu tiền gửi lưu ký"),
        new(Payout, "Phiếu chi xuất tiền lưu ký"),
        new(InmateStatement, "Bảng kê theo dõi cá nhân"),
        new(CustodyLedgerBook, "Sổ theo dõi tiền gửi lưu ký"),
        new(RemittanceList, "Bảng kê nộp tiền gửi lưu ký"),
        new(GoodsReceipt, "Phiếu nhập hàng"),
        new(PurchaseSlip, "Phiếu mua hàng"),
        new(StockMovementReport, "Báo cáo nhập, xuất hàng hoá"),
        new(RevenueReport, "Báo cáo doanh thu"),
        new(PriceList, "Bảng niêm yết giá"),
        new(PurchaseHistory, "Bảng theo dõi mua hàng"),
        new(ProductCatalogue, "Danh mục hàng hoá"),
    ];
}
