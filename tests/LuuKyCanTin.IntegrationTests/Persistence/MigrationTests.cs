using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

[Collection(LocalDbCollection.Name)]
public class MigrationTests(LocalDbFixture fixture)
{
    private const string CollationSql = "SELECT CONVERT(nvarchar(128), DATABASEPROPERTYEX(DB_NAME(), 'Collation'))";

    [LocalDbFact]
    public async Task Migrate_EmptyServer_CreatesDatabaseWithVietnameseCollation()
    {
        (await fixture.Database.ScalarAsync(CollationSql)).ShouldBe(AppDbContext.Collation);
    }

    [LocalDbFact]
    public async Task Migrate_AppliesEveryMigration()
    {
        await using var db = fixture.Database.CreateDbContext();

        (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
        (await db.Database.GetAppliedMigrationsAsync()).ShouldBe(db.Database.GetMigrations());
    }

    [LocalDbFact]
    public async Task Migrate_DatabaseCreatedByAdmin_ChangesItsCollation()
    {
        await using var database = new TestDatabase();
        await database.CreateEmptyAsync();
        (await database.ScalarAsync(CollationSql)).ShouldNotBe(AppDbContext.Collation);

        await database.MigrateAsync();

        (await database.ScalarAsync(CollationSql)).ShouldBe(AppDbContext.Collation);
    }
}
