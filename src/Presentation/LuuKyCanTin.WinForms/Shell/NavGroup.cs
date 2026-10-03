namespace LuuKyCanTin.WinForms.Shell;

/// <summary>A collapsible sidebar group under a caps section header ("NGHIỆP VỤ", "QUẢN LÝ").</summary>
public sealed record NavGroup(string Key, string Section, string Caption, string Glyph, IReadOnlyList<NavItem> Items);
