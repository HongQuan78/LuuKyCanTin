using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.MasterData;

internal sealed class InmateConfiguration : AuditableEntityConfiguration<Inmate>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Inmate> builder)
    {
        builder.ToTable("Inmate", t =>
        {
            t.HasCheckConstraint("CK_Inmate_CustodyBalance", "[CustodyBalance] >= 0");
            t.HasCheckConstraint("CK_Inmate_ReleaseDate", "[Status] = 1 OR [ReleaseDate] IS NOT NULL");
        });

        builder.Property(e => e.InmateCode).HasMaxLength(30).IsUnicode(false).IsRequired();
        builder.HasIndex(e => e.InmateCode).IsUnique();

        builder.Property(e => e.FullName).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.FullName);

        builder.Property(e => e.BirthYear).HasColumnType("smallint");
        builder.Property(e => e.Cell).HasMaxLength(50);

        // The sentinel is the default itself; a new detainee explicitly set to InCustody still stores 1.
        builder.Property(e => e.Status).HasDefaultValue(InmateStatus.InCustody).HasSentinel(InmateStatus.InCustody);
        builder.HasEnumCheck(e => e.Status);
        builder.HasEnumCheck(e => e.InmateType);

        // The balance is only ever written by the ledger engine's raw conditional UPDATE. Ignoring the
        // property on both saves makes that rule structural: EF never emits this column in an INSERT/UPDATE.
        var balance = builder.Property(e => e.CustodyBalance).HasPrecision(18, 0).HasDefaultValue(0m);
        balance.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        balance.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
