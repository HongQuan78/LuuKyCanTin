using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// The DESIGN.md banner for form-level errors and system warnings: a soft background, a 3px left bar, a glyph and a
/// wrapping message. Hidden while the message is empty; its height follows the message.
/// </summary>
internal sealed class Banner : Panel
{
    private const int BarWidth = 3;
    private const int PaddingVertical = 10;
    private const int TextLeft = 37;

    private readonly Panel _bar = new() { Dock = DockStyle.Left, Width = BarWidth };

    private readonly Label _glyph = new()
    {
        AutoSize = true,
        Location = new Point(BarWidth + 9, PaddingVertical + 1),
        Font = AppTheme.IconFont(11F),
        UseMnemonic = false,
    };

    private readonly Label _message = new()
    {
        AutoSize = true,
        Location = new Point(TextLeft, PaddingVertical),
        Font = AppTheme.BodyFont,
        UseMnemonic = false,
    };

    private BannerKind _kind = BannerKind.Error;

    public Banner()
    {
        Visible = false;
        Controls.Add(_message);
        Controls.Add(_glyph);
        Controls.Add(_bar);
        ApplyKind();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BannerKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            ApplyKind();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Message
    {
        get => _message.Text;
        set
        {
            _message.Text = value;
            Visible = value.Length > 0;
            FitHeight();
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        FitHeight();
    }

    private void ApplyKind()
    {
        var (background, accent, glyph) = _kind == BannerKind.Warning
            ? (AppTheme.WarningSoft, AppTheme.Warning, Glyphs.Warning)
            : (AppTheme.DangerSoft, AppTheme.Danger, Glyphs.Error);
        BackColor = background;
        _bar.BackColor = accent;
        _glyph.ForeColor = accent;
        _glyph.Text = glyph;
        _message.ForeColor = AppTheme.Text;
    }

    private void FitHeight()
    {
        var textWidth = Math.Max(1, Width - TextLeft - 12);
        if (_message.MaximumSize.Width != textWidth)
            _message.MaximumSize = new Size(textWidth, 0);

        var textHeight = _message.GetPreferredSize(new Size(textWidth, 0)).Height;
        var height = Math.Max(textHeight, _glyph.PreferredHeight) + 2 * PaddingVertical;
        if (Height != height)
            Height = height;
    }
}
