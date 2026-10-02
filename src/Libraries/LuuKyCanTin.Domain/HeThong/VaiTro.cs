using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.HeThong;

/// <summary>
/// One of the 6 standard roles. Roles are never added or deleted; their permission set is edited and audited,
/// which is why the row is audited.
/// </summary>
public sealed class VaiTro : AuditableEntity, IAuditable
{
    public int Id { get; set; }

    /// <summary>Stable code such as <c>QUAN_TRI</c>; the display name may be corrected, the code may not.</summary>
    public string Ma { get; set; } = "";

    public string Ten { get; set; } = "";
}
