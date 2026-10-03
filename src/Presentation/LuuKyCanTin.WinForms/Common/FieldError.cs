using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>The red message under an invalid field (error glyph plus danger 9pt text); hidden while empty. Replaces ErrorProvider.</summary>
internal sealed class FieldError : FlowLayoutPanel
{
    private readonly Label _glyph = new()
    {
        AutoSize = true,
        Text = Glyphs.Error,
        Font = AppTheme.IconFont(9F),
        ForeColor = AppTheme.Danger,
        Margin = new Padding(0, 2, 6, 0),
        UseMnemonic = false,
    };

    private readonly Label _message = new()
    {
        AutoSize = true,
        Font = AppTheme.SmallFont,
        ForeColor = AppTheme.Danger,
        Margin = Padding.Empty,
        UseMnemonic = false,
    };

    public FieldError()
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        WrapContents = false;
        Margin = new Padding(0, 4, 0, 0);
        Padding = Padding.Empty;
        Visible = false;
        Controls.Add(_glyph);
        Controls.Add(_message);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Message
    {
        get => _message.Text;
        set
        {
            _message.Text = value;
            Visible = value.Length > 0;
        }
    }

    /// <summary>Wraps long messages at this width, so a field error never widens its column.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int MessageWidth
    {
        set => _message.MaximumSize = new Size(value - _glyph.PreferredWidth - 6, 0);
    }
}
