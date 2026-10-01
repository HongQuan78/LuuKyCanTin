using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.IntegrationTests.Persistence.TestModel;

// A test-only entity that exercises every shared convention, since the real model has no tables yet.
public enum MauTrangThai : byte
{
    Nhap = 1,
    DaGhiSo = 2,
    DaHuy = 3,
}

public sealed class MauChungTu : AuditableEntity
{
    public int Id { get; set; }
    public decimal SoTien { get; set; }
    public DateOnly NgayChungTu { get; set; }
    public DateTime? ThoiDiemIn { get; set; }
    public string NoiDung { get; set; } = "";
    public MauTrangThai TrangThai { get; set; }
    public MauTrangThai? TrangThaiTruoc { get; set; }
}

public sealed class MauChungTuConfiguration : AuditableEntityConfiguration<MauChungTu>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MauChungTu> builder)
    {
        builder.HasEnumCheck(e => e.TrangThai);
        builder.HasEnumCheck(e => e.TrangThaiTruoc);
    }
}

public sealed class TestAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    public DbSet<MauChungTu> MauChungTu => Set<MauChungTu>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new MauChungTuConfiguration());
    }
}
