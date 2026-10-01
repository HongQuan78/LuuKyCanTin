using System.Xml.Linq;

namespace LuuKyCanTin.IntegrationTests.Architecture;

/// <summary>
/// The allowed dependency graph between the source projects, checked against raw .csproj XML.
/// </summary>
internal static class ProjectReferenceRules
{
    public const string Domain = "LuuKyCanTin.Domain";
    public const string Application = "LuuKyCanTin.Application";
    public const string Infrastructure = "LuuKyCanTin.Infrastructure";
    public const string WinForms = "LuuKyCanTin.WinForms";

    // Kept as one list so that admitting Microsoft.EntityFrameworkCore (core, no provider) is a one-line change.
    public static readonly IReadOnlySet<string> ApplicationAllowedPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "FluentValidation",
        "FluentValidation.DependencyInjectionExtensions",
        "Microsoft.Extensions.DependencyInjection.Abstractions",
    };

    public static readonly IReadOnlyList<string> ForbiddenInInnerLayers =
    [
        "Microsoft.EntityFrameworkCore.SqlServer",
        "QuestPDF",
        "ClosedXML",
        "System.Windows.Forms",
    ];

    private static readonly Dictionary<string, string[]> AllowedProjectReferences = new()
    {
        [Domain] = [],
        [Application] = [Domain],
        [Infrastructure] = [Application, Domain],
        [WinForms] = [Application, Infrastructure],
    };

    public static IReadOnlyCollection<string> KnownProjects => AllowedProjectReferences.Keys;

    public static IReadOnlyList<string> Check(string projectName, string csprojXml)
    {
        if (!AllowedProjectReferences.TryGetValue(projectName, out var allowedProjects))
            return [$"{projectName}: no reference rule defined"];

        var project = XDocument.Parse(csprojXml);
        var violations = new List<string>();

        var projectReferences = Elements(project, "ProjectReference")
            .Select(e => Path.GetFileNameWithoutExtension(((string?)e.Attribute("Include") ?? "").Replace('\\', '/')))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var extra in projectReferences.Except(allowedProjects, StringComparer.OrdinalIgnoreCase))
            violations.Add($"{projectName} must not reference project {extra}");
        foreach (var missing in allowedProjects.Except(projectReferences, StringComparer.OrdinalIgnoreCase))
            violations.Add($"{projectName} must reference project {missing}");

        var packages = Elements(project, "PackageReference")
            .Where(e => !IsAnalyzerOnly(e))
            .Select(e => (string?)e.Attribute("Include") ?? "")
            .ToList();

        switch (projectName)
        {
            case Domain:
                violations.AddRange(packages.Select(p => $"{projectName} must not reference package {p}"));
                break;
            case Application:
                violations.AddRange(packages
                    .Where(p => !ApplicationAllowedPackages.Contains(p))
                    .Select(p => $"{projectName} must not reference package {p}"));
                break;
        }

        if (projectName is Domain or Application)
        {
            if (Elements(project, "UseWindowsForms").Any(e => e.Value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)))
                violations.Add($"{projectName} must not use Windows Forms");
            violations.AddRange(Elements(project, "FrameworkReference")
                .Select(e => $"{projectName} must not reference framework {(string?)e.Attribute("Include")}"));
        }

        return violations;
    }

    private static IEnumerable<XElement> Elements(XDocument project, string localName) =>
        project.Descendants().Where(e => e.Name.LocalName == localName);

    private static bool IsAnalyzerOnly(XElement packageReference)
    {
        var privateAssets = (string?)packageReference.Attribute("PrivateAssets")
            ?? packageReference.Elements().FirstOrDefault(e => e.Name.LocalName == "PrivateAssets")?.Value;
        return string.Equals(privateAssets?.Trim(), "all", StringComparison.OrdinalIgnoreCase);
    }
}
