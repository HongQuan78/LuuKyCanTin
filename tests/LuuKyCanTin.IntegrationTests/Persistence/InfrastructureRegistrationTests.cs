using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Infrastructure;
using LuuKyCanTin.Infrastructure.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Persistence.Interceptors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public class InfrastructureRegistrationTests
{
    private const string ConnectionString = "Server=.\\SQLEXPRESS;Database=LuuKyCanTin;Integrated Security=true";

    // EF normalizes the connection string and appends an Application Name, so compare what it points at.
    private static (string Server, string Database) TargetOf(DbContext db)
    {
        var builder = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        return (builder.DataSource, builder.InitialCatalog);
    }

    private static ServiceProvider BuildProvider(string? connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:LuuKyCanTin"] = connectionString })
            .Build();
        return new ServiceCollection().AddLogging().AddInfrastructure(configuration).BuildServiceProvider(validateScopes: true);
    }

    [Fact]
    public void AddInfrastructure_RegistersDbContextPerScopeWithConfiguredConnection()
    {
        using var provider = BuildProvider(ConnectionString);
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var db = scope1.ServiceProvider.GetRequiredService<AppDbContext>();

        db.ShouldBeSameAs(scope1.ServiceProvider.GetRequiredService<AppDbContext>());
        db.ShouldNotBeSameAs(scope2.ServiceProvider.GetRequiredService<AppDbContext>());
        TargetOf(db).ShouldBe((@".\SQLEXPRESS", "LuuKyCanTin"));
    }

    [Theory]
    [InlineData(typeof(ISchemaVersionChecker))]
    [InlineData(typeof(IDatabaseMigrator))]
    [InlineData(typeof(IDemoDataSeeder))]
    [InlineData(typeof(IGhiNhatKy))]
    [InlineData(typeof(IClock))]
    [InlineData(typeof(ICurrentUser))]
    public void AddInfrastructure_RegistersDatabaseServices(Type service)
    {
        using var provider = BuildProvider(ConnectionString);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetService(service).ShouldNotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ExposesTheScopesDbContextAsTheUnitOfWork()
    {
        using var provider = BuildProvider(ConnectionString);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IAppDbContext>()
            .ShouldBeSameAs(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    [Fact]
    public void AddInfrastructure_SharesOneSignedInUserAcrossScopes()
    {
        using var provider = BuildProvider(ConnectionString);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<ICurrentUser>()
            .ShouldBeSameAs(provider.GetRequiredService<CurrentUserSession>());
    }

    [Fact]
    public void AddInfrastructure_GivesEachDbContextItsOwnAuditInterceptor()
    {
        using var provider = BuildProvider(ConnectionString);
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var interceptor1 = InterceptorOf(scope1.ServiceProvider.GetRequiredService<AppDbContext>());
        var interceptor2 = InterceptorOf(scope2.ServiceProvider.GetRequiredService<AppDbContext>());

        interceptor1.ShouldNotBeNull();
        interceptor1.ShouldNotBeSameAs(interceptor2);
    }

    private static AuditInterceptor? InterceptorOf(DbContext db) => db.GetService<IDbContextOptions>()
        .Extensions.OfType<CoreOptionsExtension>().Single()
        .Interceptors?.OfType<AuditInterceptor>().SingleOrDefault();

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void AddInfrastructure_WithoutConnectionString_FailsFast(string? connectionString)
    {
        Should.Throw<InvalidOperationException>(() => BuildProvider(connectionString))
            .Message.ShouldContain("ConnectionStrings:LuuKyCanTin");
    }

    [Fact]
    public void DesignTimeFactory_UsesEnvironmentVariableWhenSet()
    {
        var previous = Environment.GetEnvironmentVariable(DesignTimeDbContextFactory.ConnectionStringVariable);
        try
        {
            Environment.SetEnvironmentVariable(DesignTimeDbContextFactory.ConnectionStringVariable, ConnectionString);

            using var db = new DesignTimeDbContextFactory().CreateDbContext([]);

            TargetOf(db).ShouldBe((@".\SQLEXPRESS", "LuuKyCanTin"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(DesignTimeDbContextFactory.ConnectionStringVariable, previous);
        }
    }
}
