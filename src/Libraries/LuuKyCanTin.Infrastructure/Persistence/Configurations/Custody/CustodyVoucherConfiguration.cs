using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Custody;

internal sealed class CustodyVoucherConfiguration : AuditableEntityConfiguration<CustodyVoucher>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CustodyVoucher> builder)
    {
        builder.ToTable("CustodyVoucher", t =>
        {
            t.HasCheckConstraint("CK_CustodyVoucher_TransactionType_VoucherType", "[TransactionType] / 10 = [VoucherType]");
            t.HasCheckConstraint("CK_CustodyVoucher_BankTransfer", "[PaymentMethod] <> 2 OR [SenderAccountNumber] IS NOT NULL");
            t.HasCheckConstraint("CK_CustodyVoucher_Amount", "[Amount] > 0");
            t.HasCheckConstraint(
                "CK_CustodyVoucher_Cancellation",
                "[Status] <> 3 OR ([CancellationReason] IS NOT NULL AND [CancelledAt] IS NOT NULL AND [CancelledById] IS NOT NULL)");
        });

        builder.Property(e => e.VoucherNumber).HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.HasIndex(e => e.VoucherNumber).IsUnique();

        builder.Property(e => e.InmateFullName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SenderFullName).HasMaxLength(100);
        builder.Property(e => e.Relationship).HasMaxLength(50);
        builder.Property(e => e.SourceDocumentNumber).HasMaxLength(30).IsUnicode(false);
        builder.Property(e => e.SenderAccountNumber).HasMaxLength(30).IsUnicode(false);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.AmountInWords).HasMaxLength(300).IsRequired();
        builder.Property(e => e.CancellationReason).HasMaxLength(300);
        builder.Property(e => e.CancelledAt).HasColumnType("datetime2(0)");
        builder.Property(e => e.PrintCount).HasDefaultValue((short)0);

        builder.HasEnumCheck(e => e.VoucherType);
        builder.HasEnumCheck(e => e.TransactionType);
        builder.HasEnumCheck(e => e.PaymentMethod);
        builder.HasEnumCheck(e => e.Status);
        builder.HasEnumCheck(e => e.InmateType);

        builder.HasOne<Inmate>().WithMany().HasForeignKey(e => e.InmateId).OnDelete(DeleteBehavior.Restrict);

        // Statement scanning (per detainee, by date) and the posted-documents-by-date reports (Epic 6).
        builder.HasIndex(e => new { e.InmateId, e.VoucherDate, e.Id })
            .IncludeProperties(e => new { e.VoucherType, e.Amount, e.Status });
        builder.HasIndex(e => e.VoucherDate).HasFilter("[Status] = 2");
    }
}
