using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

internal sealed class SignatoryConfigurationConfiguration : IEntityTypeConfiguration<SignatoryConfiguration>
{
    public void Configure(EntityTypeBuilder<SignatoryConfiguration> builder)
    {
        builder.ToTable("SignatoryConfiguration", t =>
        {
            t.HasCheckConstraint("CK_SignatoryConfiguration_Ordinal", "[Ordinal] >= 1");
            t.HasCheckConstraint("CK_SignatoryConfiguration_TemplateCode", BuildTemplateCodeCheck());
        });

        builder.Property(e => e.TemplateCode).HasMaxLength(30).IsUnicode(false).IsRequired();
        // The CHECK (Ordinal >= 1) rejects the only invalid byte value, 0; the type stays tinyint to match the DB design.
        builder.Property(e => e.Ordinal).HasColumnType("tinyint");
        builder.Property(e => e.Title).HasMaxLength(SignatoryConfigurationService.TitleMaxLength).IsRequired();

        builder.HasIndex(e => new { e.TemplateCode, e.Ordinal }).IsUnique();

        builder.HasOne<Officer>()
            .WithMany()
            .HasForeignKey(e => e.OfficerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Defaults seeded once (the unit owns them after): the 39 signer lines of the 12 templates, in print order.
        // Later migrations must never re-seed these rows, or they would overwrite the unit's edits.
        builder.HasData(
            new SignatoryConfiguration { Id = 1, TemplateCode = TemplateCodes.DepositReceipt, Ordinal = 1, Title = "Người gửi" },
            new SignatoryConfiguration { Id = 2, TemplateCode = TemplateCodes.DepositReceipt, Ordinal = 2, Title = "Người nhận" },
            new SignatoryConfiguration { Id = 3, TemplateCode = TemplateCodes.DepositReceipt, Ordinal = 3, Title = "Lãnh đạo đơn vị" },
            new SignatoryConfiguration { Id = 4, TemplateCode = TemplateCodes.Payout, Ordinal = 1, Title = "Cán bộ theo dõi tiền lưu ký" },
            new SignatoryConfiguration { Id = 5, TemplateCode = TemplateCodes.Payout, Ordinal = 2, Title = "Người bị tạm giữ, tạm giam/phạm nhân xác nhận" },
            new SignatoryConfiguration { Id = 6, TemplateCode = TemplateCodes.Payout, Ordinal = 3, Title = "Cán bộ quản giáo xác nhận" },
            new SignatoryConfiguration { Id = 7, TemplateCode = TemplateCodes.Payout, Ordinal = 4, Title = "Lãnh đạo đơn vị xác nhận" },
            new SignatoryConfiguration { Id = 8, TemplateCode = TemplateCodes.InmateStatement, Ordinal = 1, Title = "Cán bộ căn tin" },
            new SignatoryConfiguration { Id = 9, TemplateCode = TemplateCodes.InmateStatement, Ordinal = 2, Title = "Cán bộ quản giáo" },
            new SignatoryConfiguration { Id = 10, TemplateCode = TemplateCodes.InmateStatement, Ordinal = 3, Title = "Người bị tạm giữ, tạm giam/phạm nhân" },
            new SignatoryConfiguration { Id = 11, TemplateCode = TemplateCodes.InmateStatement, Ordinal = 4, Title = "Thủ trưởng đơn vị" },
            new SignatoryConfiguration { Id = 12, TemplateCode = TemplateCodes.CustodyLedgerBook, Ordinal = 1, Title = "Cán bộ căn tin" },
            new SignatoryConfiguration { Id = 13, TemplateCode = TemplateCodes.CustodyLedgerBook, Ordinal = 2, Title = "Chỉ huy phụ trách" },
            new SignatoryConfiguration { Id = 14, TemplateCode = TemplateCodes.CustodyLedgerBook, Ordinal = 3, Title = "Kế toán đơn vị" },
            new SignatoryConfiguration { Id = 15, TemplateCode = TemplateCodes.CustodyLedgerBook, Ordinal = 4, Title = "Thủ trưởng đơn vị" },
            new SignatoryConfiguration { Id = 16, TemplateCode = TemplateCodes.RemittanceList, Ordinal = 1, Title = "Người nộp" },
            new SignatoryConfiguration { Id = 17, TemplateCode = TemplateCodes.RemittanceList, Ordinal = 2, Title = "Chỉ huy phụ trách" },
            new SignatoryConfiguration { Id = 18, TemplateCode = TemplateCodes.RemittanceList, Ordinal = 3, Title = "Thủ trưởng đơn vị" },
            new SignatoryConfiguration { Id = 19, TemplateCode = TemplateCodes.GoodsReceipt, Ordinal = 1, Title = "Người giao" },
            new SignatoryConfiguration { Id = 20, TemplateCode = TemplateCodes.GoodsReceipt, Ordinal = 2, Title = "Người nhận" },
            new SignatoryConfiguration { Id = 21, TemplateCode = TemplateCodes.GoodsReceipt, Ordinal = 3, Title = "Chỉ huy đội" },
            new SignatoryConfiguration { Id = 22, TemplateCode = TemplateCodes.GoodsReceipt, Ordinal = 4, Title = "Lãnh đạo đơn vị" },
            new SignatoryConfiguration { Id = 23, TemplateCode = TemplateCodes.PurchaseSlip, Ordinal = 1, Title = "Người mua hàng" },
            new SignatoryConfiguration { Id = 24, TemplateCode = TemplateCodes.PurchaseSlip, Ordinal = 2, Title = "Cán bộ căn tin" },
            new SignatoryConfiguration { Id = 25, TemplateCode = TemplateCodes.PurchaseSlip, Ordinal = 3, Title = "Lãnh đạo đơn vị" },
            new SignatoryConfiguration { Id = 26, TemplateCode = TemplateCodes.StockMovementReport, Ordinal = 1, Title = "Cán bộ bán hàng" },
            new SignatoryConfiguration { Id = 27, TemplateCode = TemplateCodes.StockMovementReport, Ordinal = 2, Title = "Chỉ huy phụ trách" },
            new SignatoryConfiguration { Id = 28, TemplateCode = TemplateCodes.StockMovementReport, Ordinal = 3, Title = "Lãnh đạo đơn vị" },
            new SignatoryConfiguration { Id = 29, TemplateCode = TemplateCodes.RevenueReport, Ordinal = 1, Title = "Cán bộ căn tin" },
            new SignatoryConfiguration { Id = 30, TemplateCode = TemplateCodes.RevenueReport, Ordinal = 2, Title = "Chỉ huy phụ trách" },
            new SignatoryConfiguration { Id = 31, TemplateCode = TemplateCodes.RevenueReport, Ordinal = 3, Title = "Lãnh đạo đơn vị" },
            new SignatoryConfiguration { Id = 32, TemplateCode = TemplateCodes.PriceList, Ordinal = 1, Title = "Cán bộ căn tin" },
            new SignatoryConfiguration { Id = 33, TemplateCode = TemplateCodes.PriceList, Ordinal = 2, Title = "Lãnh đạo đơn vị" },
            new SignatoryConfiguration { Id = 34, TemplateCode = TemplateCodes.PurchaseHistory, Ordinal = 1, Title = "Cán bộ căn tin" },
            new SignatoryConfiguration { Id = 35, TemplateCode = TemplateCodes.PurchaseHistory, Ordinal = 2, Title = "Chỉ huy phụ trách" },
            new SignatoryConfiguration { Id = 36, TemplateCode = TemplateCodes.PurchaseHistory, Ordinal = 3, Title = "Lãnh đạo đơn vị" },
            new SignatoryConfiguration { Id = 37, TemplateCode = TemplateCodes.ProductCatalogue, Ordinal = 1, Title = "Cán bộ căn tin" },
            new SignatoryConfiguration { Id = 38, TemplateCode = TemplateCodes.ProductCatalogue, Ordinal = 2, Title = "Chỉ huy phụ trách" },
            new SignatoryConfiguration { Id = 39, TemplateCode = TemplateCodes.ProductCatalogue, Ordinal = 3, Title = "Lãnh đạo đơn vị" });
    }

    // One source for the allowed templates: a typo cannot create a 13th template in the database.
    private static string BuildTemplateCodeCheck()
    {
        var codes = string.Join(", ", TemplateCodes.All.Select(t => $"'{t.Code}'"));
        return $"[TemplateCode] IN ({codes})";
    }
}
