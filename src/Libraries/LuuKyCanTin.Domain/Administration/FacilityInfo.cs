using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.Administration;

/// <summary>The unit header printed on every template. Exactly one row exists (Id = 1).</summary>
public sealed class FacilityInfo : AuditableEntity, IAuditable
{
    public int Id { get; set; }

    public string? ParentAgencyName { get; set; }

    public string FacilityName { get; set; } = "";

    public string Address { get; set; } = "";
}
