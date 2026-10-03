using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.Data.SqlClient;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

/// <summary>Every Domain enum column must have a deployed CHECK that allows exactly the enum's values.</summary>
[Collection(SqlServerCollection.Name)]
public sealed class EnumCheckConstraintTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task Verify_EveryEnumColumnInModel_HasNoProblems()
    {
        await using var db = fixture.Database.CreateDbContext();

        var problems = EnumCheckVerifier.Verify(EnumModel.GetEnumColumns(db), await GetCheckConstraintsAsync(fixture.Database));

        problems.ShouldBeEmpty();
    }

    // The real model has no enum column yet, so prove the check against constraints SQL Server actually stored.
    [SqlServerFact]
    public async Task Verify_DeployedMismatchedConstraints_ReportsEachKind()
    {
        await using var database = new TestDatabase();
        await using (var db = new TestAppDbContext(database.Options))
        {
            await db.Database.EnsureCreatedAsync();
            EnumCheckVerifier.Verify(EnumModel.GetEnumColumns(db), await GetCheckConstraintsAsync(database)).ShouldBeEmpty();
        }
        await database.ExecuteAsync("ALTER TABLE [SampleVoucher] DROP CONSTRAINT [CK_SampleVoucher_PreviousStatus]");
        var checks = await GetCheckConstraintsAsync(database);

        EnumCheckVerifier.Verify([CreateColumn("Status", typeof(SampleStatusMissingValue))], checks)
            .ShouldHaveSingleItem().ShouldContain("CHECK allows 3, which SampleStatusMissingValue does not define");
        EnumCheckVerifier.Verify([CreateColumn("Status", typeof(SampleStatusExtraValue))], checks)
            .ShouldHaveSingleItem().ShouldContain("enum value New=4 is not allowed by the CHECK constraint");
        EnumCheckVerifier.Verify([CreateColumn("PreviousStatus", typeof(SampleStatus))], checks)
            .ShouldHaveSingleItem().ShouldContain("no CHECK constraint");
    }

    private enum SampleStatusMissingValue : byte { Draft = 1, Posted = 2 }

    private enum SampleStatusExtraValue : byte { Draft = 1, Posted = 2, Cancelled = 3, New = 4 }

    private static EnumColumn CreateColumn(string column, Type enumType) => new(EnumModel.DefaultSchema, "SampleVoucher", column, enumType);

    private static async Task<IReadOnlyList<DeployedCheck>> GetCheckConstraintsAsync(TestDatabase database)
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
