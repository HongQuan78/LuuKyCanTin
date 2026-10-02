using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.HeThong;

/// <summary>Reference data: it changes only when a migration adds a permission.</summary>
internal sealed class QuyenConfiguration : IEntityTypeConfiguration<Quyen>
{
    public void Configure(EntityTypeBuilder<Quyen> builder)
    {
        builder.ToTable("Quyen");

        // The catalogue decides the ids: they are the HasData keys and stay stable for good.
        builder.Property(q => q.Id).ValueGeneratedNever();
        builder.Property(q => q.Ma).HasMaxLength(50).IsUnicode(false).IsRequired();
        builder.HasIndex(q => q.Ma).IsUnique();
        builder.Property(q => q.Ten).HasMaxLength(150).IsRequired();
        builder.Property(q => q.Module).HasMaxLength(10).IsUnicode(false).IsRequired();

        builder.HasData(MaQuyen.TatCa
            .Select(q => new Quyen { Id = q.Id, Ma = q.Ma, Ten = q.Ten, Module = q.Module })
            .ToArray());
    }
}
