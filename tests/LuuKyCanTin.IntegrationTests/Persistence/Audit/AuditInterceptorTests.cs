using System.Text.Json;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Audit;

public sealed class AuditInterceptorTests : IClassFixture<AuditDatabaseFixture>
{
    private const int UserId = 7;

    private readonly AuditDatabaseFixture _fixture;

    public AuditInterceptorTests(AuditDatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.User.SignIn(UserId, "thuquy", null, "thuquy", []);
    }

    private static SampleVoucher CreateVoucher(string description = "Nộp tiền lưu ký") => new()
    {
        Amount = 100_000,
        VoucherDate = new DateOnly(2026, 10, 1),
        Description = description,
        Status = SampleStatus.Posted,
        Secret = "bí-mật-ban-đầu",
    };

    private async Task<SampleVoucher> AddAsync(SampleVoucher voucher)
    {
        await using var db = _fixture.CreateAuditedDbContext();
        db.SampleVoucher.Add(voucher);
        await db.SaveChangesAsync();
        return voucher;
    }

    private async Task UpdateAsync(int id, Action<SampleVoucher> change)
    {
        // Load, change, save: the "before" values come from the tracked query.
        await using var db = _fixture.CreateAuditedDbContext();
        change(await db.SampleVoucher.SingleAsync(v => v.Id == id));
        await db.SaveChangesAsync();
    }

    private async Task<List<AuditLog>> GetAuditLogsAsync(int id)
    {
        await using var db = _fixture.CreateDbContext();
        return await db.AuditLog.Where(n => n.TableName == "SampleVoucher" && n.RecordId == id).OrderBy(n => n.Id).ToListAsync();
    }

    private async Task<int> CountAuditLogsAsync()
    {
        await using var db = _fixture.CreateDbContext();
        return await db.AuditLog.CountAsync();
    }

