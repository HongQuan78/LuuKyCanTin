using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The white bar above the content area: the page title and a muted subtitle. The right side stays free for quick search.</summary>
internal sealed class HeaderBar : UserControl
{
    private readonly Label _title = new()
    {
        AutoSize = true,
        Font = AppTheme.PageTitleFont,
        UseMnemonic = false,
    };

    private readonly Label _subtitle = new()
    {
        AutoSize = true,
        Font = AppTheme.SmallFont,
        UseMnemonic = false,
    };

    public HeaderBar()
    {
        BackColor = AppTheme.Card;
        Dock = DockStyle.Top;
        Height = AppTheme.HeaderHeight;
        Font = AppTheme.BodyFont;
        _title.ForeColor = AppTheme.Text;
        _subtitle.ForeColor = AppTheme.Muted;
        Controls.Add(_title);
        Controls.Add(_subtitle);
    }

    public void ShowTitle(string title, string subtitle = "")
    {
        _title.Text = title;
        _subtitle.Text = subtitle;
        PerformLayout();
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        _title.Location = new Point(AppTheme.PagePadding, (Height - _title.Height) / 2);
        _subtitle.Location = new Point(_title.Right + AppTheme.Gap, (Height - _subtitle.Height) / 2 + 1);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var line = new SolidBrush(AppTheme.Border);
        e.Graphics.FillRectangle(line, 0, Height - 1, Width, 1);
    }
}
