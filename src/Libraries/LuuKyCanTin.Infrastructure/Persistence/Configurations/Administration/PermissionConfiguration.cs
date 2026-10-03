using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

/// <summary>Reference data: it changes only when a migration adds a permission.</summary>
internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permission");

        // The catalogue decides the ids: they are the HasData keys and stay stable for good.
        builder.Property(q => q.Id).ValueGeneratedNever();
        builder.Property(q => q.Code).HasMaxLength(50).IsUnicode(false).IsRequired();
        builder.HasIndex(q => q.Code).IsUnique();
        builder.Property(q => q.Name).HasMaxLength(150).IsRequired();
        builder.Property(q => q.Module).HasMaxLength(10).IsUnicode(false).IsRequired();

        builder.HasData(PermissionCodes.All
            .Select(q => new Permission { Id = q.Id, Code = q.Code, Name = q.Name, Module = q.Module })
            .ToArray());
    }
}
