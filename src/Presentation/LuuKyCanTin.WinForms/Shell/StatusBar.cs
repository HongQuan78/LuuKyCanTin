using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// The 26px strip under the content area: CSDL and Máy items, each with an 8px square dot, separated by 1px borders,
/// and the app version at the far right. Only items whose data exists are shown.
/// </summary>
internal sealed class StatusBar : UserControl
{
    private const int DotSize = 8;

    private readonly FlowLayoutPanel _items = new()
    {
        Dock = DockStyle.Fill,
        WrapContents = false,
        Padding = new Padding(0, 1, 0, 0),
        Margin = Padding.Empty,
    };

    private readonly Label _version = new()
    {
        Dock = DockStyle.Right,
        AutoSize = true,
        Padding = new Padding(12, 5, 12, 0),
        UseMnemonic = false,
    };

    public StatusBar()
    {
        BackColor = AppTheme.Card;
        Dock = DockStyle.Bottom;
        Height = AppTheme.StatusHeight;
        Font = AppTheme.SmallFont;
        _version.ForeColor = AppTheme.Text2;
        Controls.Add(_items);
        Controls.Add(_version);
    }

    public void ShowWorkstation(WorkstationInfo workstation)
    {
        _items.SuspendLayout();
        _items.Controls.Clear();
        _items.Controls.Add(CreateItem($"CSDL: {workstation.DatabaseText}", workstation.IsDatabaseConnected));
        _items.Controls.Add(CreateItem($"Máy: {workstation.WorkstationName}", isHealthy: true));
        _items.ResumeLayout();
        _version.Text = workstation.Version;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var line = new SolidBrush(AppTheme.Border);
        e.Graphics.FillRectangle(line, 0, 0, Width, 1);
    }

    private static Control CreateItem(string text, bool isHealthy)
    {
        var item = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(12, 0, 12, 0),
            Height = AppTheme.StatusHeight - 1,
        };
        item.Paint += (_, e) =>
        {
            using var line = new SolidBrush(AppTheme.Border);
            e.Graphics.FillRectangle(line, item.Width - 1, 0, 1, item.Height);
        };

        var dot = new Panel
        {
            Size = new Size(DotSize, DotSize),
            Margin = new Padding(0, 8, 6, 0),
            BackColor = isHealthy ? AppTheme.Success : AppTheme.Danger,
        };
        var label = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 0),
            ForeColor = isHealthy ? AppTheme.Text2 : AppTheme.Danger,
            Font = isHealthy ? AppTheme.SmallFont : AppTheme.LabelFont,
            Text = text,
            UseMnemonic = false,
        };
        item.Controls.Add(dot);
        item.Controls.Add(label);
        return item;
    }
}
