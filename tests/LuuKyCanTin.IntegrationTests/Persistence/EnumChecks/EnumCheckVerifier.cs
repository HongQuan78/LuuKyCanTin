using System.Globalization;
using System.Text.RegularExpressions;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

/// <summary>An enum-typed column of the EF model.</summary>
public sealed record EnumColumn(string Schema, string Table, string Column, Type EnumType);

/// <summary>A row of <c>sys.check_constraints</c>, as deployed.</summary>
public sealed record DeployedCheck(string Schema, string Table, string Definition);

public static class EnumCheckVerifier
{
    public static IReadOnlyList<string> FindProblems(IEnumerable<EnumColumn> columns, IReadOnlyList<DeployedCheck> checks)
    {
        var problems = new List<string>();

        foreach (var column in columns)
        {
            var location = $"{column.Schema}.{column.Table}.{column.Column}";
            var allowedSets = checks
                .Where(c => Same(c.Schema, column.Schema) && Same(c.Table, column.Table))
                .Select(c => ParseAllowedValues(c.Definition, column.Column))
                .OfType<HashSet<long>>()
                .ToList();

            if (allowedSets.Count == 0)
            {
                problems.Add($"{location} ({column.EnumType.Name}): no CHECK constraint");
                continue;
            }

            // Every constraint on the column has to hold, so only values they all accept are allowed.
            var allowed = allowedSets.Aggregate((a, b) => [.. a.Intersect(b)]);
            var enumValues = Enum.GetValues(column.EnumType).Cast<object>()
                .ToDictionary(v => Convert.ToInt64(v, CultureInfo.InvariantCulture), v => v.ToString()!);

            problems.AddRange(enumValues.Keys.Except(allowed).Order()
                .Select(v => $"{location}: enum value {enumValues[v]}={v} is not allowed by the CHECK constraint"));
            problems.AddRange(allowed.Except(enumValues.Keys).Order()
                .Select(v => $"{location}: CHECK allows {v}, which {column.EnumType.Name} does not define"));
        }

        return problems;
    }

    /// <summary>
    /// Values a definition allows for one column, or null when it does not mention the column. SQL Server stores
    /// <c>[Col] IN (1,2)</c> normalized as <c>([Col]=(1) OR [Col]=(2))</c>; both forms are accepted.
    /// </summary>
    private static HashSet<long>? ParseAllowedValues(string definition, string column)
    {
        var name = Regex.Escape($"[{column}]");
        if (!Regex.IsMatch(definition, name))
            return null;

        var values = Regex.Matches(definition, name + @"\s*=\s*\(\s*(-?\d+)\s*\)")
            .Select(m => m.Groups[1].Value)
            .Concat(Regex.Matches(definition, name + @"\s+IN\s*\(([^)]*)\)", RegexOptions.IgnoreCase)
                .SelectMany(m => m.Groups[1].Value.Split(',')))
            .Select(v => long.Parse(v.Trim(' ', '(', ')'), CultureInfo.InvariantCulture));

        return [.. values];
    }

    private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
