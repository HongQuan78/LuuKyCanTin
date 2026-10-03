using LuuKyCanTin.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Base configuration for every table with audit columns. The user-id columns deliberately have no
/// foreign key to NguoiDung: one FK per table buys nothing over the audit log and risks multiple cascade paths.
/// </summary>
public abstract class AuditableEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : AuditableEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
        builder.Property(e => e.NgaySua).HasColumnType("datetime2(0)");
        builder.Property(e => e.RowVer).IsRowVersion();

        CauHinhRieng(builder);
    }

    protected abstract void CauHinhRieng(EntityTypeBuilder<T> builder);
}
