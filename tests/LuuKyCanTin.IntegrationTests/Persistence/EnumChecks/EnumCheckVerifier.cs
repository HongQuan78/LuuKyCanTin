using System.Globalization;
using System.Text.RegularExpressions;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

public static class EnumCheckVerifier
{
    public static IReadOnlyList<string> KiemTra(IEnumerable<EnumColumn> columns, IReadOnlyList<DeployedCheck> checks)
    {
        var problems = new List<string>();

        foreach (var column in columns)
        {
            var location = $"{column.Schema}.{column.Table}.{column.Column}";
            var allowedSets = checks
                .Where(c => LaGiongNhau(c.Schema, column.Schema) && LaGiongNhau(c.Table, column.Table))
                .Select(c => LayGiaTriChoPhep(c.Definition, column.Column))
                .OfType<HashSet<string>>()
                .ToList();

            if (allowedSets.Count == 0)
            {
                problems.Add($"{location} ({column.EnumType.Name}): no CHECK constraint");
                continue;
            }

            // Every constraint on the column has to hold, so only values they all accept are allowed.
            var allowed = allowedSets.Aggregate((a, b) => [.. a.Intersect(b)]);
            // Stored value -> how to name it in a message.
            var enumValues = column.StoredAsName
                ? Enum.GetNames(column.EnumType).ToDictionary(n => Quote(n), n => n)
                : Enum.GetValues(column.EnumType).Cast<object>().ToDictionary(
                    v => Convert.ToInt64(v, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
                    v => $"{v}={Convert.ToInt64(v, CultureInfo.InvariantCulture)}");

            problems.AddRange(enumValues.Keys.Except(allowed).Order(StringComparer.Ordinal)
                .Select(v => $"{location}: enum value {enumValues[v]} is not allowed by the CHECK constraint"));
            problems.AddRange(allowed.Except(enumValues.Keys).Order(StringComparer.Ordinal)
                .Select(v => $"{location}: CHECK allows {v}, which {column.EnumType.Name} does not define"));
        }

        return problems;
    }

    /// <summary>
    /// Values a definition allows for one column, or null when it is not an enum list for that column. SQL Server stores
    /// <c>[Col] IN (1,2)</c> normalized as <c>([Col]=(1) OR [Col]=(2))</c> and <c>[Col] IN ('A')</c> as
    /// <c>([Col]='A')</c>; both forms are accepted. Numbers come back bare, text quoted (<c>'A'</c>).
    /// </summary>
    private static HashSet<string>? LayGiaTriChoPhep(string definition, string column)
    {
        var name = Regex.Escape($"[{column}]");
        if (!Regex.IsMatch(definition, name))
            return null;

        // Only a constraint that talks about this column alone defines its allowed set. A business check that
        // merely mentions the column (e.g. [TrangThai] = 1 OR [NgayRa] IS NOT NULL) is not an enum check.
        var otherColumn = Regex.Matches(definition, @"\[([^\]]+)\]")
            .Select(m => m.Groups[1].Value)
            .Any(c => !string.Equals(c, column, StringComparison.OrdinalIgnoreCase));
        if (otherColumn)
            return null;

        const string value = @"\(\s*-?\d+\s*\)|-?\d+|N?'[^']*'";
        var values = Regex.Matches(definition, name + @"\s*=\s*(" + value + ")")
            .Select(m => m.Groups[1].Value)
            .Concat(Regex.Matches(definition, name + @"\s+IN\s*\(((?:" + value + @"|[\s,])*)\)", RegexOptions.IgnoreCase)
                .SelectMany(m => Regex.Matches(m.Groups[1].Value, value).Select(v => v.Value)))
            .Select(Normalize);

        // A single-column check with no = or IN (e.g. [TrangThai] > 0) is not an enum list either; an empty set
        // here would wipe out the intersection and report every enum value as disallowed.
        HashSet<string> allowed = [.. values];
        return allowed.Count == 0 ? null : allowed;
    }

    private static string Normalize(string literal)
    {
        var text = Regex.Match(literal, "'([^']*)'");
        return text.Success
            ? Quote(text.Groups[1].Value)
            : long.Parse(literal.Trim(' ', '(', ')'), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
    }

    private static string Quote(string name) => $"'{name}'";

    private static bool LaGiongNhau(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
