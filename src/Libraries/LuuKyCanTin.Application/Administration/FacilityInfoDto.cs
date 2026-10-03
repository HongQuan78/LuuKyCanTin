namespace LuuKyCanTin.Application.Administration;

/// <param name="IsConfigured">True when both the name and the address are filled in, so a first-run checklist can tell an install that was never set up.</param>
public sealed record FacilityInfoDto(
    string? ParentAgencyName,
    string FacilityName,
    string Address,
    bool IsConfigured,
    byte[] RowVer);
