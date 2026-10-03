using LuuKyCanTin.Domain.MasterData;

namespace LuuKyCanTin.Application.MasterData;

/// <summary>Persistence port for the detainee table. Narrow on purpose: only what the skeleton's use cases need.</summary>
public interface IInmateStore
{
    Task<Inmate?> FindByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Inmate>> GetInCustodyAsync(CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string inmateCode, CancellationToken ct = default);

    /// <summary>
    /// Adds and saves the detainee. Returns false when the unique <c>InmateCode</c> index rejects it because a
    /// concurrent save won the race; the failed entity must not stay tracked for the context's next save.
    /// </summary>
    Task<bool> AddAsync(Inmate inmate, CancellationToken ct = default);
}
