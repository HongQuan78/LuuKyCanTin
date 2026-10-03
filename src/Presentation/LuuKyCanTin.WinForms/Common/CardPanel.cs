using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// The DESIGN.md card: a white panel with a 1px border, an optional 44px header (card title, bottom border) and a
/// body padded by 16.
/// </summary>
internal class CardPanel : Panel
{
    private string _headerText = "";
    private bool _isHighlighted;

    public CardPanel()
    {
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        BackColor = AppTheme.Card;
        ApplyPadding();
    }

    /// <summary>The card title; empty means no header band.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string HeaderText
    {
        get => _headerText;
        set
        {
            _headerText = value;
            ApplyPadding();
            Invalidate();
        }
    }

    /// <summary>Draws a 2px accent border, as a quick-action tile does under the mouse.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsHighlighted
    {
        get => _isHighlighted;
        set
        {
            _isHighlighted = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_headerText.Length > 0)
        {
            var header = new Rectangle(AppTheme.CardPadding, 0, Width - 2 * AppTheme.CardPadding, AppTheme.CardHeaderHeight);
            TextRenderer.DrawText(e.Graphics, _headerText, AppTheme.CardTitleFont, header, AppTheme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            using var line = new SolidBrush(AppTheme.Border);
            e.Graphics.FillRectangle(line, 0, AppTheme.CardHeaderHeight - 1, Width, 1);
        }

        var (colour, width) = _isHighlighted ? (AppTheme.Accent, 2) : (AppTheme.Border, 1);
        ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
            colour, width, ButtonBorderStyle.Solid, colour, width, ButtonBorderStyle.Solid,
            colour, width, ButtonBorderStyle.Solid, colour, width, ButtonBorderStyle.Solid);
    }

    private void ApplyPadding()
    {
        var top = _headerText.Length > 0 ? AppTheme.CardHeaderHeight + AppTheme.CardPadding : AppTheme.CardPadding;
        Padding = new Padding(AppTheme.CardPadding, top, AppTheme.CardPadding, AppTheme.CardPadding);
    }
}
