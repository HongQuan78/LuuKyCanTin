using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

/// <summary>Finds the enum-typed columns of an EF model.</summary>
public static class EnumModel
{
    public const string DefaultSchema = "dbo";

    public static IEnumerable<IProperty> EnumPropertiesOf(DbContext db) => db.GetService<IDesignTimeModel>().Model
        .GetEntityTypes()
        .SelectMany(e => e.GetProperties())
        .Where(p => (Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType).IsEnum);

    /// <summary>True for an enum converted to its name (<c>HasConversion&lt;string&gt;()</c>) rather than stored as a number.</summary>
    public static bool IsStoredAsName(IProperty property) => property.GetProviderClrType() == typeof(string);

    public static List<EnumColumn> EnumColumnsOf(DbContext db) => EnumPropertiesOf(db)
        .Select(p =>
        {
            var entity = (IEntityType)p.DeclaringType;
            var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            return new EnumColumn(
                entity.GetSchema() ?? DefaultSchema,
                table.Name,
                p.GetColumnName(table)!,
                Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType,
                IsStoredAsName(p));
        })
        .ToList();
}
