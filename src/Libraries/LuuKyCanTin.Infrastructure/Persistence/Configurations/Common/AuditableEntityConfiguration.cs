using LuuKyCanTin.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Base configuration for every table with audit columns. The user-id columns deliberately have no
/// foreign key to User: one FK per table buys nothing over the audit log and risks multiple cascade paths.
/// </summary>
public abstract class AuditableEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : AuditableEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)");
        builder.Property(e => e.ModifiedAt).HasColumnType("datetime2(0)");
        builder.Property(e => e.RowVer).IsRowVersion();

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
}
