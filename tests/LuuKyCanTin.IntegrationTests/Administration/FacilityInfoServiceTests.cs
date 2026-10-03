using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Custody;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Reports;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UglyToad.PdfPig;

namespace LuuKyCanTin.IntegrationTests.Administration;

public sealed class FacilityInfoServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private const string BaselineName = "TRẠI TẠM GIAM ABC";
    private const string BaselineAddress = "Xã ABC, huyện ABC, tỉnh ABC";
    private const string BaselineAgency = "CÔNG AN TỈNH ABC";

    private readonly AppDatabaseFixture _fixture;
    private readonly List<AppDbContext> _contexts = [];

    public FacilityInfoServiceTests(AppDatabaseFixture fixture) => _fixture = fixture;

    // Every test starts from the same three header lines, whatever the previous one saved.
    public Task InitializeAsync() => _fixture.Database.ExecuteAsync($"""
        UPDATE FacilityInfo
        SET ParentAgencyName = N'{BaselineAgency}',
            FacilityName = N'{BaselineName}',
            Address = N'{BaselineAddress}'
        WHERE Id = 1;
        """);

    public async Task DisposeAsync()
    {
        _fixture.User.SignOut();
        foreach (var context in _contexts)
            await context.DisposeAsync();
    }

    private AppDbContext NewContext()
    {
        var db = _fixture.CreateAuditedContext();
        _contexts.Add(db);
        return db;
    }

    private FacilityInfoService CreateService(AppDbContext db) =>
        new(db, new PermissionChecker(db, _fixture.User), new SaveFacilityInfoRequestValidator());

    private static SaveFacilityInfoRequest Request(FacilityInfoDto current, string? agency, string facilityName, string address) =>
        new(agency, facilityName, address, current.RowVer);

    private async Task SignInAdminAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        var user = await db.User.SingleAsync(u => u.UserName == "admin");
        await _fixture.SignInAsync(user);
    }

    /// <summary>A test-only user whose single role grants exactly the given codes.</summary>
    private async Task<User> CreateUserAsync(params string[] permissionCodes)
    {
        await using var db = NewContext();
        var officer = new Officer("T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            "Cán bộ " + Guid.NewGuid().ToString("N")[..6], "Cán bộ", isSupervisingOfficer: false);
        db.Officer.Add(officer);
        await db.SaveChangesAsync();

        var role = new Role { Code = "R" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(), Name = "Vai trò thử" };
        db.Role.Add(role);
        await db.SaveChangesAsync();

        var permissionIds = await db.Permission
            .Where(permission => permissionCodes.Contains(permission.Code))
            .Select(permission => permission.Id)
            .ToListAsync();
        foreach (var permissionId in permissionIds)
            db.RolePermission.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
        await db.SaveChangesAsync();

        var user = new User
        {
            UserName = "u" + Guid.NewGuid().ToString("N")[..12],
            OfficerId = officer.Id,
            PasswordHash = "PBKDF2-SHA256$1$abc$def",
            IsActive = true,
        };
        db.User.Add(user);
        await db.SaveChangesAsync();
        db.UserRole.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await db.SaveChangesAsync();

        return user;
    }

    private async Task<FacilityInfo> GetRowAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.FacilityInfo.AsNoTracking().SingleAsync(f => f.Id == 1);
    }

    private async Task<List<AuditLog>> GetFacilityAuditLogsAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.AuditLog
            .Where(n => n.TableName == "FacilityInfo")
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    private async Task<List<string>> GetPermissionCodesAsync(int userId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await (from userRole in db.UserRole
                      join rolePermission in db.RolePermission on userRole.RoleId equals rolePermission.RoleId
                      join permission in db.Permission on rolePermission.PermissionId equals permission.Id
                      where userRole.UserId == userId
                      select permission.Code).ToListAsync();
    }

    [SqlServerFact]
    public async Task Save_ChangedField_UpdatesTheRowAndWritesOneUpdateRowWithOnlyTheChangedFields()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);
        var before = await service.GetAsync();
        var logsBefore = (await GetFacilityAuditLogsAsync()).Count;

        await service.SaveAsync(Request(before, before.ParentAgencyName, "TRẠI TẠM GIAM MỚI", before.Address));

        var after = await GetRowAsync();
        after.FacilityName.ShouldBe("TRẠI TẠM GIAM MỚI");
        after.ParentAgencyName.ShouldBe(before.ParentAgencyName);
        after.Address.ShouldBe(before.Address);
        after.RowVer.ShouldNotBe(before.RowVer);

        var logs = await GetFacilityAuditLogsAsync();
        logs.Count.ShouldBe(logsBefore + 1);
        logs[^1].Action.ShouldBe(AuditAction.Update);
        logs[^1].RecordId.ShouldBe(1L);
        logs[^1].OldValues.ShouldBe($$"""{"FacilityName":"{{BaselineName}}"}""");
        logs[^1].NewValues.ShouldBe("""{"FacilityName":"TRẠI TẠM GIAM MỚI"}""");
    }

    [SqlServerFact]
    public async Task Get_BlankRow_IsNotConfiguredAndBecomesConfiguredAfterASave()
    {
        await SignInAdminAsync();
        await _fixture.Database.ExecuteAsync(
            "UPDATE FacilityInfo SET ParentAgencyName = NULL, FacilityName = N'', Address = N'' WHERE Id = 1");
        await using var db = NewContext();
        var service = CreateService(db);

        (await service.GetAsync()).IsConfigured.ShouldBeFalse();

        var blank = await service.GetAsync();
        await service.SaveAsync(Request(blank, null, BaselineName, BaselineAddress));

        (await service.GetAsync()).IsConfigured.ShouldBeTrue();
    }

    [SqlServerFact]
    public async Task Save_BlankFacilityName_IsRejectedAndWritesNothing()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);
        var before = await service.GetAsync();
        var logsBefore = (await GetFacilityAuditLogsAsync()).Count;

        await Should.ThrowAsync<RequestValidationException>(
            () => service.SaveAsync(Request(before, before.ParentAgencyName, "   ", before.Address)));

        var after = await GetRowAsync();
        after.FacilityName.ShouldBe(before.FacilityName);
        after.RowVer.ShouldBe(before.RowVer);
        (await GetFacilityAuditLogsAsync()).Count.ShouldBe(logsBefore);
    }

    [SqlServerFact]
    public async Task Save_BlankAgency_StoresNullAndLogsTheChange()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);
        var before = await service.GetAsync();

        await service.SaveAsync(Request(before, "   ", before.FacilityName, before.Address));

        (await GetRowAsync()).ParentAgencyName.ShouldBeNull();
        var log = (await GetFacilityAuditLogsAsync())[^1];
        log.OldValues.ShouldBe($$"""{"ParentAgencyName":"{{BaselineAgency}}"}""");
        log.NewValues.ShouldBe("""{"ParentAgencyName":null}""");
    }

    [SqlServerFact]
    public async Task Save_PaddedValues_AreStoredTrimmed()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);
        var before = await service.GetAsync();

        await service.SaveAsync(Request(before, "  CÔNG AN X  ", "  TRẠI MỚI  ", "  Xã Mới  "));

        var after = await GetRowAsync();
        after.ParentAgencyName.ShouldBe("CÔNG AN X");
        after.FacilityName.ShouldBe("TRẠI MỚI");
        after.Address.ShouldBe("Xã Mới");
    }

    [SqlServerFact]
    public async Task Save_TwiceWithTheReturnedRowVer_Succeeds()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);
        var first = await service.GetAsync();
        await service.SaveAsync(Request(first, first.ParentAgencyName, "TRẠI LẦN MỘT", first.Address));

        // The screen reloads after a save; the second save carries the fresh row version.
        var reloaded = await service.GetAsync();
        reloaded.RowVer.ShouldNotBe(first.RowVer);
        await service.SaveAsync(Request(reloaded, reloaded.ParentAgencyName, "TRẠI LẦN HAI", reloaded.Address));

        (await GetRowAsync()).FacilityName.ShouldBe("TRẠI LẦN HAI");
    }

    [SqlServerFact]
    public async Task Save_ViewOnlyUser_IsDeniedAndWritesNothing()
    {
        var user = await CreateUserAsync(PermissionCodes.Administration.View);
        await _fixture.SignInAsync(user);
        var codes = await GetPermissionCodesAsync(user.Id);
        codes.ShouldContain(PermissionCodes.Administration.View);
        codes.ShouldNotContain(PermissionCodes.Administration.Update);

        await using var db = NewContext();
        var before = await GetRowAsync();
        var logsBefore = (await GetFacilityAuditLogsAsync()).Count;

        await Should.ThrowAsync<PermissionDeniedException>(() => CreateService(db).SaveAsync(
            new SaveFacilityInfoRequest(before.ParentAgencyName, "TRẠI TẠM GIAM MỚI", before.Address, before.RowVer)));

        (await GetRowAsync()).FacilityName.ShouldBe(BaselineName);
        (await GetFacilityAuditLogsAsync()).Count.ShouldBe(logsBefore);
    }

    [SqlServerFact]
    public async Task GetAndSave_MissingRow_ThrowInvalidOperationException()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);

        // Delete inside a rollback-only transaction, so the row is missing for this test and restored for the next.
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM FacilityInfo WHERE Id = 1");

        await Should.ThrowAsync<InvalidOperationException>(() => service.GetAsync());
        await Should.ThrowAsync<InvalidOperationException>(
            () => service.SaveAsync(new SaveFacilityInfoRequest(null, BaselineName, BaselineAddress, [1])));

        await transaction.RollbackAsync();
    }

    [SqlServerFact]
    public async Task Save_StaleRowVer_ConflictsAndWritesNothing()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);
        var before = await service.GetAsync();

        // Another workstation changes the row (the same values still bump the row version); the caller's copy is stale.
        await _fixture.Database.ExecuteAsync("UPDATE FacilityInfo SET FacilityName = FacilityName WHERE Id = 1");

        await Should.ThrowAsync<ConcurrencyConflictException>(
            () => service.SaveAsync(Request(before, before.ParentAgencyName, "TRẠI TẠM GIAM MỚI", before.Address)));

        (await GetRowAsync()).FacilityName.ShouldBe(BaselineName);
    }

    [SqlServerFact]
    public async Task DepositReceiptPrintQuery_AfterSave_RendersThePreviewedHeader()
    {
        await SignInAdminAsync();
        await using (var db = NewContext())
        {
            var service = CreateService(db);
            var before = await service.GetAsync();
            await service.SaveAsync(Request(before, "Công an tỉnh ABC", "TRẠI TẠM GIAM MỚI", before.Address));

            var inmate = new Inmate
            {
                InmateCode = "T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                FullName = "Nguyễn Văn A",
                InmateType = InmateType.PreTrialDetainee,
                AdmissionDate = new DateOnly(2026, 9, 1),
            };
            db.Inmate.Add(inmate);
            await db.SaveChangesAsync();

            var voucher = CustodyVoucher.CreatePostedDepositReceipt(
                "BNT-TEST-" + Guid.NewGuid().ToString("N")[..8], new DateOnly(2026, 10, 1), inmate,
                VoucherType.Receipt, TransactionType.SentByRelative, PaymentMethod.Cash,
                "Trần Thị B", "Mẹ", null, null, null, "Tiền gửi lưu ký",
                500_000m, "Năm trăm nghìn đồng", 0m, 500_000m);
            db.CustodyVoucher.Add(voucher);
            await db.SaveChangesAsync();

            var query = new DepositReceiptPrintQuery(new CustodyVoucherStore(db), new FacilityInfoStore(db));
            var model = (await query.GetAsync(voucher.Id)).ShouldNotBeNull();

            model.FacilityName.ShouldBe("TRẠI TẠM GIAM MỚI");
            model.ParentAgencyName.ShouldBe("Công an tỉnh ABC");
            model.Address.ShouldBe(BaselineAddress);

            // The printed header is the previewed one: agency in capitals, then the name, then the address line.
            var pdf = new DepositReceiptTemplate(_fixture.Clock).Render(model);
            using var document = PdfDocument.Open(pdf);
            var text = string.Concat(document.GetPages().Select(p => p.Text));

            text.ShouldContain("CÔNG AN TỈNH ABC");
            text.ShouldContain("TRẠI TẠM GIAM MỚI");
            text.ShouldContain($"Địa chỉ: {BaselineAddress}");
        }
    }
}
