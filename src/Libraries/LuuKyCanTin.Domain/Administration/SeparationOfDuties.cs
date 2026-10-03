namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// FR3 / NEN-09: the account that created a document must never be the account that approves it.
/// </summary>
public static class SeparationOfDuties
{
    /// <summary>
    /// True when the approver is the creator: the same account, or two accounts linked to the same staff member.
    /// Comparing the staff record as well catches an older inactive account of the same person, since an account
    /// can be deactivated and replaced while the person stays (only one account per person is active at a time).
    /// </summary>
    /// <param name="creatorUserId">The user id of the account that created the document.</param>
    /// <param name="creatorOfficerId">The creator's staff record, or null for the built-in administrator.</param>
    /// <param name="approverUserId">The user id of the account attempting to approve.</param>
    /// <param name="approverOfficerId">The approver's staff record, or null for the built-in administrator.</param>
    public static bool IsSamePerson(
        int creatorUserId,
        int? creatorOfficerId,
        int approverUserId,
        int? approverOfficerId) =>
        creatorUserId == approverUserId
        || (creatorOfficerId is { } creator && approverOfficerId is { } approver && creator == approver);
}
