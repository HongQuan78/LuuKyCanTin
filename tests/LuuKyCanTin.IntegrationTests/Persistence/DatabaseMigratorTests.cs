using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public sealed class DatabaseMigratorTests
{
    [SqlServerFact]
    public async Task ApDungMigration_NewDatabase_AppliesAndReportsEveryMigration()
    {
        await using var database = new TestDatabase();
        await using var db = database.TaoDbContext();

        var danhSachDaApDung = await new DatabaseMigrator(db).ApDungMigrationAsync();

        danhSachDaApDung.ShouldBe(db.Database.GetMigrations());
        (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
    }

    [SqlServerFact]
    public async Task ApDungMigration_AlreadyCurrent_ReportsNothing()
    {
        await using var database = new TestDatabase();
        await database.ApDungMigrationAsync();
        await using var db = database.TaoDbContext();

        (await new DatabaseMigrator(db).ApDungMigrationAsync()).ShouldBeEmpty();
    }
}
