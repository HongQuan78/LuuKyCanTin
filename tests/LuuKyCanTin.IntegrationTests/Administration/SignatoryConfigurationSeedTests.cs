using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

/// <summary>
/// The AC 1 seed, checked in its own class so no save test can have replaced a template's rows first. The class
/// fixture gives this class its own database, unshared with other test classes.
/// </summary>
public sealed class SignatoryConfigurationSeedTests : IClassFixture<AppDatabaseFixture>
{
    // The signer lists of the process doc, left to right; compared character for character.
    private static readonly (string Code, string[] Titles)[] Expected =
    [
        (TemplateCodes.DepositReceipt, ["Người gửi", "Người nhận", "Lãnh đạo đơn vị"]),
        (TemplateCodes.Payout,
        [
            "Cán bộ theo dõi tiền lưu ký",
            "Người bị tạm giữ, tạm giam/phạm nhân xác nhận",
            "Cán bộ quản giáo xác nhận",
            "Lãnh đạo đơn vị xác nhận",
        ]),
        (TemplateCodes.InmateStatement,
        [
            "Cán bộ căn tin",
            "Cán bộ quản giáo",
            "Người bị tạm giữ, tạm giam/phạm nhân",
            "Thủ trưởng đơn vị",
        ]),
        (TemplateCodes.CustodyLedgerBook, ["Cán bộ căn tin", "Chỉ huy phụ trách", "Kế toán đơn vị", "Thủ trưởng đơn vị"]),
        (TemplateCodes.RemittanceList, ["Người nộp", "Chỉ huy phụ trách", "Thủ trưởng đơn vị"]),
        (TemplateCodes.GoodsReceipt, ["Người giao", "Người nhận", "Chỉ huy đội", "Lãnh đạo đơn vị"]),
        (TemplateCodes.PurchaseSlip, ["Người mua hàng", "Cán bộ căn tin", "Lãnh đạo đơn vị"]),
        (TemplateCodes.StockMovementReport, ["Cán bộ bán hàng", "Chỉ huy phụ trách", "Lãnh đạo đơn vị"]),
        (TemplateCodes.RevenueReport, ["Cán bộ căn tin", "Chỉ huy phụ trách", "Lãnh đạo đơn vị"]),
        (TemplateCodes.PriceList, ["Cán bộ căn tin", "Lãnh đạo đơn vị"]),
        (TemplateCodes.PurchaseHistory, ["Cán bộ căn tin", "Chỉ huy phụ trách", "Lãnh đạo đơn vị"]),
        (TemplateCodes.ProductCatalogue, ["Cán bộ căn tin", "Chỉ huy phụ trách", "Lãnh đạo đơn vị"]),
    ];

    private readonly AppDatabaseFixture _fixture;

    public SignatoryConfigurationSeedTests(AppDatabaseFixture fixture) => _fixture = fixture;

    [SqlServerFact]
    public async Task Seed_DefaultSignatories_MatchTheProcessDocumentExactly()
    {
        await using var db = _fixture.Database.CreateDbContext();
        var rows = await db.SignatoryConfiguration.AsNoTracking()
            .OrderBy(s => s.TemplateCode)
            .ThenBy(s => s.Ordinal)
            .ToListAsync();

        rows.Count.ShouldBe(39);
        rows.Select(s => s.TemplateCode).Distinct().ShouldBe(TemplateCodes.All.Select(t => t.Code), ignoreOrder: true);

        foreach (var (code, titles) in Expected)
        {
            var templateRows = rows.Where(s => s.TemplateCode == code).ToList();
            templateRows.Select(s => s.Title).ShouldBe(titles);
            templateRows.Select(s => (int)s.Ordinal).ShouldBe(Enumerable.Range(1, titles.Length));
        }
    }

    [SqlServerFact]
    public async Task Seed_Signatories_HaveNoDefaultOfficer()
    {
        await using var db = _fixture.Database.CreateDbContext();
        var officerIds = await db.SignatoryConfiguration.AsNoTracking().Select(s => s.OfficerId).ToListAsync();

        officerIds.Count.ShouldBe(39);
        officerIds.ShouldAllBe(officerId => officerId == null);
    }
}
