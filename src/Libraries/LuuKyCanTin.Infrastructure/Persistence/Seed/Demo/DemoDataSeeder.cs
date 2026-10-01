using LuuKyCanTin.Application.HeThong;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;

internal sealed class DemoDataSeeder(AppDbContext db) : IDemoDataSeeder
{
    public Task<DemoSeedDecision> SeedAsync(bool isDevelopment, string? confirmedDatabaseName, CancellationToken cancellationToken = default)
    {
        var decision = DemoSeedPolicy.Decide(isDevelopment, confirmedDatabaseName, db.Database.GetDbConnection().Database);

        // No demo data yet: later stories add it here once their tables exist.
        return Task.FromResult(decision);
    }
}
