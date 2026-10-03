using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Common;

/// <summary>AppTheme mirrors the DESIGN.md front matter one to one; a token added there must be added here.</summary>
public partial class AppThemeTests
{
    private static readonly string DesignPath = Path.Combine(
        RepositoryPaths.Root, "_bmad-output", "planning-artifacts", "ux-designs", "ux-TienGuiLuuKy-2026-10-02", "DESIGN.md");

    public static TheoryData<string, string> ColourTokens()
    {
        var data = new TheoryData<string, string>();
        foreach (var (name, value) in ReadSection("colors"))
            data.Add(name, ColourValue().Match(value).Groups[1].Value);
        return data;
    }

    public static TheoryData<string, float, string> TypographyTokens()
    {
        var data = new TheoryData<string, float, string>();
        foreach (var (name, value) in ReadSection("typography").Where(t => t.Name != "icon"))
        {
            var size = float.Parse(FontSize().Match(value).Groups[1].Value, CultureInfo.InvariantCulture);
            data.Add(name, size, FontWeight().Match(value).Groups[1].Value);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ColourTokens))]
    public void Colour_EveryDesignToken_HasTheSameValue(string token, string hex)
    {
        // High contrast replaces the brand colours with the user's system colours.
        if (SystemInformation.HighContrast)
            return;

        var colour = (Color)GetThemeProperty(ToPascalCase(token)).GetValue(null)!;

        colour.ToArgb().ShouldBe(int.Parse(hex, NumberStyles.HexNumber) | unchecked((int)0xFF000000), token);
    }

    [Theory]
    [MemberData(nameof(TypographyTokens))]
    public void Font_EveryDesignToken_HasTheSameSizeAndWeight(string token, float size, string weight)
    {
        var font = (Font)GetThemeProperty(ToPascalCase(token) + "Font").GetValue(null)!;

        font.SizeInPoints.ShouldBe(size, token);
        switch (weight)
        {
            case "400":
                font.Bold.ShouldBeFalse(token);
                font.Name.ShouldBe("Segoe UI", token);
                break;
            case "600":
                font.Name.ShouldBe("Segoe UI Semibold", token);
                break;
            case "700":
                font.Bold.ShouldBeTrue(token);
                break;
            default:
                throw new InvalidOperationException($"Unexpected weight {weight} for {token}.");
        }
    }

    [Fact]
    public void Font_SameToken_IsTheSameSharedInstance()
    {
        AppTheme.BodyFont.ShouldBeSameAs(AppTheme.BodyFont);
        AppTheme.IconFont(12F).ShouldBeSameAs(AppTheme.IconFont(12F));
    }

    [Fact]
    public void StylePrimary_DisablingTheButton_SwitchesToTheDisabledColours()
    {
        // High contrast replaces the brand colours with the user's system colours.
        if (SystemInformation.HighContrast)
            return;

        StaThread.Run(() =>
        {
            using var button = new Button();
            AppTheme.StylePrimary(button);
            button.BackColor.ShouldBe(AppTheme.Accent);

            button.Enabled = false;
            button.BackColor.ShouldBe(AppTheme.Border);
            button.ForeColor.ShouldBe(AppTheme.Placeholder);

            button.Enabled = true;
            button.BackColor.ShouldBe(AppTheme.Accent);
        });
    }

    private static PropertyInfo GetThemeProperty(string name) =>
        typeof(AppTheme).GetProperty(name, BindingFlags.Public | BindingFlags.Static)
        ?? throw new ShouldAssertException($"AppTheme has no {name} for the DESIGN.md token.");

    private static string ToPascalCase(string token) =>
        string.Concat(token.Split('-').Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    /// <summary>The <c>name: value</c> lines of one top-level section of the DESIGN.md front matter.</summary>
    private static IEnumerable<(string Name, string Value)> ReadSection(string section)
    {
        var isInSection = false;
        foreach (var line in File.ReadLines(DesignPath))
        {
            if (!line.StartsWith(' '))
            {
                isInSection = line == section + ":";
                continue;
            }

            if (isInSection && TokenLine().Match(line) is { Success: true } match)
                yield return (match.Groups[1].Value, match.Groups[2].Value);
        }
    }

    [GeneratedRegex(@"^  ([a-z0-9-]+):\s*(.+)$")]
    private static partial Regex TokenLine();

    [GeneratedRegex(@"'#([0-9A-Fa-f]{6})'")]
    private static partial Regex ColourValue();

    [GeneratedRegex(@"fontSize:\s*([0-9.]+)pt")]
    private static partial Regex FontSize();

    [GeneratedRegex(@"fontWeight:\s*'(\d+)'")]
    private static partial Regex FontWeight();
}
