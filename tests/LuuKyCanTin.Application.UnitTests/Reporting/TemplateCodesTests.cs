using LuuKyCanTin.Application.Reporting;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Reporting;

public class TemplateCodesTests
{
    [Fact]
    public void All_ListsTheTwelveConstantsInOrderWithDistinctCodesAndDisplayNames()
    {
        string[] constants =
        [
            TemplateCodes.DepositReceipt,
            TemplateCodes.Payout,
            TemplateCodes.InmateStatement,
            TemplateCodes.CustodyLedgerBook,
            TemplateCodes.RemittanceList,
            TemplateCodes.GoodsReceipt,
            TemplateCodes.PurchaseSlip,
            TemplateCodes.StockMovementReport,
            TemplateCodes.RevenueReport,
            TemplateCodes.PriceList,
            TemplateCodes.PurchaseHistory,
            TemplateCodes.ProductCatalogue,
        ];

        TemplateCodes.All.Select(t => t.Code).ShouldBe(constants);
        constants.Distinct(StringComparer.Ordinal).Count().ShouldBe(12);
        TemplateCodes.All.ShouldAllBe(t => t.DisplayName.Length > 0);
    }

    [Fact]
    public void All_DisplayNames_MatchTheScreenWordingInCatalogueOrder()
    {
        string[] expected =
        [
            "Biên nhận thu tiền gửi lưu ký",
            "Phiếu chi xuất tiền lưu ký",
            "Bảng kê theo dõi cá nhân",
            "Sổ theo dõi tiền gửi lưu ký",
            "Bảng kê nộp tiền gửi lưu ký",
            "Phiếu nhập hàng",
            "Phiếu mua hàng",
            "Báo cáo nhập, xuất hàng hoá",
            "Báo cáo doanh thu",
            "Bảng niêm yết giá",
            "Bảng theo dõi mua hàng",
            "Danh mục hàng hoá",
        ];

        TemplateCodes.All.Select(t => t.DisplayName).ShouldBe(expected);
    }

    [Fact]
    public void All_Codes_FitTheVarchar30KeyColumn()
    {
        TemplateCodes.All.ShouldAllBe(t => t.Code.Length <= 30);
    }
}
