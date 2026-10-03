using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.MasterData;

public sealed class OfficerServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private const int UserId = 3;
    private readonly AppDatabaseFixture _fixture;
    private readonly List<AppDbContext> _contexts = [];

    public OfficerServiceTests(AppDatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.User.SignIn(UserId, "admin", null, "admin");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var context in _contexts)
            await context.DisposeAsync();
    }

    /// <summary>A service on its own context, like one operation in the app.</summary>
    private OfficerService NewService()
    {
        var context = _fixture.CreateAuditedContext();
        _contexts.Add(context);
        return new OfficerService(context, new SaveOfficerRequestValidator());
    }

    // The database is shared by the class, so every test picks codes nobody else uses.
    private static string NewCode() => "T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

    private static SaveOfficerRequest Request(string officerCode, string fullName = "Nguyễn Văn Ánh", bool isSupervisingOfficer = true) =>
        new(officerCode, fullName, "Cán bộ quản giáo", isSupervisingOfficer);

    private static SaveOfficerRequest EditOf(OfficerDto dto) =>
        new(dto.OfficerCode, dto.FullName, dto.Position, dto.IsSupervisingOfficer, dto.IsActive, dto.RowVer);

    private async Task<List<AuditLog>> LogOfAsync(int id)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.AuditLog.Where(n => n.TableName == "Officer" && n.RecordId == id).OrderBy(n => n.Id).ToListAsync();
    }

    [SqlServerFact]
    public async Task Add_IsSaved_AndFoundByNameWithoutDiacritics()
    {
        var code = NewCode();
        var dto = await NewService().AddAsync(Request(code.ToLowerInvariant(), "Nguyễn Văn Ánh"));

        dto.Id.ShouldBeGreaterThan(0);
        dto.OfficerCode.ShouldBe(code);
        dto.RowVer.ShouldNotBeEmpty();
        (await NewService().SearchAsync("nguyen van anh", includeInactive: false)).ShouldContain(c => c.Id == dto.Id);
        (await NewService().SearchAsync("NGUYỄN VĂN", includeInactive: false)).ShouldContain(c => c.Id == dto.Id);
    }

    [SqlServerFact]
    public async Task Search_FoldsEveryVietnameseLetter_IncludingDAndHornedVowels()
    {
        var dto = await NewService().AddAsync(Request(NewCode(), "Đặng Thị Ngọc Ưng Ơn"));

        (await NewService().SearchAsync("dang thi ngoc ung on", includeInactive: false)).ShouldContain(c => c.Id == dto.Id);
    }

    [SqlServerFact]
    public async Task Search_MatchesPartOfTheCode_InAnyCase()
    {
        var code = NewCode();
        var dto = await NewService().AddAsync(Request(code, "Trần Thị Cúc"));

        var result = await NewService().SearchAsync(code[3..9].ToLowerInvariant(), includeInactive: false);

        result.ShouldHaveSingleItem().Id.ShouldBe(dto.Id);
    }

    [SqlServerFact]
    public async Task Search_TreatsLikeWildcardsInTheKeywordAsText()
    {
        await NewService().AddAsync(Request(NewCode(), "Phạm Văn Đức"));

        (await NewService().SearchAsync("%", includeInactive: true)).ShouldBeEmpty();
    }

    [SqlServerFact]
    public async Task Add_DuplicateCode_IsRejected()
    {
        var code = NewCode();
        await NewService().AddAsync(Request(code));

        var error = await Should.ThrowAsync<BusinessRuleException>(() => NewService().AddAsync(Request(code.ToLowerInvariant(), "Người khác")));

        error.Message.ShouldBe("Mã cán bộ đã tồn tại");
        (await NewService().SearchAsync(code, includeInactive: true)).ShouldHaveSingleItem();
    }

    [SqlServerFact]
    public async Task UniqueIndex_IsTranslatedForTheService()
    {
        // Two workstations passing the duplicate check at the same moment: only the unique index stops the second.
        var code = NewCode();
        await using var db = _fixture.CreateAuditedContext();
        db.Officer.AddRange(new Officer(code, "Một", null, false), new Officer(code, "Hai", null, false));

        await Should.ThrowAsync<UniqueConstraintException>(() => ((IAppDbContext)db).SaveChangesAsync());
    }

    [SqlServerFact]
    public async Task StaffWhoLeft_AreHiddenFromSelectionLists_ButStillFound()
    {
        var code = NewCode();
        var dto = await NewService().AddAsync(Request(code, isSupervisingOfficer: true));
        (await NewService().GetActiveOfficersAsync(supervisingOnly: true)).ShouldContain(c => c.Id == dto.Id);

        await NewService().UpdateAsync(dto.Id, EditOf(dto) with { IsActive = false });

        (await NewService().GetActiveOfficersAsync(supervisingOnly: true)).ShouldNotContain(c => c.Id == dto.Id);
        (await NewService().GetActiveOfficersAsync(supervisingOnly: false)).ShouldNotContain(c => c.Id == dto.Id);
        (await NewService().SearchAsync(code, includeInactive: false)).ShouldBeEmpty();
        (await NewService().SearchAsync(code, includeInactive: true)).ShouldHaveSingleItem().IsActive.ShouldBeFalse();
    }

    [SqlServerFact]
    public async Task Add_SomeoneWhoHasAlreadyLeft_IsStoredInactive()
    {
        var code = NewCode();
        await NewService().AddAsync(Request(code) with { IsActive = false });

        (await NewService().SearchAsync(code, includeInactive: true)).ShouldHaveSingleItem().IsActive.ShouldBeFalse();
    }

    [SqlServerFact]
    public async Task SelectionList_ForWardens_LeavesOutOtherStaff()
    {
        var accountant = await NewService().AddAsync(Request(NewCode(), "Kế toán", isSupervisingOfficer: false));

        (await NewService().GetActiveOfficersAsync(supervisingOnly: true)).ShouldNotContain(c => c.Id == accountant.Id);
        (await NewService().GetActiveOfficersAsync(supervisingOnly: false)).ShouldContain(c => c.Id == accountant.Id);
    }

    [SqlServerFact]
    public async Task SecondEditOfTheSameVersion_IsAConflict_AndDoesNotOverwrite()
    {
        var dto = await NewService().AddAsync(Request(NewCode(), "Bản gốc"));
        var firstEdit = EditOf(dto) with { FullName = "Người thứ nhất sửa" };
        var secondEdit = EditOf(dto) with { FullName = "Người thứ hai sửa" };

        await NewService().UpdateAsync(dto.Id, firstEdit);
        var error = await Should.ThrowAsync<ConcurrencyConflictException>(() => NewService().UpdateAsync(dto.Id, secondEdit));

        error.Message.ShouldBe("Dữ liệu đã bị người khác thay đổi, vui lòng tải lại");
        (await NewService().SearchAsync(dto.OfficerCode, includeInactive: true)).ShouldHaveSingleItem().FullName.ShouldBe("Người thứ nhất sửa");
    }

    [SqlServerFact]
    public async Task EditWithTheReturnedVersion_Succeeds()
    {
        var dto = await NewService().AddAsync(Request(NewCode(), "Lần một"));
        var afterFirst = await NewService().UpdateAsync(dto.Id, EditOf(dto) with { FullName = "Lần hai" });

        var afterSecond = await NewService().UpdateAsync(dto.Id, EditOf(afterFirst) with { FullName = "Lần ba" });

        afterSecond.FullName.ShouldBe("Lần ba");
        afterSecond.RowVer.ShouldNotBe(afterFirst.RowVer);
    }

    [SqlServerFact]
    public async Task AddAndEdit_EachWriteOneAuditRow()
    {
        var dto = await NewService().AddAsync(Request(NewCode(), "Hoàng Văn Em"));
        await NewService().UpdateAsync(dto.Id, EditOf(dto) with { Position = "Chỉ huy phụ trách" });

        var log = await LogOfAsync(dto.Id);

        log.Select(n => n.Action).ShouldBe([AuditAction.Create, AuditAction.Update]);
        log.ShouldAllBe(n => n.UserId == UserId);
        log[1].OldValues.ShouldBe("""{"Position":"Cán bộ quản giáo"}""");
        log[1].NewValues.ShouldBe("""{"Position":"Chỉ huy phụ trách"}""");
    }

    [SqlServerFact]
    public async Task Staff_CanNeverBeHardDeleted()
    {
        var dto = await NewService().AddAsync(Request(NewCode()));
        await using var db = _fixture.CreateAuditedContext();
        db.Officer.Remove(await db.Officer.SingleAsync(c => c.Id == dto.Id));

        await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [SqlServerFact]
    public async Task NewStaffMember_IsActive_AndNotAWarden_ByDatabaseDefault()
    {
        var code = NewCode();
        await _fixture.Database.ExecuteAsync($"INSERT INTO Officer (OfficerCode, FullName, CreatedAt, CreatedById) VALUES ('{code}', N'Mặc định', '2026-10-02', 0)");

        (await _fixture.Database.GetScalarAsync($"SELECT CONCAT(IsActive, IsSupervisingOfficer) FROM Officer WHERE OfficerCode = '{code}'")).ShouldBe("10");
    }
}
