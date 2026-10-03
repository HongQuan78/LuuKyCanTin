namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Everything the sidebar and the Trang chủ tiles show: Trang chủ first, then the groups in order.</summary>
public sealed record NavigationModel(NavItem Home, IReadOnlyList<NavGroup> Groups)
{
    public IEnumerable<NavItem> AllItems => Groups.SelectMany(g => g.Items).Prepend(Home);

    public IReadOnlyList<NavItem> Tiles => [.. AllItems.Where(i => i.Tile is not null)];

    public NavItem? FindByShortcut(Keys keys) =>
        keys == Keys.None ? null : AllItems.FirstOrDefault(i => i.Shortcut == keys);
}
