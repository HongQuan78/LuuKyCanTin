namespace LuuKyCanTin.Domain.HeThong;

/// <summary>
/// One permission in the fixed catalogue (module × action). Reference data: it changes only through migrations,
/// so it has no audit columns and no row version.
/// </summary>
public sealed class Quyen
{
    public int Id { get; set; }

    /// <summary>The contract code, for example <c>LK-C.Duyet</c>.</summary>
    public string Ma { get; set; } = "";

    /// <summary>Vietnamese display name, for example "Duyệt — Giảm tiền lưu ký".</summary>
    public string Ten { get; set; } = "";

    /// <summary>An abbreviation such as HT, DM or HH-BC.</summary>
    public string Module { get; set; } = "";
}
