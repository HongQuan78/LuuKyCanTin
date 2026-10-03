using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public sealed class MigrationTests(SqlServerFixture fixture)
{
    private const string CollationSql = "SELECT CONVERT(nvarchar(128), DATABASEPROPERTYEX(DB_NAME(), 'Collation'))";

    [SqlServerFact]
    public async Task Migrate_EmptyServer_CreatesDatabaseWithVietnameseCollation()
    {
        (await fixture.Database.GetScalarAsync(CollationSql)).ShouldBe(AppDbContext.Collation);
    }

    [SqlServerFact]
    public async Task Migrate_NewDatabase_AppliesEveryMigration()
    {
        await using var db = fixture.Database.CreateDbContext();

        (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
        (await db.Database.GetAppliedMigrationsAsync()).ShouldBe(db.Database.GetMigrations());
    }

    [SqlServerFact]
    public async Task Migrate_DatabaseCreatedByAdmin_ChangesItsCollation()
    {
        await using var database = new TestDatabase();
        await database.CreateEmptyAsync();
        (await database.GetScalarAsync(CollationSql)).ShouldNotBe(AppDbContext.Collation);

        await database.MigrateAsync();

        (await database.GetScalarAsync(CollationSql)).ShouldBe(AppDbContext.Collation);
    }
}
