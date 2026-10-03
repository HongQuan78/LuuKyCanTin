using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public sealed class SchemaVersionCheckerTests(SqlServerFixture fixture)
{
    private static Task<SchemaVersionCheckResult> CheckAsync(AppDbContext db) =>
        new SchemaVersionChecker(db, NullLogger<SchemaVersionChecker>.Instance).CheckAsync();

    [SqlServerFact]
    public async Task Check_FullyMigratedDatabase_Matches()
    {
        await using var db = fixture.Database.CreateDbContext();

        var result = await CheckAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Matches);
        result.Actual.ShouldBe(db.Database.GetMigrations().Last());
    }

    [SqlServerFact]
    public async Task Check_DatabaseDoesNotExist_IsMismatchWithNoActualVersion()
    {
        await using var database = new TestDatabase();
        await using var db = database.CreateDbContext();

        var result = await CheckAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task Check_EmptyDatabaseWithoutHistoryTable_IsMismatchWithNoActualVersion()
    {
        await using var database = new TestDatabase();
        await database.CreateEmptyAsync();
        await using var db = database.CreateDbContext();

        var result = await CheckAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task Check_DatabaseMigratedByNewerBuild_IsMismatch()
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

    [SqlServerFact]
    public async Task Check_MiddleMigrationMissingFromHistory_IsMismatch()
    {
        // What a branch merge leaves behind: a migration that sorts before the last applied one was never applied.
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateDbContext();
        var middle = db.Database.GetMigrations().Skip(1).First();
        await database.ExecuteAsync($"DELETE FROM __EFMigrationsHistory WHERE MigrationId = N'{middle}'");

        var result = await CheckAsync(db);

        result.Actual.ShouldBe(result.Expected);
        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
    }
}
