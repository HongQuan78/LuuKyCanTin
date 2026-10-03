using System.ComponentModel;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// The dark navigation column: brand block, Trang chủ, the caps section headers with their groups (one expanded at a
/// time) and the user block at the bottom. Built from a <see cref="NavigationModel"/>, not from Designer controls, so
/// a permission-driven model can replace the static one.
/// </summary>
internal sealed class Sidebar : UserControl
{
    private const int BrandHeight = 56;
    private const int UserBlockHeight = 57;
    private const int ItemTextIndent = 44;

    private readonly Label _facilityName = new();
    private readonly Label _initials = new();
    private readonly Label _userName = new();
    private readonly Label _roleText = new();
    private readonly FlowLayoutPanel _nav = new();
    private readonly Dictionary<string, NavButton> _itemButtons = [];
    private readonly List<(NavButton Header, List<NavButton> Items)> _groups = [];

    public Sidebar()
    {
        BackColor = AppTheme.Side;
        Dock = DockStyle.Left;
        Width = AppTheme.SidebarWidth;
        Font = AppTheme.BodyFont;

        _nav.Dock = DockStyle.Fill;
        _nav.FlowDirection = FlowDirection.TopDown;
        _nav.WrapContents = false;
        _nav.AutoScroll = true;
        _nav.Padding = new Padding(0, 8, 0, 8);

        Controls.Add(_nav);
        Controls.Add(CreateUserBlock());
        Controls.Add(CreateBrand());
    }

    /// <summary>A sidebar item, Trang chủ included, was clicked or chosen with its mnemonic.</summary>
    public event EventHandler<NavItem>? ItemClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string FacilityName
    {
        set => _facilityName.Text = value;
    }

    public void ShowNavigation(NavigationModel navigation)
    {
        _nav.SuspendLayout();
        _nav.Controls.Clear();
        _itemButtons.Clear();
        _groups.Clear();

        _nav.Controls.Add(CreateItemButton(navigation.Home, AppTheme.NavItemHeight, ItemTextIndent));

        string? section = null;
        foreach (var group in navigation.Groups)
        {
            if (group.Section != section)
            {
                section = group.Section;
                _nav.Controls.Add(CreateSectionHeader(section));
            }

            var header = new NavButton(group.Caption, group.Glyph, AppTheme.NavItemHeight, ItemTextIndent)
            {
                Name = "nav-" + group.Key,
                TrailingText = Glyphs.ChevronRight,
                IsTrailingGlyph = true,
            };
            var items = group.Items
                .Select(item => CreateItemButton(item, AppTheme.NavSubHeight, AppTheme.NavSubIndent))
                .ToList();
            var entry = (header, items);
            header.Click += (_, _) => ToggleGroup(entry);
            _groups.Add(entry);

            _nav.Controls.Add(header);
            foreach (var item in items)
            {
                item.Visible = false;
                _nav.Controls.Add(item);
            }
        }

        _nav.ResumeLayout();
    }

    public void ShowUser(string initials, string displayName, string roleText)
    {
        _initials.Text = initials;
        _userName.Text = displayName;
        _roleText.Text = roleText;
    }

    /// <summary>Marks the entry shown in the content area, and opens its group if it is a sub-item.</summary>
    public void SetActive(string key)
    {
        foreach (var (itemKey, button) in _itemButtons)
            button.IsActive = itemKey == key;

        foreach (var group in _groups.Where(g => g.Items.Any(i => i.IsActive)))
            ExpandGroup(group);
    }

    private NavButton CreateItemButton(NavItem item, int height, int textIndent)
    {
        var button = new NavButton(item.Caption, item.Glyph, height, textIndent)
        {
            Name = "nav-" + item.Key,
            TrailingText = item.ShortcutText,
        };
        button.Click += (_, _) => ItemClicked?.Invoke(this, item);
        _itemButtons[item.Key] = button;
        return button;
    }

    private void ToggleGroup((NavButton Header, List<NavButton> Items) group)
    {
        var isExpanded = group.Items.Count > 0 && group.Items[0].Visible;
        if (isExpanded)
            CollapseGroup(group);
        else
            ExpandGroup(group);
    }

