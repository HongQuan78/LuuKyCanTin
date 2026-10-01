using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;

public static class EnumCheckExtensions
{
    /// <summary>
    /// Adds <c>CHECK ([Col] IN (...))</c> built from the enum's values, so the constraint and the enum share one source.
    /// </summary>
    public static EntityTypeBuilder<TEntity> HasEnumCheck<TEntity, TEnum>(
        this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TEnum>> property)
        where TEntity : class
        where TEnum : struct, Enum
        => builder.HasEnumCheck<TEntity, TEnum>(builder.Property(property).Metadata.GetColumnName());

    public static EntityTypeBuilder<TEntity> HasEnumCheck<TEntity, TEnum>(
        this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TEnum?>> property)
        where TEntity : class
        where TEnum : struct, Enum
        => builder.HasEnumCheck<TEntity, TEnum>(builder.Property(property).Metadata.GetColumnName());

    private static EntityTypeBuilder<TEntity> HasEnumCheck<TEntity, TEnum>(this EntityTypeBuilder<TEntity> builder, string column)
        where TEntity : class
        where TEnum : struct, Enum
    {
        var values = string.Join(", ", Enum.GetValues<TEnum>()
            .Select(v => Convert.ToInt64(v, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)));
        var table = builder.Metadata.GetTableName();

        return builder.ToTable(t => t.HasCheckConstraint($"CK_{table}_{column}", $"[{column}] IN ({values})"));
    }
}
