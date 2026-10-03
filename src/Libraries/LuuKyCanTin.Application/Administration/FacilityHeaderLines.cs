namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// The three header lines every printed template starts with, formatted once so the unit-information screen's preview
/// and the printed page cannot drift apart: the parent agency in capitals when set, then the unit name, then the
/// address line.
/// </summary>
public static class FacilityHeaderLines
{
    public static string FormatParentAgency(string? parentAgencyName) =>
        string.IsNullOrWhiteSpace(parentAgencyName) ? "" : parentAgencyName.Trim().ToUpperInvariant();

    public static string FormatFacilityName(string facilityName) => facilityName.Trim();

    public static string FormatAddressLine(string address) => $"Địa chỉ: {address.Trim()}";
}
