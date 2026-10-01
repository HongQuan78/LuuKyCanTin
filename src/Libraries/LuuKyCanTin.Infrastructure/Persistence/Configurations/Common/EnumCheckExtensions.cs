using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;

public static class EnumCheckExtensions
{
    /// <summary>
    /// Adds <c>CHECK ([Col] IN (...))</c> built from the enum's values, so the constraint and the enum share one source.
    /// A column converted to text (<c>HasConversion&lt;string&gt;()</c>, configured first) is checked against the enum's names.
    /// </summary>
    public static EntityTypeBuilder<TEntity> HasEnumCheck<TEntity, TEnum>(
        this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TEnum>> property)
        where TEntity : class
        where TEnum : struct, Enum
        => builder.HasEnumCheck<TEntity, TEnum>(builder.Property(property).Metadata);

    public static EntityTypeBuilder<TEntity> HasEnumCheck<TEntity, TEnum>(
        this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TEnum?>> property)
        where TEntity : class
        where TEnum : struct, Enum
        => builder.HasEnumCheck<TEntity, TEnum>(builder.Property(property).Metadata);

    private static EntityTypeBuilder<TEntity> HasEnumCheck<TEntity, TEnum>(this EntityTypeBuilder<TEntity> builder, IMutableProperty property)
        where TEntity : class
        where TEnum : struct, Enum
    {
        var values = property.GetProviderClrType() == typeof(string)
            ? Enum.GetNames<TEnum>().Select(name => $"'{name}'")
            : Enum.GetValues<TEnum>().Select(v => Convert.ToInt64(v, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
        var column = property.GetColumnName();
        var table = builder.Metadata.GetTableName();

        return builder.ToTable(t => t.HasCheckConstraint($"CK_{table}_{column}", $"[{column}] IN ({string.Join(", ", values)})"));
    }
}
