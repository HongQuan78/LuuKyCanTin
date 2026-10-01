using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "LuuKyCanTin";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Missing connection string 'ConnectionStrings:{ConnectionStringName}'. Set ConnectionStrings__{ConnectionStringName} "
                + "in the .env file next to the exe (see .env.example) or as an environment variable.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ISchemaVersionChecker, SchemaVersionChecker>();
        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IDemoDataSeeder, DemoDataSeeder>();

        return services;
    }
}
