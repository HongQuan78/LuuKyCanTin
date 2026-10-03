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
    public async Task ApDungMigration_EmptyServer_CreatesDatabaseWithVietnameseCollation()
    {
        (await fixture.Database.LayGiaTriAsync(CollationSql)).ShouldBe(AppDbContext.Collation);
    }

    [SqlServerFact]
    public async Task ApDungMigration_NewDatabase_AppliesEveryMigration()
    {
        await using var db = fixture.Database.TaoDbContext();

        (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
        (await db.Database.GetAppliedMigrationsAsync()).ShouldBe(db.Database.GetMigrations());
    }

    [SqlServerFact]
    public async Task ApDungMigration_DatabaseCreatedByAdmin_ChangesItsCollation()
    {
        await using var database = new TestDatabase();
        await database.TaoRongAsync();
        (await database.LayGiaTriAsync(CollationSql)).ShouldNotBe(AppDbContext.Collation);

        await database.ApDungMigrationAsync();

        (await database.LayGiaTriAsync(CollationSql)).ShouldBe(AppDbContext.Collation);
    }
}
