using System.ComponentModel;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// One sidebar row: a flat button with an optional leading glyph, a muted trailing text (shortcut or chevron) and the
/// 3px indicator bar of the active item. Hover and active colours are handled here, since child labels would
/// otherwise steal the button's own mouse-over state.
/// </summary>
internal sealed class NavButton : Button
{
    private const int IndicatorWidth = 3;
    private const int TextInset = 16;

    private readonly Panel _indicator = new() { Dock = DockStyle.Left, Width = IndicatorWidth, Visible = false };
    private readonly Label _glyph = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, UseMnemonic = false };
    private readonly Label _trailing = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleRight, UseMnemonic = false };
    private bool _isActive;
    private bool _isHovered;

    public NavButton(string caption, string glyph, int height, int textIndent)
    {
        Text = caption;
        Height = height;
        Width = AppTheme.SidebarWidth;
        Margin = Padding.Empty;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        TextAlign = ContentAlignment.MiddleLeft;
        // A flat button already insets its text by about 16px, so the padding brings the text to textIndent.
        Padding = new Padding(Math.Max(0, textIndent - TextInset), 0, 40, 0);
        Font = AppTheme.BodyFont;
        Cursor = Cursors.Hand;
        UseVisualStyleBackColor = false;

        _indicator.BackColor = AppTheme.SideIndicator;
        _glyph.Font = AppTheme.IconFont(12F);
        _glyph.Text = glyph;
        _glyph.Visible = glyph.Length > 0;
        _glyph.Bounds = new Rectangle(16, 0, 16, height);
        _trailing.Font = AppTheme.SmallFont;
        _trailing.Bounds = new Rectangle(AppTheme.SidebarWidth - 70, 0, 56, height);

        Controls.Add(_indicator);
        Controls.Add(_glyph);
        Controls.Add(_trailing);
        foreach (Control child in Controls)
        {
            child.Click += (_, _) => PerformClick();
            child.MouseEnter += (_, _) => SetHovered(true);
            child.MouseLeave += (_, _) => SetHovered(ClientRectangle.Contains(PointToClient(MousePosition)));
        }

        ApplyColours();
    }

    /// <summary>Muted text at the right: a shortcut ("F2") or a group chevron glyph.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TrailingText
    {
        get => _trailing.Text;
        set => _trailing.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsTrailingGlyph
    {
        set => _trailing.Font = value ? AppTheme.IconFont(8F) : AppTheme.SmallFont;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            ApplyColours();
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        SetHovered(true);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        SetHovered(ClientRectangle.Contains(PointToClient(MousePosition)));
    }

    private void SetHovered(bool isHovered)
    {
        _isHovered = isHovered;
        ApplyColours();
    }

    private void ApplyColours()
    {
        var background = _isActive ? AppTheme.SideActive : _isHovered ? AppTheme.SideHover : AppTheme.Side;
        BackColor = background;
        FlatAppearance.MouseOverBackColor = background;
        FlatAppearance.MouseDownBackColor = AppTheme.SideActive;
        ForeColor = _isActive ? AppTheme.OnAccent : AppTheme.SideText;
        _glyph.BackColor = _trailing.BackColor = background;
        _glyph.ForeColor = ForeColor;
        _trailing.ForeColor = AppTheme.SideMuted;
        _indicator.Visible = _isActive;
    }
}
