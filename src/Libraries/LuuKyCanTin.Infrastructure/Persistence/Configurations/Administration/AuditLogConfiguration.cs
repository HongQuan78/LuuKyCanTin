using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

/// <summary>
/// No FK from UserId: log rows must outlive any user clean-up, and inserts into the log stay cheap.
/// </summary>
internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");

        builder.Property(e => e.Workstation).HasMaxLength(100);
        builder.Property(e => e.Action)
            .HasConversion(a => AuditActionCodes.ToCode(a), c => AuditActionCodes.FromCode(c))
            .HasMaxLength(20)
            .IsUnicode(false);
        builder.HasEnumCheck(e => e.Action);
        builder.Property(e => e.TableName).HasMaxLength(50).IsUnicode(false);

        builder.HasIndex(e => new { e.TableName, e.RecordId });
        builder.HasIndex(e => e.OccurredAt);
    }
}
