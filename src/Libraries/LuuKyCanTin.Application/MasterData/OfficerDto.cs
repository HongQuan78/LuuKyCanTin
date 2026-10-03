namespace LuuKyCanTin.Application.MasterData;

/// <param name="RowVer">Sent back with an edit, so a change someone else made in the meantime is detected.</param>
public sealed record OfficerDto(
    int Id,
    string OfficerCode,
    string FullName,
    string? Position,
    bool IsSupervisingOfficer,
    bool IsActive,
    byte[] RowVer);
