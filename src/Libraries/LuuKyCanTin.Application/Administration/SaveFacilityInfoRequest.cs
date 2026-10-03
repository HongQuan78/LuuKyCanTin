namespace LuuKyCanTin.Application.Administration;

/// <param name="ParentAgencyName">Optional; blank clears it.</param>
/// <param name="RowVer">The version the user loaded. Required, so a missing version can never reach the service.</param>
public sealed record SaveFacilityInfoRequest(
    string? ParentAgencyName,
    string FacilityName,
    string Address,
    byte[] RowVer);
