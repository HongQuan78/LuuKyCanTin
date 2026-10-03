using System.Text.Json;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

public sealed class AuditLogWriterTests(AuditDatabaseFixture fixture) : IClassFixture<AuditDatabaseFixture>
{
    [SqlServerFact]
    public async Task GhiAsync_SignedInUser_WritesUserMachineAndClockTime()
    {
        fixture.User.SignIn(42, "admin", null, "admin", []);
        fixture.Clock.Now = new DateTime(2026, 10, 2, 7, 15, 30);

        await using (var db = fixture.CreateAuditedDbContext())
            await new AuditLogWriter(db, fixture.AuditLogFactory).WriteAsync(AuditAction.SignIn, "User", 42, new { UserName = "admin" });

        await using var check = fixture.CreateDbContext();
        var auditLog = await check.AuditLog.SingleAsync(n => n.Action == AuditAction.SignIn && n.RecordId == 42);
        auditLog.UserId.ShouldBe(42);
        auditLog.Workstation.ShouldBe(Environment.MachineName);
        auditLog.OccurredAt.ShouldBe(new DateTime(2026, 10, 2, 7, 15, 30));
        auditLog.TableName.ShouldBe("User");
        auditLog.OldValues.ShouldBeNull();
        JsonDocument.Parse(auditLog.NewValues!).RootElement.GetProperty("UserName").GetString().ShouldBe("admin");
    }

    [SqlServerFact]
    public async Task GhiAsync_WithoutPayloadOrUser_WritesNullColumns()
    {
        fixture.User.SignOut();
        var tableName = $"In_{Guid.NewGuid():N}"[..30];

        await using (var db = fixture.CreateAuditedDbContext())
            await new AuditLogWriter(db, fixture.AuditLogFactory).WriteAsync(AuditAction.Print, tableName, null);

        await using var check = fixture.CreateDbContext();
        var auditLog = await check.AuditLog.SingleAsync(n => n.TableName == tableName);
        auditLog.Action.ShouldBe(AuditAction.Print);
        auditLog.UserId.ShouldBeNull();
        auditLog.RecordId.ShouldBeNull();
        auditLog.NewValues.ShouldBeNull();
    }
}
