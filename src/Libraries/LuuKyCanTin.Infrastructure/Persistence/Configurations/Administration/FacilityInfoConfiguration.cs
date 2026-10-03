using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

internal sealed class FacilityInfoConfiguration : AuditableEntityConfiguration<FacilityInfo>
{
    // One reference-data row exists on every install; a fixed timestamp keeps the seed deterministic.
    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1);

    protected override void ConfigureEntity(EntityTypeBuilder<FacilityInfo> builder)
    {
        builder.ToTable("FacilityInfo", t => t.HasCheckConstraint("CK_FacilityInfo_Id", "[Id] = 1"));

        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.ParentAgencyName).HasMaxLength(200);
        builder.Property(e => e.FacilityName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Address).HasMaxLength(300).IsRequired();

        // An empty row for the admin to fill (DB design, seed data). The demo seeder fills it on a dev machine.
        builder.HasData(new FacilityInfo
        {
            Id = 1,
            ParentAgencyName = null,
            FacilityName = "",
            Address = "",
            CreatedAt = SeedCreatedAt,
            CreatedById = 0,
        });
    }
}
