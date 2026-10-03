using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Persistence.Interceptors;
using LuuKyCanTin.IntegrationTests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.IntegrationTests.Common;

/// <summary>
/// A database migrated with the real schema, with contexts wired like the app's (audit interceptor included), for
/// testing Application services end to end. Tests share it, so each one uses its own unique codes.
/// </summary>
public sealed class AppDatabaseFixture : IAsyncLifetime
{
    public TestDatabase Database { get; } = new();

    public FakeClock Clock { get; } = new(new DateTime(2026, 10, 2, 9, 0, 0));

    public CurrentUserSession User { get; } = new();

    /// <summary>A new context per call, as each operation in the app gets its own DI scope.</summary>
    public AppDbContext CreateAuditedContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(Database.ConnectionString)
        .AddInterceptors(new AuditInterceptor(Clock, User, new AuditLogFactory(Clock, User)))
        .Options);

    /// <summary>Signs the account in with the codes its roles grant, exactly as SignInService loads them.</summary>
    public async Task SignInAsync(User user, CancellationToken ct = default)
    {
        await using var db = Database.CreateDbContext();
        var permissionCodes = await new UserStore(db).GetPermissionCodesAsync(user.Id, ct);
        User.SignIn(user.Id, user.UserName, user.OfficerId, user.UserName, permissionCodes);
    }

    public Task InitializeAsync() => SqlServerFactAttribute.CanRun ? Database.MigrateAsync() : Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (SqlServerFactAttribute.CanRun)
            await Database.DisposeAsync();
    }
}
