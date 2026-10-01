using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public class SchemaVersionCheckerTests(SqlServerFixture fixture)
{
    private static Task<SchemaVersionCheckResult> CheckAsync(AppDbContext db) =>
        new SchemaVersionChecker(db, NullLogger<SchemaVersionChecker>.Instance).CheckAsync();

    [SqlServerFact]
    public async Task FullyMigratedDatabase_Matches()
    {
        await using var db = fixture.Database.CreateDbContext();

        var result = await CheckAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Matches);
        result.Actual.ShouldBe(db.Database.GetMigrations().Last());
    }

    [SqlServerFact]
    public async Task DatabaseThatDoesNotExist_IsMismatchWithNoActualVersion()
    {
        await using var database = new TestDatabase();
        await using var db = database.CreateDbContext();

        var result = await CheckAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task EmptyDatabaseWithoutHistoryTable_IsMismatchWithNoActualVersion()
    {
        await using var database = new TestDatabase();
        await database.CreateEmptyAsync();
        await using var db = database.CreateDbContext();

        var result = await CheckAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task DatabaseMigratedByANewerBuild_IsMismatch()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await database.ExecuteAsync(
            "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES (N'99991231000000_TuPhienBanMoiHon', N'10.0.0')");
        await using var db = database.CreateDbContext();

        var result = await CheckAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBe("99991231000000_TuPhienBanMoiHon");
    }
}
