namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// One stored signer line as the signatory screen and the print frame read it: title, optional default staff member
/// and that person's live state. The name is returned even after the person has left, so old prints stay readable.
/// </summary>
public sealed record SignatoryRowDto(
    byte Ordinal,
    string Title,
    int? OfficerId,
    string? OfficerFullName,
    bool IsOfficerActive);
