using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

// Model-only: the map must point at names that exist, or the audit viewer would translate old rows into nothing.
public sealed class LegacyAuditNamesTests
{
    // Keys of explicit event payloads, which are not columns.
    private static readonly string[] PayloadProperties = ["Event", "Permissions"];

    private static readonly DbContextOptions<AppDbContext> Options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    private static Dictionary<string, HashSet<string>> GetColumnsByTable()
    {
        using var db = new AppDbContext(Options);
        return db.GetService<IDesignTimeModel>().Model.GetEntityTypes().ToDictionary(
            e => e.GetTableName()!,
            e => e.GetProperties()
                .Select(p => p.GetColumnName(StoreObjectIdentifier.Table(e.GetTableName()!, e.GetSchema()))!)
                .ToHashSet(StringComparer.Ordinal));
    }

    [Fact]
    public void Tables_EveryCurrentTable_HasItsLegacyName()
    {
        LegacyAuditNames.Tables.Values.ShouldBe(GetColumnsByTable().Keys, ignoreOrder: true);
    }

    [Fact]
    public void Properties_EveryNewName_IsAColumnOfItsTable()
    {
        var columns = GetColumnsByTable();

        foreach (var (table, map) in LegacyAuditNames.Properties)
        {
            columns.ShouldContainKey(table);
            map.Values.Where(n => !PayloadProperties.Contains(n)).ShouldAllBe(n => columns[table].Contains(n));
        }
    }
}
