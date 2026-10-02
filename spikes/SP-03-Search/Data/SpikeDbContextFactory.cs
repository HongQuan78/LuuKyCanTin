using Microsoft.EntityFrameworkCore;

namespace SP03Search.Data;

/// <summary>
/// Builds the EF model once and hands out a fresh <see cref="SpikeDbContext"/> per operation,
/// which is how the product keeps a Form from holding a DbContext (CLAUDE.md).
/// </summary>
internal sealed class SpikeDbContextFactory
{
    private readonly DbContextOptions<SpikeDbContext> _options;

    public SpikeDbContextFactory(string connectionString)
    {
        _options = new DbContextOptionsBuilder<SpikeDbContext>()
            .UseSqlServer(connectionString, sql => sql.CommandTimeout(60))
            .Options;
    }

    public SpikeDbContext CreateDbContext() => new(_options);

    public void WarmUp()
    {
        using var context = CreateDbContext();
        _ = context.Model;
    }
}
