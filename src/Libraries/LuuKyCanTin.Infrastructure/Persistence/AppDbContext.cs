using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public const string Collation = "Vietnamese_CI_AI";

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
