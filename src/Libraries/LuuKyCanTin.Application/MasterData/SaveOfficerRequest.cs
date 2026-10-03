namespace LuuKyCanTin.Application.MasterData;

/// <param name="RowVer">Required for an edit: the version the user loaded. Ignored when adding.</param>
public sealed record SaveOfficerRequest(
    string OfficerCode,
    string FullName,
    string? Position,
    bool IsSupervisingOfficer,
    bool IsActive = true,
    byte[]? RowVer = null);
