using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Trang chủ: the greeting, the date line and one quick-action tile per existing screen, four tiles to a row.</summary>
internal sealed class HomePage : UserControl
{
    private const int TileColumns = 4;

    private readonly Label _greeting = new()
    {
        AutoSize = true,
        Font = AppTheme.GreetingFont,
        Location = Point.Empty,
        UseMnemonic = false,
    };

    private readonly Label _dateLine = new()
    {
        AutoSize = true,
        Location = new Point(0, 30),
        UseMnemonic = false,
    };

    private readonly TableLayoutPanel _tiles = new()
    {
        ColumnCount = TileColumns,
        RowCount = 1,
        Location = new Point(0, 72),
        Height = AppTheme.TileHeight,
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        Margin = Padding.Empty,
    };

    public HomePage()
    {
        BackColor = AppTheme.Surface;
        Font = AppTheme.BodyFont;
        _greeting.ForeColor = AppTheme.Text;
        _dateLine.ForeColor = AppTheme.Muted;
        for (var i = 0; i < TileColumns; i++)
            _tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / TileColumns));
        _tiles.RowStyles.Add(new RowStyle(SizeType.Absolute, AppTheme.TileHeight));
        Controls.Add(_greeting);
        Controls.Add(_dateLine);
        Controls.Add(_tiles);
    }

    public event EventHandler<NavItem>? TileClicked;

    public void ShowGreeting(string greeting, string dateLine)
    {
        _greeting.Text = greeting;
        _dateLine.Text = dateLine;
    }

    public void ShowTiles(IReadOnlyList<NavItem> items)
    {
        _tiles.SuspendLayout();
        _tiles.Controls.Clear();
        for (var i = 0; i < items.Count; i++)
        {
            var tile = CreateTile(items[i]);
            // The table has no outer gap, so each tile keeps a 16px gap to its right neighbour only.
            tile.Margin = new Padding(0, 0, i % TileColumns < TileColumns - 1 ? AppTheme.Gap : 0, 0);
            _tiles.Controls.Add(tile, i % TileColumns, i / TileColumns);
        }

        _tiles.ResumeLayout();
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        _tiles.Width = ClientSize.Width;
    }

    private CardPanel CreateTile(NavItem item)
    {
        var tile = new CardPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            Cursor = Cursors.Hand,
            Name = "tile-" + item.Key,
        };

        var glyph = new Label
        {
            AutoSize = true,
            Location = new Point(16, 18),
            Font = AppTheme.IconFont(18F),
            ForeColor = AppTheme.Accent,
            Text = item.Tile?.Glyph ?? item.Glyph,
            UseMnemonic = false,
        };
        var shortcut = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Padding = new Padding(8, 2, 8, 2),
            BackColor = AppTheme.AccentSoft,
            ForeColor = AppTheme.AccentHover,
            Font = AppTheme.LabelFont,
            Text = item.ShortcutText,
            Visible = item.ShortcutText.Length > 0,
            UseMnemonic = false,
        };
        var title = new Label
        {
            AutoSize = true,
            Location = new Point(18, 60),
            Font = AppTheme.CardTitleFont,
            ForeColor = AppTheme.Text,
            Text = item.Tile?.Title ?? item.Caption,
            UseMnemonic = false,
        };
        var description = new Label
        {
            AutoSize = false,
            AutoEllipsis = true,
            Location = new Point(18, 86),
            Height = 18,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Font = AppTheme.SmallFont,
            ForeColor = AppTheme.Muted,
            Text = item.Tile?.Description ?? "",
            UseMnemonic = false,
        };

        tile.Controls.AddRange([glyph, shortcut, title, description]);
        tile.Layout += (_, _) =>
        {
            shortcut.Location = new Point(tile.Width - shortcut.Width - 14, 14);
            description.Width = tile.Width - 36;
        };

        foreach (Control part in tile.Controls.Cast<Control>().Append(tile))
        {
            part.Click += (_, _) => TileClicked?.Invoke(this, item);
            part.MouseEnter += (_, _) => tile.IsHighlighted = true;
            part.MouseLeave += (_, _) => tile.IsHighlighted = tile.ClientRectangle.Contains(tile.PointToClient(MousePosition));
        }

        return tile;
    }
}
