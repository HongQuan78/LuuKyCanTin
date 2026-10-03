using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

/// <summary>Finds the enum-typed columns of an EF model.</summary>
public static class EnumModel
{
    public const string DefaultSchema = "dbo";

    public static IEnumerable<IProperty> GetEnumProperties(DbContext db) => db.GetService<IDesignTimeModel>().Model
        .GetEntityTypes()
        .SelectMany(e => e.GetProperties())
        .Where(p => (Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType).IsEnum);

    /// <summary>True for an enum converted to text (its name or a code) rather than stored as a number.</summary>
    public static bool IsStoredAsText(IProperty property) => property.GetTypeMapping().Converter?.ProviderClrType == typeof(string);

    public static List<EnumColumn> GetEnumColumns(DbContext db) => GetEnumProperties(db)
        .Select(p =>
        {
            var entity = (IEntityType)p.DeclaringType;
            var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            var converter = p.GetTypeMapping().Converter;
            return new EnumColumn(
                entity.GetSchema() ?? DefaultSchema,
                table.Name,
                p.GetColumnName(table)!,
                Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType,
                IsStoredAsText(p))
            {
                ToStoredText = IsStoredAsText(p) && converter is not null
                    ? value => (string)converter.ConvertToProvider(value)!
                    : null,
            };
        })
        .ToList();
}
