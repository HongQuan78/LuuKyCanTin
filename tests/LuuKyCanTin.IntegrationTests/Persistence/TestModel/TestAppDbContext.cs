using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.IntegrationTests.Persistence.TestModel;

public sealed class TestAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    public DbSet<SampleVoucher> SampleVoucher => Set<SampleVoucher>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new SampleVoucherConfiguration());
    }
}