    // Only one group is open at a time.
    private void ExpandGroup((NavButton Header, List<NavButton> Items) group)
    {
        _nav.SuspendLayout();
        foreach (var other in _groups.Where(g => g.Header != group.Header))
            CollapseGroup(other);

        group.Header.TrailingText = Glyphs.ChevronDown;
        foreach (var item in group.Items)
            item.Visible = true;
        _nav.ResumeLayout();
    }

    private static void CollapseGroup((NavButton Header, List<NavButton> Items) group)
    {
        group.Header.TrailingText = Glyphs.ChevronRight;
        foreach (var item in group.Items)
            item.Visible = false;
    }

    private static Label CreateSectionHeader(string text) => new()
    {
        AutoSize = false,
        Size = new Size(AppTheme.SidebarWidth, 35),
        Padding = new Padding(16, 14, 16, 0),
        Margin = Padding.Empty,
        Font = AppTheme.SmallFont,
        ForeColor = AppTheme.SideMuted,
        Text = text,
        UseMnemonic = false,
    };

    private Panel CreateBrand()
    {
        var brand = new Panel { Dock = DockStyle.Top, Height = BrandHeight };
        brand.Paint += (_, e) => PaintLine(e.Graphics, 0, BrandHeight - 1);

        var logo = new Label
        {
            Bounds = new Rectangle(16, 12, 32, 32),
            BackColor = AppTheme.Accent,
            ForeColor = AppTheme.OnAccent,
            Font = AppTheme.LabelFont,
            Text = "LK",
            TextAlign = ContentAlignment.MiddleCenter,
            UseMnemonic = false,
        };
        var name = new Label
        {
            AutoSize = true,
            Location = new Point(56, 10),
            ForeColor = AppTheme.OnAccent,
            Font = AppTheme.CardTitleFont,
            Text = "Lưu ký – Căn tin",
            UseMnemonic = false,
        };
        _facilityName.AutoSize = false;
        _facilityName.Bounds = new Rectangle(58, 31, AppTheme.SidebarWidth - 66, 16);
        _facilityName.AutoEllipsis = true;
        _facilityName.ForeColor = AppTheme.SideMuted;
        _facilityName.Font = AppTheme.SmallFont;
        _facilityName.UseMnemonic = false;

        brand.Controls.AddRange([logo, name, _facilityName]);
        return brand;
    }

    private Panel CreateUserBlock()
    {
        var block = new Panel { Dock = DockStyle.Bottom, Height = UserBlockHeight };
        block.Paint += (_, e) => PaintLine(e.Graphics, 0, 0);

        _initials.Bounds = new Rectangle(16, 13, 32, 32);
        _initials.BackColor = AppTheme.Label;
        _initials.ForeColor = AppTheme.OnAccent;
        _initials.Font = AppTheme.LabelFont;
        _initials.TextAlign = ContentAlignment.MiddleCenter;
        _initials.UseMnemonic = false;

        _userName.AutoSize = false;
        _userName.AutoEllipsis = true;
        _userName.Bounds = new Rectangle(58, 11, AppTheme.SidebarWidth - 70, 19);
        _userName.ForeColor = AppTheme.OnAccent;
        _userName.Font = AppTheme.BodySemiboldFont;
        _userName.UseMnemonic = false;

        _roleText.AutoSize = false;
        _roleText.AutoEllipsis = true;
        _roleText.Bounds = new Rectangle(58, 31, AppTheme.SidebarWidth - 70, 16);
        _roleText.ForeColor = AppTheme.SideMuted;
        _roleText.Font = AppTheme.SmallFont;
        _roleText.UseMnemonic = false;

        block.Controls.AddRange([_initials, _userName, _roleText]);
        return block;
    }

    private static void PaintLine(Graphics graphics, int x, int y)
    {
        using var line = new SolidBrush(AppTheme.SideHover);
        graphics.FillRectangle(line, x, y, AppTheme.SidebarWidth, 1);
    }
}
