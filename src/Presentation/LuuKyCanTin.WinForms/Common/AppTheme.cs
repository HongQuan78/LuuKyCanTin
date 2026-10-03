namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// The DESIGN.md tokens, one to one: every colour, font and size a screen uses comes from here. When Windows high
/// contrast is on, the colours fall back to <see cref="SystemColors"/> so the user's contrast theme wins.
/// </summary>
internal static class AppTheme
{
    private const string FontFamilyName = "Segoe UI";
    private const string SemiboldFontFamilyName = "Segoe UI Semibold";

    // ---- Colours ---------------------------------------------------------------------------------------------

    public static Color Accent => Pick(0x2563EB, SystemColors.Highlight);

    public static Color AccentHover => Pick(0x1D4ED8, SystemColors.Highlight);

    public static Color AccentSoft => Pick(0xDBEAFE, SystemColors.Window);

    /// <summary>Text and glyphs drawn on <see cref="Accent"/> or <see cref="SideActive"/> (the DESIGN.md "#FFFFFF").</summary>
    public static Color OnAccent => Pick(0xFFFFFF, SystemColors.HighlightText);

    public static Color Side => Pick(0x1E293B, SystemColors.Window);

    public static Color SideHover => Pick(0x273449, SystemColors.Window);

    public static Color SideActive => Pick(0x334155, SystemColors.Highlight);

    public static Color SideIndicator => Pick(0x60A5FA, SystemColors.HighlightText);

    public static Color SideText => Pick(0xCBD5E1, SystemColors.WindowText);

    public static Color SideMuted => Pick(0x7C8BA1, SystemColors.GrayText);

    public static Color Surface => Pick(0xF1F5F9, SystemColors.Window);

    public static Color Card => Pick(0xFFFFFF, SystemColors.Window);

    public static Color Subtle => Pick(0xF8FAFC, SystemColors.Window);

    public static Color Border => Pick(0xE2E8F0, SystemColors.WindowFrame);

    public static Color RowLine => Pick(0xEEF2F6, SystemColors.WindowFrame);

    public static Color InputBorder => Pick(0xCBD5E1, SystemColors.WindowFrame);

    public static Color Text => Pick(0x0F172A, SystemColors.WindowText);

    public static Color Text2 => Pick(0x334155, SystemColors.WindowText);

    public static Color Label => Pick(0x475569, SystemColors.WindowText);

    public static Color Muted => Pick(0x64748B, SystemColors.GrayText);

    public static Color Placeholder => Pick(0x94A3B8, SystemColors.GrayText);

    public static Color Success => Pick(0x15803D, SystemColors.WindowText);

    public static Color SuccessSoft => Pick(0xDCFCE7, SystemColors.Window);

    public static Color Danger => Pick(0xB91C1C, SystemColors.WindowText);

    public static Color DangerSoft => Pick(0xFEE2E2, SystemColors.Window);

    public static Color Warning => Pick(0xB45309, SystemColors.WindowText);

    public static Color WarningSoft => Pick(0xFEF3C7, SystemColors.Window);

    // ---- Typography: created once and shared, so never dispose one -----------------------------------------------

    public static Font BodyFont { get; } = new(FontFamilyName, 10F);

    /// <summary>Body size at weight 600: primary buttons, names in grids and the user block.</summary>
    public static Font BodySemiboldFont { get; } = new(SemiboldFontFamilyName, 10F);

    /// <summary>Italic body text, for an amount spelled out in words.</summary>
    public static Font AmountInWordsFont { get; } = new(FontFamilyName, 10F, FontStyle.Italic);

    public static Font LabelFont { get; } = new(SemiboldFontFamilyName, 9F);

    public static Font SmallFont { get; } = new(FontFamilyName, 9F);

    public static Font CardTitleFont { get; } = new(SemiboldFontFamilyName, 11F);

    public static Font DialogTitleFont { get; } = new(SemiboldFontFamilyName, 13F);

    public static Font PageTitleFont { get; } = new(SemiboldFontFamilyName, 13.5F);

    public static Font GreetingFont { get; } = new(SemiboldFontFamilyName, 15F);

    public static Font AmountMediumFont { get; } = new(SemiboldFontFamilyName, 14F);

    public static Font AmountLargeFont { get; } = new(FontFamilyName, 24F, FontStyle.Bold);

    private static readonly Dictionary<float, Font> IconFonts = [];

    private static readonly string IconFontFamilyName =
        FontFamily.Families.Any(f => f.Name == "Segoe Fluent Icons") ? "Segoe Fluent Icons" : "Segoe MDL2 Assets";

