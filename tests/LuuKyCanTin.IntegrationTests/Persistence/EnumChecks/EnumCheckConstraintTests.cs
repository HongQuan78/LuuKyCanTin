using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.Data.SqlClient;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

/// <summary>Every Domain enum column must have a deployed CHECK that allows exactly the enum's values.</summary>
[Collection(SqlServerCollection.Name)]
public class EnumCheckConstraintTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task EveryEnumColumn_MatchesItsDeployedCheckConstraint()
    {
        await using var db = fixture.Database.CreateDbContext();

        var problems = EnumCheckVerifier.FindProblems(EnumModel.EnumColumnsOf(db), await ReadCheckConstraintsAsync(fixture.Database));

        problems.ShouldBeEmpty();
    }

    // The real model has no enum column yet, so prove the check against constraints SQL Server actually stored.
    [SqlServerFact]
    public async Task SelfTest_DetectsEachKindOfMismatchInADeployedDatabase()
    {
        await using var database = new TestDatabase();
        await using (var db = new TestAppDbContext(database.Options))
        {
            await db.Database.EnsureCreatedAsync();
            EnumCheckVerifier.FindProblems(EnumModel.EnumColumnsOf(db), await ReadCheckConstraintsAsync(database)).ShouldBeEmpty();
        }
        await database.ExecuteAsync("ALTER TABLE [MauChungTu] DROP CONSTRAINT [CK_MauChungTu_TrangThaiTruoc]");
        var checks = await ReadCheckConstraintsAsync(database);

        EnumCheckVerifier.FindProblems([Column("TrangThai", typeof(MauTrangThaiThieu))], checks)
            .ShouldHaveSingleItem().ShouldContain("CHECK allows 3, which MauTrangThaiThieu does not define");
        EnumCheckVerifier.FindProblems([Column("TrangThai", typeof(MauTrangThaiThua))], checks)
            .ShouldHaveSingleItem().ShouldContain("enum value Moi=4 is not allowed by the CHECK constraint");
        EnumCheckVerifier.FindProblems([Column("TrangThaiTruoc", typeof(MauTrangThai))], checks)
            .ShouldHaveSingleItem().ShouldContain("no CHECK constraint");
    }

    private enum MauTrangThaiThieu : byte { Nhap = 1, DaGhiSo = 2 }

    private enum MauTrangThaiThua : byte { Nhap = 1, DaGhiSo = 2, DaHuy = 3, Moi = 4 }

    private static EnumColumn Column(string column, Type enumType) => new(EnumModel.DefaultSchema, "MauChungTu", column, enumType);

    private static async Task<IReadOnlyList<DeployedCheck>> ReadCheckConstraintsAsync(TestDatabase database)
    {
        const string sql = """
            SELECT s.name, t.name, cc.definition
            FROM sys.check_constraints cc
            JOIN sys.tables t ON t.object_id = cc.parent_object_id
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            """;

        await using var connection = new SqlConnection(database.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var checks = new List<DeployedCheck>();
        while (await reader.ReadAsync())
            checks.Add(new DeployedCheck(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        return checks;
    }
}
