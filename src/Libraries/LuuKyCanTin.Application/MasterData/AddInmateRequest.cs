using LuuKyCanTin.Domain.MasterData;

namespace LuuKyCanTin.Application.MasterData;

public sealed record AddInmateRequest(
    string InmateCode,
    string FullName,
    short? BirthYear,
    InmateType InmateType,
    DateOnly AdmissionDate,
    string? Cell);
