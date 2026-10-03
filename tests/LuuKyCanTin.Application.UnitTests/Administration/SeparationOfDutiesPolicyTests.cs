using System.Text.Json;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.Administration;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

public class SeparationOfDutiesPolicyTests
{
    private const int CreatorUserId = 1;
    private const int ApproverUserId = 2;
    private const int CreatorOfficerId = 10;
    private const int ApproverOfficerId = 20;
    private const string DocumentTable = "CustodyVoucher";
    private const long DocumentId = 42;

    private readonly InMemoryAppDbContext _db = new();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();
    private readonly SeparationOfDutiesPolicy _policy;

    public SeparationOfDutiesPolicyTests()
    {
        _policy = new SeparationOfDutiesPolicy(_db, _auditLog);
    }

    private void SeedUser(int userId, int? officerId)
    {
        _db.User.Add(new User { Id = userId, UserName = "user" + userId, OfficerId = officerId });
        _db.SaveChanges();
    }

    private async Task AssertRefusedAsync(int creatorUserId, int approverUserId)
    {
        var error = await Should.ThrowAsync<SeparationOfDutiesViolationException>(
            () => _policy.EnsureCanApproveAsync(creatorUserId, approverUserId, DocumentTable, DocumentId));

        error.Message.ShouldBe("Người lập không được tự duyệt.");

        var call = _auditLog.ReceivedCalls().ShouldHaveSingleItem();
        call.GetMethodInfo().Name.ShouldBe(nameof(IAuditLogWriter.WriteAsync));
        ((AuditAction)call.GetArguments()[0]!).ShouldBe(AuditAction.Approve);
        call.GetArguments()[1].ShouldBe(DocumentTable);
        call.GetArguments()[2].ShouldBe(DocumentId);
        JsonSerializer.Serialize(call.GetArguments()[3]).ShouldBe(
            $"{{\"Result\":\"TuChoi\",\"Reason\":\"TachNhiemVu\",\"CreatorUserId\":{creatorUserId},\"ApproverUserId\":{approverUserId}}}");
    }

    [Fact]
    public async Task EnsureCanApproveAsync_SameUser_WritesOneRejectedApproveRowThenThrows()
    {
        SeedUser(CreatorUserId, CreatorOfficerId);

        await AssertRefusedAsync(CreatorUserId, CreatorUserId);
    }

    [Fact]
    public async Task EnsureCanApproveAsync_TwoAccountsOfTheSameOfficer_WritesOneRejectedApproveRowThenThrows()
    {
        SeedUser(CreatorUserId, CreatorOfficerId);
        SeedUser(ApproverUserId, CreatorOfficerId);

        await AssertRefusedAsync(CreatorUserId, ApproverUserId);
    }

    [Fact]
    public async Task EnsureCanApproveAsync_DifferentStaff_WritesNothing()
    {
        SeedUser(CreatorUserId, CreatorOfficerId);
        SeedUser(ApproverUserId, ApproverOfficerId);

        await _policy.EnsureCanApproveAsync(CreatorUserId, ApproverUserId, DocumentTable, DocumentId);

        _auditLog.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory]
    [InlineData(99, ApproverUserId)]
    [InlineData(CreatorUserId, 99)]
    public async Task EnsureCanApproveAsync_UnknownUserId_ThrowsInvalidOperationAndWritesNothing(int creatorUserId, int approverUserId)
    {
        SeedUser(CreatorUserId, CreatorOfficerId);
        SeedUser(ApproverUserId, ApproverOfficerId);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _policy.EnsureCanApproveAsync(creatorUserId, approverUserId, DocumentTable, DocumentId));

        _auditLog.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task EnsureCanApproveAsync_AnApprovalTransactionIsAlreadyOpen_ThrowsInvalidOperationAndWritesNothing()
    {
        SeedUser(CreatorUserId, CreatorOfficerId);
        SeedUser(ApproverUserId, ApproverOfficerId);
        _db.HasActiveTransaction = true;

        await Should.ThrowAsync<InvalidOperationException>(
            () => _policy.EnsureCanApproveAsync(CreatorUserId, ApproverUserId, DocumentTable, DocumentId));

        _auditLog.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EnsureCanApproveAsync_BlankTableName_IsRejected(string tableName)
    {
        await Should.ThrowAsync<ArgumentException>(
            () => _policy.EnsureCanApproveAsync(CreatorUserId, ApproverUserId, tableName, DocumentId));

        _auditLog.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task EnsureCanApproveAsync_NonPositiveRecordId_IsRejected(long recordId)
    {
        await Should.ThrowAsync<ArgumentOutOfRangeException>(
            () => _policy.EnsureCanApproveAsync(CreatorUserId, ApproverUserId, DocumentTable, recordId));

        _auditLog.ReceivedCalls().ShouldBeEmpty();
    }
}
