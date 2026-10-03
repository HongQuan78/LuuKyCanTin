using System.Text.RegularExpressions;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Common;

/// <summary>Every colour and font comes from AppTheme, so no screen drifts from the DESIGN.md tokens.</summary>
public partial class ThemeUsageTests
{
    [Fact]
    public void WinFormsSource_OutsideAppTheme_UsesNoHardCodedColourOrFont()
    {
        var violations = Directory
            .EnumerateFiles(RepositoryPaths.WinFormsSource, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsExcluded(path))
            .SelectMany(path => File.ReadLines(path).Select((line, index) => (path, line, number: index + 1)))
            .Where(v => BannedPattern().IsMatch(v.line))
            .Select(v => $"{Path.GetRelativePath(RepositoryPaths.Root, v.path)}:{v.number}: {v.line.Trim()}")
            .ToList();

        violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("lbl.ForeColor = Color.Firebrick;")]
    [InlineData("BackColor = Color.FromArgb(37, 99, 235);")]
    [InlineData("ForeColor = SystemColors.GrayText;")]
    [InlineData("Font = new Font(\"Segoe UI\", 12F);")]
    public void BannedPattern_AHardCodedColourOrFont_Matches(string line)
    {
        BannedPattern().IsMatch(line).ShouldBeTrue();
    }

    [Theory]
    [InlineData("BackColor = Color.Transparent;")]
    [InlineData("if (color == Color.Empty)")]
    [InlineData("ForeColor = AppTheme.Danger;")]
    [InlineData("public Color BorderColor { get; set; }")]
    public void BannedPattern_ThemeOrAllowedColour_DoesNotMatch(string line)
    {
        BannedPattern().IsMatch(line).ShouldBeFalse();
    }

    private static bool IsExcluded(string path)
    {
        var relative = Path.GetRelativePath(RepositoryPaths.WinFormsSource, path);
        var firstSegment = relative.Split(Path.DirectorySeparatorChar)[0];
        return firstSegment is "bin" or "obj" || Path.GetFileName(path) == "AppTheme.cs";
    }

    [GeneratedRegex(@"Color\.FromArgb|\bColor\.(?!Transparent\b|Empty\b)[A-Z]\w*|\bSystemColors\.|\bnew\s+Font\s*\(")]
    private static partial Regex BannedPattern();
}
