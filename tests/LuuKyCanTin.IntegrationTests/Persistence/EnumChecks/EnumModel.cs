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

    public static List<EnumColumn> EnumColumnsOf(DbContext db) => EnumPropertiesOf(db)
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
}
