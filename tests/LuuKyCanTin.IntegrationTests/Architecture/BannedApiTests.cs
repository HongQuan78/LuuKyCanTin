using System.Xml.Linq;
using LuuKyCanTin.IntegrationTests.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Architecture;

// The analyzer is what fails the build; these guard its configuration from being dropped or narrowed.
public class BannedApiTests
{
    private static readonly string SourceRoot = Path.Combine(RepositoryPaths.Root, "src");

    [Theory]
    [InlineData("P:System.DateTime.Now")]
    [InlineData("P:System.DateTime.Today")]
    [InlineData("P:System.DateTime.UtcNow")]
    [InlineData("P:System.DateTimeOffset.Now")]
    [InlineData("P:System.DateTimeOffset.UtcNow")]
    [InlineData("P:System.TimeProvider.System")]
    public void BannedSymbols_SystemClockRead_IsBanned(string symbol)
    {
        File.ReadAllLines(Path.Combine(SourceRoot, "BannedSymbols.txt"))
            .Select(line => line.Split(';')[0].Trim())
            .ShouldContain(symbol);
    }

    [Fact]
    public void DirectoryBuildProps_EverySourceProject_GetsTheBannedApiAnalyzer()
    {
        var props = XDocument.Load(Path.Combine(SourceRoot, "Directory.Build.props"));

        props.Descendants("PackageReference").Select(e => (string?)e.Attribute("Include"))
            .ShouldContain("Microsoft.CodeAnalysis.BannedApiAnalyzers");
        props.Descendants("AdditionalFiles").Select(e => (string?)e.Attribute("Include") ?? "")
            .ShouldContain(path => path.EndsWith("BannedSymbols.txt", StringComparison.Ordinal));
    }
}
