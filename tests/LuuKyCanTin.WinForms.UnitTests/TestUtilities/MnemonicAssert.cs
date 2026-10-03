using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.TestUtilities;

/// <summary>
/// Fails when two buttons or labels of one form or UserControl share an <c>&amp;</c> mnemonic letter, because Alt+letter
/// then reaches only the first of them. Call it on an STA thread (<see cref="StaThread"/>).
/// </summary>
internal static class MnemonicAssert
{
    public static void HasUniqueMnemonics(Control root)
    {
        var duplicates = GetMnemonicControls(root)
            .GroupBy(v => v.Letter)
            .Where(g => g.Count() > 1)
            .Select(g => $"'{g.Key}': {string.Join(", ", g.Select(v => $"{v.Control.Name} \"{v.Control.Text}\""))}")
            .ToList();

        duplicates.ShouldBeEmpty($"{root.GetType().Name} has duplicate mnemonics");
    }

    /// <summary>Every button and label in the tree that has a mnemonic, with its upper-case letter.</summary>
    public static IEnumerable<(Control Control, char Letter)> GetMnemonicControls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            if (IsMnemonicOwner(child) && GetMnemonic(child.Text) is { } letter)
                yield return (child, letter);

            foreach (var nested in GetMnemonicControls(child))
                yield return nested;
        }
    }

    /// <summary>The letter after the first single <c>&amp;</c>; <c>&amp;&amp;</c> is a literal ampersand.</summary>
    public static char? GetMnemonic(string text)
    {
        for (var i = 0; i < text.Length - 1; i++)
        {
            if (text[i] != '&')
                continue;
            if (text[i + 1] == '&')
            {
                i++;
                continue;
            }

            return char.ToUpperInvariant(text[i + 1]);
        }

        return null;
    }

    private static bool IsMnemonicOwner(Control control) => control switch
    {
        ButtonBase button => button.UseMnemonic,
        Label label => label.UseMnemonic,
        _ => false,
    };
}
