using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Administration;

public interface IFacilityInfoStore
{
    /// <summary>The single unit-information row (Id = 1), or null on a database that was never seeded.</summary>
    Task<FacilityInfo?> GetAsync(CancellationToken ct = default);
}
