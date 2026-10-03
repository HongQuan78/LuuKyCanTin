using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

public sealed class SignatoryConfigurationServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private static readonly SignatoryLine[] SeededPayout =
    [
        new("Cán bộ theo dõi tiền lưu ký"),
        new("Người bị tạm giữ, tạm giam/phạm nhân xác nhận"),
        new("Cán bộ quản giáo xác nhận"),
        new("Lãnh đạo đơn vị xác nhận"),
    ];

    private readonly AppDatabaseFixture _fixture;
    private readonly List<AppDbContext> _contexts = [];

    public SignatoryConfigurationServiceTests(AppDatabaseFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

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

    private SignatoryConfigurationService CreateService(AppDbContext db) =>
        new(
            db,
            new PermissionChecker(db, _fixture.User),
            new AuditLogWriter(db, new AuditLogFactory(_fixture.Clock, _fixture.User)));

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

    private async Task<int> CreateOfficerAsync(bool isActive)
    {
        await using var db = NewContext();
        var officer = new Officer("T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            "Cán bộ " + Guid.NewGuid().ToString("N")[..6], "Cán bộ", isSupervisingOfficer: false)
        {
            IsActive = isActive,
        };
        db.Officer.Add(officer);
        await db.SaveChangesAsync();
        return officer.Id;
    }

    private async Task<List<AuditLog>> GetSignatoryAuditLogsAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.AuditLog
            .Where(n => n.TableName == "SignatoryConfiguration")
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    /// <summary>Saves a known payout baseline, then asserts the rejected save changed neither rows nor log.</summary>
    private async Task AssertPayoutSaveRejectedAsync(string expectedMessage, SignatoryLine[] lines)
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);
        await service.SaveAsync(TemplateCodes.Payout, SeededPayout);
        var before = await service.GetByTemplateAsync(TemplateCodes.Payout);
        var logsBefore = (await GetSignatoryAuditLogsAsync()).Count;

        var error = await Should.ThrowAsync<BusinessRuleException>(() => service.SaveAsync(TemplateCodes.Payout, lines));

        error.Message.ShouldBe(expectedMessage);
        var after = await service.GetByTemplateAsync(TemplateCodes.Payout);
        after.Select(r => (r.Ordinal, r.Title, r.OfficerId)).ShouldBe(before.Select(r => (r.Ordinal, r.Title, r.OfficerId)));
        (await GetSignatoryAuditLogsAsync()).Count.ShouldBe(logsBefore);
    }

    [SqlServerFact]
    public async Task Save_Reorder_SavesFreshOneToNOrdinalsAndOneUpdateAuditRowWithBeforeAndAfter()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);
        // A known starting point, whatever an earlier test in this class left behind.
        await service.SaveAsync(TemplateCodes.Payout, SeededPayout);
        var logsBefore = (await GetSignatoryAuditLogsAsync()).Count;

        SignatoryLine[] reordered =
        [
            new("Lãnh đạo đơn vị xác nhận"),
            new("Cán bộ quản giáo xác nhận"),
            new("Người bị tạm giữ, tạm giam/phạm nhân xác nhận"),
            new("Cán bộ theo dõi tiền lưu ký"),
        ];
        await service.SaveAsync(TemplateCodes.Payout, reordered);

        var after = await service.GetByTemplateAsync(TemplateCodes.Payout);
        after.Select(r => (int)r.Ordinal).ShouldBe([1, 2, 3, 4]);
        after.Select(r => r.Title).ShouldBe(reordered.Select(line => line.Title));

        var logs = await GetSignatoryAuditLogsAsync();
        logs.Count.ShouldBe(logsBefore + 1);
        logs[^1].Action.ShouldBe(AuditAction.Update);
        logs[^1].TableName.ShouldBe("SignatoryConfiguration");
        logs[^1].RecordId.ShouldBeNull();
        logs[^1].OldValues.ShouldBe(
            """{"TemplateCode":"PHIEU_CHI","Signatories":[{"Ordinal":1,"Title":"Cán bộ theo dõi tiền lưu ký","OfficerId":null,"FullName":null},{"Ordinal":2,"Title":"Người bị tạm giữ, tạm giam/phạm nhân xác nhận","OfficerId":null,"FullName":null},{"Ordinal":3,"Title":"Cán bộ quản giáo xác nhận","OfficerId":null,"FullName":null},{"Ordinal":4,"Title":"Lãnh đạo đơn vị xác nhận","OfficerId":null,"FullName":null}]}""");
        logs[^1].NewValues.ShouldBe(
            """{"TemplateCode":"PHIEU_CHI","Signatories":[{"Ordinal":1,"Title":"Lãnh đạo đơn vị xác nhận","OfficerId":null,"FullName":null},{"Ordinal":2,"Title":"Cán bộ quản giáo xác nhận","OfficerId":null,"FullName":null},{"Ordinal":3,"Title":"Người bị tạm giữ, tạm giam/phạm nhân xác nhận","OfficerId":null,"FullName":null},{"Ordinal":4,"Title":"Cán bộ theo dõi tiền lưu ký","OfficerId":null,"FullName":null}]}""");
    }

    [SqlServerFact]
    public async Task Save_EmptyList_IsRejectedAndLeavesTheRowsUnchanged()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);
        await service.SaveAsync(TemplateCodes.Payout, SeededPayout);
        var before = await service.GetByTemplateAsync(TemplateCodes.Payout);
        var logsBefore = (await GetSignatoryAuditLogsAsync()).Count;

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => service.SaveAsync(TemplateCodes.Payout, []));

        error.Message.ShouldBe(SignatoryConfigurationService.AtLeastOneSignatoryMessage);
        var after = await service.GetByTemplateAsync(TemplateCodes.Payout);
        after.Select(r => (r.Ordinal, r.Title, r.OfficerId)).ShouldBe(before.Select(r => (r.Ordinal, r.Title, r.OfficerId)));
        (await GetSignatoryAuditLogsAsync()).Count.ShouldBe(logsBefore);
    }

    [SqlServerFact]
    public async Task Save_UnknownTemplateCode_IsRejected()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        var service = CreateService(db);

        var error = await Should.ThrowAsync<BusinessRuleException>(
            () => service.SaveAsync("KHONG_CO_MAU_NAY", [new("Người gửi")]));

        error.Message.ShouldBe(SignatoryConfigurationService.UnknownTemplateMessage);
    }

    [SqlServerFact]
    public Task Save_BlankTitle_IsRejectedAndLeavesTheRowsUnchanged() =>
        AssertPayoutSaveRejectedAsync(
            SignatoryConfigurationService.TitleRequiredMessage,
            [new("Cán bộ theo dõi tiền lưu ký"), new("   ")]);

    [SqlServerFact]
    public Task Save_OverLengthTitle_IsRejectedAndLeavesTheRowsUnchanged() =>
        AssertPayoutSaveRejectedAsync(
            SignatoryConfigurationService.TitleMaxLengthMessage,
            [new(new string('a', SignatoryConfigurationService.TitleMaxLength + 1))]);

    [SqlServerFact]
    public Task Save_UnknownOfficer_IsRejectedAndLeavesTheRowsUnchanged() =>
        AssertPayoutSaveRejectedAsync(
            SignatoryConfigurationService.OfficerNotFoundMessage,
            [new("Cán bộ căn tin", 999_999)]);

    [SqlServerFact]
    public Task Save_TooManyRows_IsRejectedAndLeavesTheRowsUnchanged() =>
        AssertPayoutSaveRejectedAsync(
            SignatoryConfigurationService.TooManySignatoriesMessage,
            [.. Enumerable.Range(1, SignatoryConfigurationService.MaxSignatories + 1).Select(i => new SignatoryLine($"Chức danh {i}"))]);

    [SqlServerFact]
    public async Task Database_RejectsATemplateCodeOutsideTheCatalogue()
    {
        var error = await Should.ThrowAsync<SqlException>(() => _fixture.Database.ExecuteAsync(
            "INSERT INTO SignatoryConfiguration (TemplateCode, Ordinal, Title) VALUES ('MAU_THU_13', 1, N'Người ký')"));

        error.Number.ShouldBe(547);
    }

    [SqlServerFact]
    public async Task Database_RejectsADuplicateOrdinalWithinOneTemplate()
    {
        await SignInAdminAsync();
        await using var db = NewContext();
        await CreateService(db).SaveAsync(TemplateCodes.Payout, SeededPayout);

        var error = await Should.ThrowAsync<SqlException>(() => _fixture.Database.ExecuteAsync(
            "INSERT INTO SignatoryConfiguration (TemplateCode, Ordinal, Title) VALUES ('PHIEU_CHI', 1, N'Trùng thứ tự')"));

        new[] { 2601, 2627 }.ShouldContain(error.Number);
    }

    [SqlServerFact]
    public async Task Save_InactiveOfficerAsANewDefault_IsRejected()
    {
        await SignInAdminAsync();
        var retiredOfficerId = await CreateOfficerAsync(isActive: false);
        await using var db = NewContext();
        var service = CreateService(db);
        await service.SaveAsync(TemplateCodes.PriceList, [new("Cán bộ căn tin"), new("Lãnh đạo đơn vị")]);
        var before = await service.GetByTemplateAsync(TemplateCodes.PriceList);

        var error = await Should.ThrowAsync<BusinessRuleException>(() => service.SaveAsync(
            TemplateCodes.PriceList,
            [new("Cán bộ căn tin", retiredOfficerId), new("Lãnh đạo đơn vị")]));

        error.Message.ShouldBe(SignatoryConfigurationService.InactiveOfficerMessage);
        var after = await service.GetByTemplateAsync(TemplateCodes.PriceList);
        after.Select(r => (r.Title, r.OfficerId)).ShouldBe(before.Select(r => (r.Title, r.OfficerId)));
    }

    [SqlServerFact]
    public async Task Save_KeepingAnExistingInactiveDefault_IsAllowedAndKeepsTheName()
    {
        await SignInAdminAsync();
        var officerId = await CreateOfficerAsync(isActive: true);
        await using var db = NewContext();
        var service = CreateService(db);
        SignatoryLine[] lines = [new("Cán bộ căn tin", officerId), new("Lãnh đạo đơn vị")];
        await service.SaveAsync(TemplateCodes.PriceList, lines);

        // The person leaves after having been chosen as the default.
        await using (var updateDb = NewContext())
        {
            var officer = await updateDb.Officer.SingleAsync(o => o.Id == officerId);
            officer.IsActive = false;
            await updateDb.SaveChangesAsync();
        }

        var read = await service.GetByTemplateAsync(TemplateCodes.PriceList);
        read[0].OfficerId.ShouldBe(officerId);
        read[0].OfficerFullName.ShouldNotBeNullOrWhiteSpace();
        read[0].IsOfficerActive.ShouldBeFalse();

        await Should.NotThrowAsync(() => service.SaveAsync(TemplateCodes.PriceList, lines));

        var after = await service.GetByTemplateAsync(TemplateCodes.PriceList);
        after[0].OfficerId.ShouldBe(officerId);
    }

    [SqlServerFact]
    public async Task Save_WithoutAdministrationUpdate_IsRefusedAndWritesNothing()
    {
        var user = await CreateUserAsync(PermissionCodes.Administration.View);
        await _fixture.SignInAsync(user);
        await using var db = NewContext();
        var service = CreateService(db);
        var before = await service.GetByTemplateAsync(TemplateCodes.Payout);
        var logsBefore = (await GetSignatoryAuditLogsAsync()).Count;

        await Should.ThrowAsync<PermissionDeniedException>(
            () => service.SaveAsync(TemplateCodes.Payout, [new("Người gửi")]));

        var after = await service.GetByTemplateAsync(TemplateCodes.Payout);
        after.Select(r => (r.Ordinal, r.Title, r.OfficerId)).ShouldBe(before.Select(r => (r.Ordinal, r.Title, r.OfficerId)));
        (await GetSignatoryAuditLogsAsync()).Count.ShouldBe(logsBefore);
    }
}
