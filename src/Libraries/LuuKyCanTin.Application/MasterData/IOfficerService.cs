using LuuKyCanTin.Application.Common;

namespace LuuKyCanTin.Application.MasterData;

/// <summary>The staff register. Staff are never deleted; someone who has left is marked inactive.</summary>
public interface IOfficerService
{
    /// <exception cref="BusinessRuleException">Invalid input, or the code is already used.</exception>
    Task<OfficerDto> AddAsync(SaveOfficerRequest request, CancellationToken ct = default);

    /// <exception cref="BusinessRuleException">Invalid input, the code is already used, or the record doesn't exist.</exception>
    /// <exception cref="ConcurrencyConflictException">Someone else changed the record after <see cref="SaveOfficerRequest.RowVer"/> was loaded.</exception>
    Task<OfficerDto> UpdateAsync(int id, SaveOfficerRequest request, CancellationToken ct = default);

    /// <summary>Matches part of the code or the name, ignoring case and diacritics.</summary>
    Task<IReadOnlyList<OfficerDto>> SearchAsync(string? keyword, bool includeInactive, CancellationToken ct = default);

    /// <summary>The staff a selection list may offer (wardens, signers, accounts): active staff only.</summary>
    Task<IReadOnlyList<OfficerDto>> GetActiveOfficersAsync(bool supervisingOnly, CancellationToken ct = default);
}
