using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.Administration;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// FR3 / NEN-09: refuses an approval attempted by the person who created the document, and leaves an audit row for
/// every refusal. Later approval flows (transfer approval, opening-balance approval) call this policy instead of
/// re-implementing the check.
/// </summary>
public sealed class SeparationOfDutiesPolicy(IAppDbContext db, IAuditLogWriter auditLog)
{
    /// <summary>The <c>Result</c> value of a refusal payload. Stored data, so it keeps the spelling in the audit log.</summary>
    public const string RejectedResult = "TuChoi";

    /// <summary>The <c>Reason</c> value of a refusal payload. Stored data, so it keeps the spelling in the audit log.</summary>
    public const string SeparationOfDutiesReason = "TachNhiemVu";

    /// <summary>
    /// Throws when <paramref name="approverUserId"/> is the person who created the document.
    /// Callers must call this <b>before</b> beginning the approval transaction, right after the permission check:
    /// the refusal audit row is saved on its own, so a transaction already open would roll it back when the
    /// exception leaves the caller. The caller must also have no pending tracked changes, because the audit
    /// writer saves the current unit of work before it returns.
    /// </summary>
    /// <param name="creatorUserId">The document's creator user id.</param>
    /// <param name="approverUserId">The approving user id, normally the signed-in user.</param>
    /// <param name="tableName">The table of the document being approved, for the refusal's audit row.</param>
    /// <param name="recordId">The document being approved, for the refusal's audit row; must be positive.</param>
    /// <exception cref="InvalidOperationException">
    /// The context already has an open transaction, or either user id does not exist. Both are programming errors.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="tableName"/> is null, empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="recordId"/> is zero or negative.</exception>
    /// <exception cref="SeparationOfDutiesViolationException">The approver is the creator.</exception>
    public async Task EnsureCanApproveAsync(
        int creatorUserId,
        int approverUserId,
        string tableName,
        long recordId,
        CancellationToken ct = default)
    {
        if (db.HasActiveTransaction)
            throw new InvalidOperationException(
                "SeparationOfDutiesPolicy must be called before the approval transaction begins.");

        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(recordId);

        var officerIdByUserId = await db.User
            .AsNoTracking()
            .Where(user => user.Id == creatorUserId || user.Id == approverUserId)
            .ToDictionaryAsync(user => user.Id, user => user.OfficerId, ct);

        if (!officerIdByUserId.TryGetValue(creatorUserId, out var creatorOfficerId))
            throw new InvalidOperationException($"User {creatorUserId} does not exist.");
        if (!officerIdByUserId.TryGetValue(approverUserId, out var approverOfficerId))
            throw new InvalidOperationException($"User {approverUserId} does not exist.");

        if (!SeparationOfDuties.IsSamePerson(creatorUserId, creatorOfficerId, approverUserId, approverOfficerId))
            return;

        await auditLog.WriteAsync(
            AuditAction.Approve,
            tableName,
            recordId,
            new
            {
                Result = RejectedResult,
                Reason = SeparationOfDutiesReason,
                CreatorUserId = creatorUserId,
                ApproverUserId = approverUserId,
            },
            ct);

        throw new SeparationOfDutiesViolationException();
    }
}
