using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public sealed class DatabaseMigratorTests
{
    [SqlServerFact]
    public async Task Migrate_NewDatabase_AppliesAndReportsEveryMigration()
    {
        await using var database = new TestDatabase();
        await using var db = database.CreateDbContext();

        var applied = await new DatabaseMigrator(db).MigrateAsync();

        applied.ShouldBe(db.Database.GetMigrations());
        (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
    }

    [SqlServerFact]
    public async Task Migrate_AlreadyCurrent_ReportsNothing()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateDbContext();

        (await new DatabaseMigrator(db).MigrateAsync()).ShouldBeEmpty();
    }
}
