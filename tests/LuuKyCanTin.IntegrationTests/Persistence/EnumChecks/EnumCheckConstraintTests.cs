using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

/// <summary>Every Domain enum column must be tinyint with a deployed CHECK that allows exactly the enum's values.</summary>
[Collection(LocalDbCollection.Name)]
public class EnumCheckConstraintTests(LocalDbFixture fixture)
{
    private const string DefaultSchema = "dbo";

    private static readonly DbContextOptions<AppDbContext> ModelOnlyOptions = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    [LocalDbFact]
    public async Task EveryEnumColumn_MatchesItsDeployedCheckConstraint()
    {
        await using var db = fixture.Database.CreateDbContext();

        var problems = EnumCheckVerifier.FindProblems(EnumColumnsOf(db), await ReadCheckConstraintsAsync(fixture.Database));

        problems.ShouldBeEmpty();
    }

    [Fact]
    public void EveryEnumColumn_IsTinyint()
    {
        using var db = new AppDbContext(ModelOnlyOptions);

        EnumPropertiesOf(db)
            .Where(p => p.GetColumnType() != "tinyint")
            .Select(p => $"{p.DeclaringType.DisplayName()}.{p.Name} is {p.GetColumnType()}; declare the enum ': byte'")
            .ShouldBeEmpty();
    }

    [Fact]
    public void ModelWalk_FindsNullableAndNonNullableEnumColumns()
    {
        using var db = new TestAppDbContext(ModelOnlyOptions);

        EnumColumnsOf(db).ShouldBe(
            [Column("TrangThai", typeof(MauTrangThai)), Column("TrangThaiTruoc", typeof(MauTrangThai))],
            ignoreOrder: true);
    }

    // The real model has no enum column yet, so prove the check against constraints SQL Server actually stored.
    [LocalDbFact]
    public async Task SelfTest_DetectsEachKindOfMismatchInADeployedDatabase()
    {
        await using var database = new TestDatabase();
        await using (var db = new TestAppDbContext(database.Options))
        {
            await db.Database.EnsureCreatedAsync();
            EnumCheckVerifier.FindProblems(EnumColumnsOf(db), await ReadCheckConstraintsAsync(database)).ShouldBeEmpty();
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

    private static EnumColumn Column(string column, Type enumType) => new(DefaultSchema, "MauChungTu", column, enumType);

    private static IEnumerable<IProperty> EnumPropertiesOf(DbContext db) => db.GetService<IDesignTimeModel>().Model
        .GetEntityTypes()
        .SelectMany(e => e.GetProperties())
        .Where(p => (Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType).IsEnum);

    private static List<EnumColumn> EnumColumnsOf(DbContext db) => EnumPropertiesOf(db)
        .Select(p =>
        {
            var entity = (IEntityType)p.DeclaringType;
            var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            return new EnumColumn(
                entity.GetSchema() ?? DefaultSchema,
                table.Name,
                p.GetColumnName(table)!,
                Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType);
        })
        .ToList();

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
