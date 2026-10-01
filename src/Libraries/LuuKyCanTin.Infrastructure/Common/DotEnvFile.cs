namespace LuuKyCanTin.Infrastructure.Common;

/// <summary>
/// Loads machine-specific secrets such as the connection string from a <c>.env</c> file kept out of source control.
/// Keys use the environment-variable form (<c>ConnectionStrings__LuuKyCanTin</c>), so the host's
/// environment-variable provider picks them up.
/// </summary>
public static class DotEnvFile
{
    public const string FileName = ".env";

    /// <summary>
    /// Sets each variable that is not already set. A real environment variable wins, so an admin can
    /// override the file on one machine without editing it.
    /// </summary>
    public static void Load(string path)
    {
        if (!File.Exists(path))
            return;

        foreach (var (key, value) in Parse(File.ReadAllLines(path)))
        {
            if (Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    /// <remarks>Values are literal: no escapes and no variable expansion, so <c>.\SQLEXPRESS</c> needs no doubling.</remarks>
    public static IReadOnlyList<KeyValuePair<string, string>> Parse(IEnumerable<string> lines)
    {
        var pairs = new List<KeyValuePair<string, string>>();
        var lineNumber = 0;

        foreach (var rawLine in lines)
        {
            lineNumber++;
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = line.IndexOf('=');
            if (separator <= 0)
                throw new FormatException($"{FileName}: line {lineNumber} is not in the form KEY=VALUE.");

            pairs.Add(new(line[..separator].Trim(), Unquote(line[(separator + 1)..].Trim())));
        }

        return pairs;
    }

    private static string Unquote(string value) =>
        value.Length >= 2 && (value[0] is '"' or '\'') && value[^1] == value[0] ? value[1..^1] : value;
}
