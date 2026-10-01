using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LuuKyCanTin.Infrastructure.Persistence;

/// <summary>Lets <c>dotnet ef</c> build the context without starting the WinForms host.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    // Same variable the app reads (from the environment or .env), so one setting serves both.
    public const string ConnectionStringVariable = "ConnectionStrings__LuuKyCanTin";

    private const string LocalDbConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=LuuKyCanTin;Integrated Security=true;TrustServerCertificate=true";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? LocalDbConnectionString;
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;
        return new AppDbContext(options);
    }
}
