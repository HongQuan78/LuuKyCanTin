using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public sealed class SchemaVersionCheckerTests(SqlServerFixture fixture)
{
    private static Task<SchemaVersionCheckResult> KiemTraAsync(AppDbContext db) =>
        new SchemaVersionChecker(db, NullLogger<SchemaVersionChecker>.Instance).KiemTraAsync();

    [SqlServerFact]
    public async Task KiemTra_FullyMigratedDatabase_Matches()
    {
        await using var db = fixture.Database.TaoDbContext();

        var result = await KiemTraAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Matches);
        result.Actual.ShouldBe(db.Database.GetMigrations().Last());
    }

    [SqlServerFact]
    public async Task KiemTra_DatabaseDoesNotExist_IsMismatchWithNoActualVersion()
    {
        await using var database = new TestDatabase();
        await using var db = database.TaoDbContext();

        var result = await KiemTraAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task KiemTra_EmptyDatabaseWithoutHistoryTable_IsMismatchWithNoActualVersion()
    {
        await using var database = new TestDatabase();
        await database.TaoRongAsync();
        await using var db = database.TaoDbContext();

        var result = await KiemTraAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task KiemTra_DatabaseMigratedByNewerBuild_IsMismatch()
    {
        await using var database = new TestDatabase();
        await database.ApDungMigrationAsync();
        await database.ThucThiAsync(
            "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES (N'99991231000000_TuPhienBanMoiHon', N'10.0.0')");
        await using var db = database.TaoDbContext();

        var result = await KiemTraAsync(db);

        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
        result.Actual.ShouldBe("99991231000000_TuPhienBanMoiHon");
    }

    [SqlServerFact]
    public async Task KiemTra_MiddleMigrationMissingFromHistory_IsMismatch()
    {
        // What a branch merge leaves behind: a migration that sorts before the last applied one was never applied.
        await using var database = new TestDatabase();
        await database.ApDungMigrationAsync();
        await using var db = database.TaoDbContext();
        var giua = db.Database.GetMigrations().Skip(1).First();
        await database.ThucThiAsync($"DELETE FROM __EFMigrationsHistory WHERE MigrationId = N'{giua}'");

        var result = await KiemTraAsync(db);

        result.Actual.ShouldBe(result.Expected);
        result.Status.ShouldBe(SchemaVersionStatus.Mismatch);
    }
}
