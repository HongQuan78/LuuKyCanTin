namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// One permission in the fixed catalogue (module × action). Reference data: it changes only through migrations,
/// so it has no audit columns and no row version.
/// </summary>
public sealed class Permission
{
    public int Id { get; set; }

    /// <summary>The contract code, for example <c>LK-C.Duyet</c>.</summary>
    public string Code { get; set; } = "";

    /// <summary>Vietnamese display name, for example "Duyệt — Giảm tiền lưu ký".</summary>
    public string Name { get; set; } = "";

    /// <summary>An abbreviation such as HT, DM or HH-BC.</summary>
    public string Module { get; set; } = "";
}
