using LuuKyCanTin.Infrastructure.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Persistence.Interceptors;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using LuuKyCanTin.IntegrationTests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.IntegrationTests.Persistence.Audit;

/// <summary>A database holding the test voucher table and the audit log, with contexts wired like the app's.</summary>
public sealed class AuditDatabaseFixture : IAsyncLifetime
{
    public TestDatabase Database { get; } = new();

    public FakeClock Clock { get; } = new(new DateTime(2026, 10, 1, 8, 30, 0));

    public CurrentUserSession User { get; } = new();

    public NhatKyFactory NhatKyFactory => new(Clock, User);

    /// <summary>A context with the audit interceptor, as the app's DI builds it: one interceptor per context.</summary>
    public TestAppDbContext CreateAuditedContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(Database.ConnectionString)
        .AddInterceptors(new AuditInterceptor(Clock, User, NhatKyFactory))
        .Options);

    /// <summary>A context without the interceptor, for reading back what was really stored.</summary>
    public TestAppDbContext CreatePlainContext() => new(Database.Options);

    public async Task InitializeAsync()
    {
        if (!SqlServerFactAttribute.DuocPhepChay)
            return;

        await using var db = CreatePlainContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (SqlServerFactAttribute.DuocPhepChay)
            await Database.DisposeAsync();
    }
}
