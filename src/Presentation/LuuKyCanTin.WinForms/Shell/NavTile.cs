namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The quick-action tile of a <see cref="NavItem"/>: a title without mnemonic, a muted description and an accent glyph.</summary>
public sealed record NavTile(string Title, string Description, string Glyph);
