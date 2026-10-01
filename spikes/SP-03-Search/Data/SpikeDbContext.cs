using Microsoft.EntityFrameworkCore;

namespace SP03Search.Data;

internal sealed class SpikeDbContext : DbContext
{
    public SpikeDbContext(DbContextOptions<SpikeDbContext> options)
        : base(options)
    {
    }

    public DbSet<DoiTuongSpike> DoiTuongSpike => Set<DoiTuongSpike>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DoiTuongSpike>();
        entity.ToTable("DoiTuongSpike", "dbo");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.MaSo).HasColumnType("varchar(30)").IsRequired();
        entity.Property(x => x.HoTen).HasColumnType("nvarchar(100)").IsRequired();
        entity.Property(x => x.HoTenKhongDau).HasColumnType("nvarchar(100)");
        entity.Property(x => x.NamSinh).HasColumnType("smallint");
        entity.Property(x => x.BuongGiam).HasColumnType("nvarchar(50)");
    }
}
