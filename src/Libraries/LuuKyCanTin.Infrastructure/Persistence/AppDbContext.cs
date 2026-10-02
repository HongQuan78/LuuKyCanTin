using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Domain.LuuKy;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public const string Collation = "Vietnamese_CI_AI";

    public DbSet<NhatKyThaoTac> NhatKyThaoTac => Set<NhatKyThaoTac>();

    public DbSet<NguoiDung> NguoiDung => Set<NguoiDung>();

    public DbSet<ThongTinDonVi> ThongTinDonVi => Set<ThongTinDonVi>();

    public DbSet<DoiTuong> DoiTuong => Set<DoiTuong>();

    public DbSet<ChungTuLuuKy> ChungTuLuuKy => Set<ChungTuLuuKy>();

    public DbSet<DemSoChungTu> DemSoChungTu => Set<DemSoChungTu>();

    async Task<IAppTransaction> IAppDbContext.BeginTransactionAsync(CancellationToken ct)
        => new AppTransaction(await Database.BeginTransactionAsync(ct));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Must stay in the first migration: a database collation cannot change once columns depend on it.
        modelBuilder.UseCollation(Collation);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Money is whole đồng; quantities and average costs override the scale per property.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 0);
        configurationBuilder.Properties<DateOnly>().HaveColumnType("date");
        configurationBuilder.Properties<DateTime>().HavePrecision(0);
        configurationBuilder.Properties<string>().AreUnicode();
        // Enums are declared ': byte', which EF already maps to tinyint; every enum column also needs HasEnumCheck.
    }
}
