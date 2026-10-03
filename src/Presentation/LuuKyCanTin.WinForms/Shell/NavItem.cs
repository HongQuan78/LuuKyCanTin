namespace LuuKyCanTin.WinForms.Shell;

/// <summary>One sidebar entry: what it shows and what it opens. Plain data, so a permission-driven registry can supply it.</summary>
/// <param name="Key">Stable id, also the content-host key of a screen shown in the content area.</param>
/// <param name="Caption">Sidebar text, with its <c>&amp;</c> mnemonic.</param>
public sealed record NavItem(string Key, string Caption, Action Open)
{
    /// <summary>The permission the entry needs, or null when every signed-in user may see it.</summary>
    public string? PermissionCode { get; init; }

    /// <summary>Icon-font glyph; top-level entries have one, sub-items don't.</summary>
    public string Glyph { get; init; } = "";

    /// <summary>The global shortcut that opens the entry from anywhere in the shell, or <see cref="Keys.None"/>.</summary>
    public Keys Shortcut { get; init; } = Keys.None;

    /// <summary>How the shortcut is shown in the sidebar: "F2", "Ctrl+L". Never the OS-localized keys converter.</summary>
    public string ShortcutText => Shortcut == Keys.None ? "" : FormatShortcut(Shortcut);

    /// <summary>Set when the entry also appears as a quick-action tile on Trang chủ.</summary>
    public NavTile? Tile { get; init; }

    private static string FormatShortcut(Keys keys)
    {
        var text = "";
        if ((keys & Keys.Control) != Keys.None)
            text += "Ctrl+";
        if ((keys & Keys.Shift) != Keys.None)
            text += "Shift+";
        if ((keys & Keys.Alt) != Keys.None)
            text += "Alt+";
        return text + (keys & Keys.KeyCode);
    }
}
