using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

public sealed class SeparationOfDutiesPolicyTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private readonly AppDatabaseFixture _fixture;
    private readonly List<AppDbContext> _contexts = [];

    public SeparationOfDutiesPolicyTests(AppDatabaseFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var context in _contexts)
            await context.DisposeAsync();
    }

    private AppDbContext NewContext()
    {
        var db = _fixture.CreateAuditedContext();
        _contexts.Add(db);
        return db;
    }

    [SqlServerFact]
    public async Task EnsureCanApproveAsync_AccountsOfTheSameOfficer_RefusesAndTheAuditRowSurvivesWithoutAnOuterTransaction()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        await using var seedDb = NewContext();
        var officer = new Officer("S" + suffix.ToUpperInvariant(), "Cán bộ " + suffix, "Cán bộ", isSupervisingOfficer: false);
        seedDb.Officer.Add(officer);
        await seedDb.SaveChangesAsync();

        // One person, two accounts over time: an inactive creator and the active account that now approves.
        // The 2.4 filtered index allows both because only one of them is active.
        var creator = new User { UserName = "creator" + suffix, OfficerId = officer.Id, PasswordHash = "x", IsActive = false };
        var approver = new User { UserName = "approver" + suffix, OfficerId = officer.Id, PasswordHash = "x", IsActive = true };
        seedDb.User.AddRange(creator, approver);
        await seedDb.SaveChangesAsync();

        await _fixture.SignInAsync(approver);

        var documentId = Random.Shared.NextInt64(1, long.MaxValue);
        await using var db = NewContext();
        var policy = new SeparationOfDutiesPolicy(
            db,
            new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User)));

        var error = await Should.ThrowAsync<SeparationOfDutiesViolationException>(
            () => policy.EnsureCanApproveAsync(creator.Id, approver.Id, "CustodyVoucher", documentId));

        error.Message.ShouldBe(SeparationOfDutiesViolationException.ViolationMessage);

        // A fresh context sees the row: the policy opened no transaction, so the refusal was committed on its own.
        await using var verify = NewContext();
        var rows = await verify.AuditLog
            .Where(log => log.TableName == "CustodyVoucher" && log.RecordId == documentId)
            .ToListAsync();

        var auditRow = rows.ShouldHaveSingleItem();
        auditRow.Action.ShouldBe(AuditAction.Approve);
        auditRow.UserId.ShouldBe(approver.Id);
        auditRow.NewValues.ShouldNotBeNull();
        auditRow.NewValues.ShouldContain("\"Result\":\"TuChoi\"");
        auditRow.NewValues.ShouldContain("\"Reason\":\"TachNhiemVu\"");
        auditRow.NewValues.ShouldContain($"\"CreatorUserId\":{creator.Id}");
        auditRow.NewValues.ShouldContain($"\"ApproverUserId\":{approver.Id}");
    }
}
