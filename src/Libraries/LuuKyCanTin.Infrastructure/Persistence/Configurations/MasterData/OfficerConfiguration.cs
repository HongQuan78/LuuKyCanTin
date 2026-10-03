using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.MasterData;

internal sealed class OfficerConfiguration : AuditableEntityConfiguration<Officer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Officer> builder)
    {
        builder.ToTable("Officer");

        builder.Property(e => e.OfficerCode).HasMaxLength(SaveOfficerRequestValidator.OfficerCodeMaxLength).IsUnicode(false);
        builder.HasIndex(e => e.OfficerCode).IsUnique();

        builder.Property(e => e.FullName).HasMaxLength(SaveOfficerRequestValidator.FullNameMaxLength).UseCollation(AppDbContext.SearchCollation);
        builder.HasIndex(e => e.FullName).HasDatabaseName("IX_Officer_FullName");

        builder.Property(e => e.Position).HasMaxLength(SaveOfficerRequestValidator.PositionMaxLength);

        builder.Property(e => e.IsSupervisingOfficer).HasDefaultValue(false);
        // EF leaves a property equal to its sentinel out of the INSERT. With the default sentinel (false), adding
        // someone who has already left would silently store the database default, true.
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasSentinel(true);
    }
}
