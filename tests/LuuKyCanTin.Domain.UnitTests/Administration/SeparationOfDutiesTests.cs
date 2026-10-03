using LuuKyCanTin.Domain.Administration;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.Administration;

public class SeparationOfDutiesTests
{
    [Theory]
    [InlineData(1, 10, 1, 10, true)]      // Same account.
    [InlineData(2, 10, 3, 10, true)]      // Two accounts of the same staff member.
    [InlineData(2, 10, 3, 20, false)]     // Different staff members.
    [InlineData(2, 10, 3, null, false)]   // The creator has a staff record, the approver has none.
    [InlineData(1, null, 1, null, true)]  // The administrator approving its own document.
    [InlineData(2, null, 3, null, false)] // Two accounts with no staff record.
    public void IsSamePerson_CreatorApproverPairs_ReturnsWhetherTheyAreTheSamePerson(
        int creatorUserId,
        int? creatorOfficerId,
        int approverUserId,
        int? approverOfficerId,
        bool expected)
    {
        SeparationOfDuties.IsSamePerson(creatorUserId, creatorOfficerId, approverUserId, approverOfficerId)
            .ShouldBe(expected);
    }

    [Fact]
    public void IsSamePerson_AdministratorApprovingAnotherUsersDocument_IsAllowedHereBecauseTheNoStaffApproverRuleBelongsToStory55()
    {
        // Alignment A17 forbids an account without a staff record from approving at all; Story 5.5 owns that rule.
        SeparationOfDuties.IsSamePerson(1, null, 2, 20).ShouldBeFalse();
    }
}
