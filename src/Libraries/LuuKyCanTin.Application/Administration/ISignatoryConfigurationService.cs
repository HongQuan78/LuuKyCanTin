namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// The signer configuration of every print template. The read side is also the print frame's contract (story 4.4):
/// rows left to right by ordinal, with the default staff member's name when set.
/// </summary>
public interface ISignatoryConfigurationService
{
    /// <summary>The template's signer rows, ordered by <c>Ordinal</c>.</summary>
    /// <exception cref="System.InvalidOperationException"><paramref name="templateCode"/> is not in <see cref="LuuKyCanTin.Application.Reporting.TemplateCodes.All"/>.</exception>
    Task<IReadOnlyList<SignatoryRowDto>> GetByTemplateAsync(string templateCode, CancellationToken ct = default);

    /// <summary>
    /// Replaces the template's whole row set with <paramref name="signatories"/>, in the list's order.
    /// </summary>
    /// <exception cref="LuuKyCanTin.Application.Common.PermissionDeniedException">The caller lacks <see cref="PermissionCodes.Administration.Update"/>.</exception>
    /// <exception cref="LuuKyCanTin.Application.Common.BusinessRuleException">Unknown template code, no rows, more rows than the ordinal column holds, a missing or over-length title, an unknown officer, or a newly chosen officer who is not active.</exception>
    Task SaveAsync(string templateCode, IReadOnlyList<SignatoryLine> signatories, CancellationToken ct = default);
}