    private static Dictionary<string, JsonElement> ReadJson(string? json) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json.ShouldNotBeNull())!;

    [SqlServerFact]
    public async Task SaveChanges_NewVoucher_WritesCreateRowWithNewValues()
    {
        var voucher = await AddAsync(CreateVoucher());

        var auditLog = (await GetAuditLogsAsync(voucher.Id)).ShouldHaveSingleItem();

        auditLog.Action.ShouldBe(AuditAction.Create);
        auditLog.OccurredAt.ShouldBe(_fixture.Clock.Now);
        auditLog.UserId.ShouldBe(UserId);
        auditLog.Workstation.ShouldBe(Environment.MachineName);
        auditLog.OldValues.ShouldBeNull();
        var newValues = ReadJson(auditLog.NewValues);
        newValues["Id"].GetInt32().ShouldBe(voucher.Id);
        newValues["Amount"].GetDecimal().ShouldBe(100_000m);
        newValues["Description"].GetString().ShouldBe("Nộp tiền lưu ký");
        newValues["Status"].GetString().ShouldBe(nameof(SampleStatus.Posted));
        newValues.Keys.ShouldNotContain("RowVer");
        newValues.Keys.ShouldNotContain("CreatedAt");
        newValues.Keys.ShouldNotContain("CreatedById");
    }

    [SqlServerFact]
    public async Task SaveChanges_VietnameseText_StaysReadableInTheJson()
    {
        var voucher = await AddAsync(CreateVoucher());

        (await GetAuditLogsAsync(voucher.Id)).ShouldHaveSingleItem().NewValues.ShouldNotBeNull().ShouldContain("\"Description\":\"Nộp tiền lưu ký\"");
    }

    [SqlServerFact]
    public async Task SaveChanges_NewVoucher_StoresTheActionAsText()
    {
        var voucher = await AddAsync(CreateVoucher());

        (await _fixture.Database.GetScalarAsync($"SELECT [Action] FROM AuditLog WHERE TableName = 'SampleVoucher' AND RecordId = {voucher.Id}"))
            .ShouldBe("Them");
    }

    [SqlServerFact]
    public async Task SaveChanges_NewVoucher_FillsCreationAuditColumns()
    {
        var voucher = await AddAsync(CreateVoucher());

        await using var db = _fixture.CreateDbContext();
        var savedVoucher = await db.SampleVoucher.SingleAsync(v => v.Id == voucher.Id);
        savedVoucher.CreatedAt.ShouldBe(_fixture.Clock.Now);
        savedVoucher.CreatedById.ShouldBe(UserId);
        savedVoucher.ModifiedAt.ShouldBeNull();
        savedVoucher.ModifiedById.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task SaveChanges_ChangedVoucher_WritesUpdateRowWithOnlyTheChangedColumns()
    {
        var voucher = await AddAsync(CreateVoucher());

        await UpdateAsync(voucher.Id, v => v.Amount = 150_000);

        var auditLog = (await GetAuditLogsAsync(voucher.Id)).Last();
        auditLog.Action.ShouldBe(AuditAction.Update);
        var oldValues = ReadJson(auditLog.OldValues);
        var newValues = ReadJson(auditLog.NewValues);
        oldValues.Keys.ShouldBe(["Amount"]);
        newValues.Keys.ShouldBe(["Amount"]);
        oldValues["Amount"].GetDecimal().ShouldBe(100_000m);
        newValues["Amount"].GetDecimal().ShouldBe(150_000m);
    }

    [SqlServerFact]
    public async Task SaveChanges_ChangedVoucher_FillsModificationColumnsAndKeepsCreationOnes()
    {
        var voucher = await AddAsync(CreateVoucher());
        var createdAt = _fixture.Clock.Now;
        _fixture.Clock.Advance(TimeSpan.FromMinutes(5));
        _fixture.User.SignIn(UserId + 1, "ketoan", null, "ketoan", []);
        try
        {
            await UpdateAsync(voucher.Id, v =>
            {
                v.Amount = 120_000;
                v.CreatedAt = DateTime.MinValue;
            });
        }
        finally
        {
            _fixture.User.SignIn(UserId, "thuquy", null, "thuquy", []);
        }

        await using var db = _fixture.CreateDbContext();
        var savedVoucher = await db.SampleVoucher.SingleAsync(v => v.Id == voucher.Id);
        savedVoucher.CreatedAt.ShouldBe(createdAt);
        savedVoucher.CreatedById.ShouldBe(UserId);
        savedVoucher.ModifiedAt.ShouldBe(_fixture.Clock.Now);
        savedVoucher.ModifiedById.ShouldBe(UserId + 1);
    }

    [SqlServerFact]
    public async Task SaveChanges_CancelledVoucher_WritesCancelRowNotUpdate()
    {
        var voucher = await AddAsync(CreateVoucher());

        await UpdateAsync(voucher.Id, v => v.Status = SampleStatus.Cancelled);

        var auditLog = (await GetAuditLogsAsync(voucher.Id)).Last();
        auditLog.Action.ShouldBe(AuditAction.Cancel);
        ReadJson(auditLog.OldValues)["Status"].GetString().ShouldBe(nameof(SampleStatus.Posted));
        ReadJson(auditLog.NewValues)["Status"].GetString().ShouldBe(nameof(SampleStatus.Cancelled));
    }

    [SqlServerFact]
    public async Task SaveChanges_AlreadyCancelledVoucherEdited_WritesUpdateRow()
    {
        var voucher = await AddAsync(CreateVoucher());
        await UpdateAsync(voucher.Id, v => v.Status = SampleStatus.Cancelled);

        await UpdateAsync(voucher.Id, v => v.Description = "Huỷ do ghi nhầm đối tượng");

        var auditLog = (await GetAuditLogsAsync(voucher.Id)).Last();
        auditLog.Action.ShouldBe(AuditAction.Update);
        ReadJson(auditLog.NewValues).Keys.ShouldBe(["Description"]);
    }

    [SqlServerFact]
    public async Task SaveChanges_NoRealChange_WritesNoRow()
    {
        var voucher = await AddAsync(CreateVoucher());

        await UpdateAsync(voucher.Id, v => v.Amount = v.Amount);

        (await GetAuditLogsAsync(voucher.Id)).ShouldHaveSingleItem().Action.ShouldBe(AuditAction.Create);
    }

    [SqlServerFact]
    public async Task SaveChanges_DetachedUpdate_ThrowsAndWritesNothing()
    {
        var voucher = await AddAsync(CreateVoucher());
        var detachedVoucher = CreateVoucher();
        detachedVoucher.Id = voucher.Id;
        detachedVoucher.Amount = 999_000;
        detachedVoucher.RowVer = voucher.RowVer;

        await using (var db = _fixture.CreateAuditedDbContext())
        {
            db.SampleVoucher.Update(detachedVoucher);

            (await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync())).Message.ShouldContain("Load it");
        }

        await using var check = _fixture.CreateDbContext();
        (await check.SampleVoucher.SingleAsync(v => v.Id == voucher.Id)).Amount.ShouldBe(100_000m);
        (await GetAuditLogsAsync(voucher.Id)).ShouldHaveSingleItem().Action.ShouldBe(AuditAction.Create);
    }

    [SqlServerFact]
    public async Task SaveChanges_ExcludedPropertyChanged_LogsTheChangeWithoutItsValue()
    {
        var voucher = await AddAsync(CreateVoucher());

        await UpdateAsync(voucher.Id, v => v.Secret = "bí-mật-mới");

        var auditLogs = await GetAuditLogsAsync(voucher.Id);
        auditLogs.Count.ShouldBe(2);
        auditLogs[1].Action.ShouldBe(AuditAction.Update);
        auditLogs.ShouldAllBe(n => !(n.OldValues ?? "").Contains("bí-mật") && !(n.NewValues ?? "").Contains("bí-mật"));
        auditLogs.ShouldAllBe(n => !(n.OldValues ?? "").Contains("Secret") && !(n.NewValues ?? "").Contains("Secret"));
    }

    [SqlServerFact]
    public async Task SaveChanges_InsertFails_RollsBackBothTheEntityAndItsAuditRows()
    {
        var marker = Guid.NewGuid().ToString("N");
        var auditLogCount = await CountAuditLogsAsync();

        await using (var db = _fixture.CreateAuditedDbContext())
        {
            db.SampleVoucher.Add(CreateVoucher(marker));
            // Violates CK_SampleVoucher_Status, so the batch fails after the first insert.
            var invalidVoucher = CreateVoucher(marker);
            invalidVoucher.Status = (SampleStatus)9;
            db.SampleVoucher.Add(invalidVoucher);

            await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }

        await using var check = _fixture.CreateDbContext();
        (await check.SampleVoucher.CountAsync(v => v.Description == marker)).ShouldBe(0);
        (await CountAuditLogsAsync()).ShouldBe(auditLogCount);
    }

    [SqlServerFact]
    public async Task SaveChanges_RetryInSameContextAfterFailure_StoresOnlyTheRetry()
    {
        var marker = Guid.NewGuid().ToString("N");

        await using (var db = _fixture.CreateAuditedDbContext())
        {
            db.SampleVoucher.Add(CreateVoucher(marker));
            var invalidVoucher = CreateVoucher(marker);
            invalidVoucher.Status = (SampleStatus)9;
            db.SampleVoucher.Add(invalidVoucher);
            await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());

            // The failed save's own transaction is gone, so the retry can't commit leftovers of the failed batch.
            db.Database.CurrentTransaction.ShouldBeNull();

            invalidVoucher.Status = SampleStatus.Posted;
            await db.SaveChangesAsync();
        }

        await using var check = _fixture.CreateDbContext();
        var ids = await check.SampleVoucher.Where(v => v.Description == marker).Select(v => v.Id).ToListAsync();
        ids.Count.ShouldBe(2);
        foreach (var id in ids)
            (await GetAuditLogsAsync(id)).ShouldHaveSingleItem().Action.ShouldBe(AuditAction.Create);
    }

    [SqlServerFact]
    public async Task SaveChanges_AuditWriteFails_RollsBackTheEntityToo()
    {
        const string rejectMarker = "TU-CHOI-NHAT-KY";
        // Test-only constraint: an audit row mentioning the marker fails, after the voucher INSERT has succeeded.
        await _fixture.Database.ExecuteAsync($"""
            IF OBJECT_ID('CK_Test_RejectAuditLog') IS NULL
                ALTER TABLE AuditLog ADD CONSTRAINT CK_Test_RejectAuditLog CHECK (NewValues NOT LIKE '%{rejectMarker}%')
            """);
        var marker = $"{rejectMarker}-{Guid.NewGuid():N}";
        var auditLogCount = await CountAuditLogsAsync();

        await using (var db = _fixture.CreateAuditedDbContext())
        {
            db.SampleVoucher.Add(CreateVoucher(marker));
            await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());

            // The rejected log row must not stay queued for the context's next save.
            db.ChangeTracker.Entries<AuditLog>().ShouldBeEmpty();
        }

        await using var check = _fixture.CreateDbContext();
        (await check.SampleVoucher.CountAsync(v => v.Description == marker)).ShouldBe(0);
        (await CountAuditLogsAsync()).ShouldBe(auditLogCount);
    }

    [SqlServerFact]
    public async Task SaveChanges_CallerTransactionRolledBack_UndoesTheAuditRowsToo()
    {
        var marker = Guid.NewGuid().ToString("N");
        var auditLogCount = await CountAuditLogsAsync();

        await using (var db = _fixture.CreateAuditedDbContext())
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            db.SampleVoucher.Add(CreateVoucher(marker));
            await db.SaveChangesAsync();

            db.Database.CurrentTransaction.ShouldBeSameAs(transaction);
            await transaction.RollbackAsync();
        }

        await using var check = _fixture.CreateDbContext();
        (await check.SampleVoucher.CountAsync(v => v.Description == marker)).ShouldBe(0);
        (await CountAuditLogsAsync()).ShouldBe(auditLogCount);
    }

    [SqlServerFact]
    public async Task SaveChanges_CallerTransactionCommitted_KeepsEntityAndAuditRow()
    {
        await using var db = _fixture.CreateAuditedDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var voucher = CreateVoucher();
        db.SampleVoucher.Add(voucher);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        (await GetAuditLogsAsync(voucher.Id)).ShouldHaveSingleItem().Action.ShouldBe(AuditAction.Create);
    }

    [SqlServerFact]
    public void SaveChanges_Synchronous_IsAuditedToo()
    {
        var voucher = CreateVoucher();
        using (var db = _fixture.CreateAuditedDbContext())
        {
            db.SampleVoucher.Add(voucher);
            db.SaveChanges();
        }

        GetAuditLogsAsync(voucher.Id).GetAwaiter().GetResult().ShouldHaveSingleItem().Action.ShouldBe(AuditAction.Create);
    }

    [SqlServerFact]
    public async Task SaveChanges_DeletedVoucher_Throws()
    {
        var voucher = await AddAsync(CreateVoucher());

        await using var db = _fixture.CreateAuditedDbContext();
        db.SampleVoucher.Remove(await db.SampleVoucher.SingleAsync(v => v.Id == voucher.Id));

        (await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync())).Message.ShouldContain("cancel");
    }

    [SqlServerFact]
    public async Task SaveChanges_ModifiedAuditRow_Throws()
    {
        var voucher = await AddAsync(CreateVoucher());

        await using var db = _fixture.CreateAuditedDbContext();
        var auditLog = await db.AuditLog.FirstAsync(n => n.TableName == "SampleVoucher" && n.RecordId == voucher.Id);
        db.Entry(auditLog).Property(n => n.NewValues).CurrentValue = "{}";

        (await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync())).Message.ShouldContain("append-only");
    }

    [SqlServerFact]
    public async Task SaveChanges_DeletedAuditRow_Throws()
    {
        var voucher = await AddAsync(CreateVoucher());

        await using var db = _fixture.CreateAuditedDbContext();
        db.AuditLog.Remove(await db.AuditLog.FirstAsync(n => n.TableName == "SampleVoucher" && n.RecordId == voucher.Id));

        Should.Throw<InvalidOperationException>(() => db.SaveChanges()).Message.ShouldContain("append-only");
    }
}