    /// <summary>The Windows icon font: 12pt in navigation, 10.5pt in buttons, 18pt in tiles.</summary>
    public static Font IconFont(float size)
    {
        if (!IconFonts.TryGetValue(size, out var font))
        {
            font = new Font(IconFontFamilyName, size);
            IconFonts[size] = font;
        }

        return font;
    }

    // ---- Spacing (px at 96 DPI) ----------------------------------------------------------------------------------

    public const int PagePadding = 20;
    public const int Gap = 16;
    public const int GapSmall = 8;
    public const int CardPadding = 16;
    public const int CardHeaderHeight = 44;
    public const int DialogPadding = 20;
    public const int SidebarWidth = 220;
    public const int HeaderHeight = 56;
    public const int StatusHeight = 26;
    public const int InputHeight = 32;
    public const int InputHeightLarge = 40;
    public const int ButtonHeight = 32;
    public const int ButtonHeightLarge = 44;
    public const int ButtonPaddingHorizontal = 14;
    public const int SegmentHeight = 28;
    public const int GridHeaderHeight = 38;
    public const int GridRowHeight = 38;
    public const int NavItemHeight = 36;
    public const int NavSubHeight = 32;
    public const int NavSubIndent = 47;
    public const int LabelGap = 5;
    public const int TileHeight = 124;

    // ---- Shared styles -------------------------------------------------------------------------------------------

    /// <summary>The one accent button of a screen or dialog.</summary>
    public static void StylePrimary(Button button)
    {
        StyleFlat(button, BodySemiboldFont);
        ApplyButtonColours(button, isPrimary: true);
        button.EnabledChanged += (_, _) => ApplyButtonColours(button, isPrimary: true);
    }

    public static void StyleSecondary(Button button)
    {
        StyleFlat(button, BodyFont);
        ApplyButtonColours(button, isPrimary: false);
        button.EnabledChanged += (_, _) => ApplyButtonColours(button, isPrimary: false);
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BorderStyle = BorderStyle.None;
        grid.BackgroundColor = Card;
        grid.GridColor = RowLine;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = GridHeaderHeight;
        grid.RowTemplate.Height = GridRowHeight;

        grid.ColumnHeadersDefaultCellStyle.BackColor = Subtle;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Label;
        grid.ColumnHeadersDefaultCellStyle.Font = LabelFont;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Subtle;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Label;

        grid.DefaultCellStyle.BackColor = Card;
        grid.DefaultCellStyle.ForeColor = Text;
        grid.DefaultCellStyle.Font = BodyFont;
        grid.DefaultCellStyle.SelectionBackColor = AccentSoft;
        grid.DefaultCellStyle.SelectionForeColor = SystemInformation.HighContrast ? SystemColors.WindowText : Text;
    }

    /// <summary>One segment of a segmented control: a button-looking radio, accent when checked.</summary>
    public static void StyleSegment(RadioButton segment)
    {
        segment.Appearance = Appearance.Button;
        segment.FlatStyle = FlatStyle.Flat;
        segment.TextAlign = ContentAlignment.MiddleCenter;
        segment.Height = SegmentHeight;
        segment.Font = BodyFont;
        segment.FlatAppearance.BorderColor = InputBorder;
        segment.FlatAppearance.CheckedBackColor = Accent;
        ApplySegmentColours(segment);
        segment.CheckedChanged += (_, _) => ApplySegmentColours(segment);
    }

    private static void StyleFlat(Button button, Font font)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.Font = font;
        button.Height = Math.Max(button.Height, ButtonHeight);
        button.Padding = new Padding(ButtonPaddingHorizontal, 0, ButtonPaddingHorizontal, 0);
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
    }

    private static void ApplyButtonColours(Button button, bool isPrimary)
    {
        if (!button.Enabled)
        {
            button.BackColor = Border;
            button.ForeColor = Placeholder;
            button.FlatAppearance.BorderColor = Border;
            return;
        }

        button.BackColor = isPrimary ? Accent : Card;
        button.ForeColor = isPrimary ? OnAccent : Text;
        button.FlatAppearance.BorderColor = isPrimary ? Accent : InputBorder;
        button.FlatAppearance.MouseOverBackColor = isPrimary ? AccentHover : Subtle;
        button.FlatAppearance.MouseDownBackColor = isPrimary ? AccentHover : Border;
    }

    private static void ApplySegmentColours(RadioButton segment)
    {
        segment.BackColor = segment.Checked ? Accent : Card;
        segment.ForeColor = segment.Checked ? OnAccent : Text;
    }

    private static Color Pick(int rgb, Color highContrast) =>
        SystemInformation.HighContrast ? highContrast : Color.FromArgb(unchecked((int)0xFF000000) | rgb);
}
