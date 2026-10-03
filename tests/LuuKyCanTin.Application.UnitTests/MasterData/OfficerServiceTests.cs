using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.MasterData;

public sealed class OfficerServiceTests : IDisposable
{
    private readonly InMemoryAppDbContext _db = new();
    private readonly IPermissionChecker _checker = Substitute.For<IPermissionChecker>();
    private readonly OfficerService _service;

    public OfficerServiceTests()
    {
        _service = new OfficerService(_db, _checker, new SaveOfficerRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    private static SaveOfficerRequest Request(string officerCode = "CB01", string fullName = "Nguyễn Văn An") =>
        new(officerCode, fullName, "Cán bộ quản giáo", IsSupervisingOfficer: true, RowVer: []);

    private async Task<Officer> SeedAsync(string officerCode, string fullName = "Lê Thị Bình")
    {
        var officer = new Officer(officerCode, fullName, null, isSupervisingOfficer: false);
        _db.Officer.Add(officer);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
        return officer;
    }

    private async Task<int> CountAsync() => await _db.Officer.CountAsync();

    [Fact]
    public async Task Add_SavesNormalizedStaffMember()
    {
        var dto = await _service.AddAsync(Request(" cb01 ", " Nguyễn Văn An "));

        dto.OfficerCode.ShouldBe("CB01");
        dto.FullName.ShouldBe("Nguyễn Văn An");
        dto.Position.ShouldBe("Cán bộ quản giáo");
        dto.IsSupervisingOfficer.ShouldBeTrue();
        dto.IsActive.ShouldBeTrue();
        (await _db.Officer.SingleAsync()).Id.ShouldBe(dto.Id);
    }

    [Fact]
    public async Task Add_WithoutPermission_IsRefusedBeforeAnythingIsSaved()
    {
        _checker.RequireAsync(PermissionCodes.MasterData.Create, Arg.Any<CancellationToken>())
            .ThrowsAsync(new PermissionDeniedException(PermissionCodes.MasterData.Create));

        await Should.ThrowAsync<PermissionDeniedException>(() => _service.AddAsync(Request()));

        _db.SaveCount.ShouldBe(0);
        (await CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Update_WithoutPermission_IsRefusedBeforeAnythingIsSaved()
    {
        var officer = await SeedAsync("CB01");
        _checker.RequireAsync(PermissionCodes.MasterData.Update, Arg.Any<CancellationToken>())
            .ThrowsAsync(new PermissionDeniedException(PermissionCodes.MasterData.Update));

        await Should.ThrowAsync<PermissionDeniedException>(
            () => _service.UpdateAsync(officer.Id, Request("CB01", "Tên mới")));

        _db.SaveCount.ShouldBe(0);
        (await _db.Officer.AsNoTracking().SingleAsync()).FullName.ShouldBe("Lê Thị Bình");
    }

    [Fact]
    public async Task Add_DuplicateCode_IsRejected_AndNothingIsSaved()
    {
        await SeedAsync("CB01");

        var error = await Should.ThrowAsync<BusinessRuleException>(() => _service.AddAsync(Request(" cb01 ")));

        error.Message.ShouldBe("Mã cán bộ đã tồn tại");
        _db.SaveCount.ShouldBe(0);
        (await CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Add_InvalidRequest_IsRejectedWithEveryMessage_AndNothingIsSaved()
    {
        var error = await Should.ThrowAsync<BusinessRuleException>(() => _service.AddAsync(Request("C B", " ")));

        error.Message.ShouldBe("Mã cán bộ chỉ gồm chữ không dấu, chữ số, dấu '-' hoặc '_'.\nHọ tên không được để trống.");
        _db.SaveCount.ShouldBe(0);
        (await CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Add_InvalidRequest_NamesThePropertyOfEachMessage()
    {
        var error = await Should.ThrowAsync<RequestValidationException>(() => _service.AddAsync(Request("C B", " ")));

        error.Errors.Select(e => e.PropertyName).ShouldBe(
            [nameof(SaveOfficerRequest.OfficerCode), nameof(SaveOfficerRequest.FullName)]);
    }

    [Fact]
    public async Task Add_UniqueIndexViolation_IsReportedAsDuplicateCode()
    {
        // Another workstation saved the same code between the check and the save.
        _db.SaveFailure = new UniqueConstraintException(new InvalidOperationException());

        var error = await Should.ThrowAsync<BusinessRuleException>(() => _service.AddAsync(Request()));

        error.Message.ShouldBe("Mã cán bộ đã tồn tại");
    }

    [Fact]
    public async Task Update_KeepingItsOwnCode_Saves()
    {
        var officer = await SeedAsync("CB01");

        var dto = await _service.UpdateAsync(officer.Id, Request("CB01", "Lê Thị Bình Minh") with { IsActive = false });

        dto.FullName.ShouldBe("Lê Thị Bình Minh");
        dto.IsActive.ShouldBeFalse();
        var stored = await _db.Officer.AsNoTracking().SingleAsync();
        stored.FullName.ShouldBe("Lê Thị Bình Minh");
        stored.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Update_ToAnotherStaffMembersCode_IsRejected()
    {
        await SeedAsync("CB01");
        var officer = await SeedAsync("CB02");

        var error = await Should.ThrowAsync<BusinessRuleException>(() => _service.UpdateAsync(officer.Id, Request("cb01")));

        error.Message.ShouldBe("Mã cán bộ đã tồn tại");
        _db.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Update_UnknownStaffMember_IsRejected()
    {
        var error = await Should.ThrowAsync<BusinessRuleException>(() => _service.UpdateAsync(999, Request()));

        error.Message.ShouldBe("Không tìm thấy cán bộ.");
    }

    [Fact]
    public async Task Update_WithoutRowVersion_IsAProgrammingError()
    {
        var officer = await SeedAsync("CB01");

        await Should.ThrowAsync<ArgumentException>(() => _service.UpdateAsync(officer.Id, Request() with { RowVer = null }));
    }

    [Fact]
    public async Task Update_ConcurrencyConflict_PassesThroughAsBusinessError()
    {
        var officer = await SeedAsync("CB01");
        _db.SaveFailure = new ConcurrencyConflictException(new InvalidOperationException());

        var error = await Should.ThrowAsync<ConcurrencyConflictException>(() => _service.UpdateAsync(officer.Id, Request("CB01", "Tên mới")));

        error.Message.ShouldBe("Dữ liệu đã bị người khác thay đổi, vui lòng tải lại");
    }

    [Fact]
    public async Task Search_HidesInactiveStaffUnlessAskedFor()
    {
        await SeedAsync("CB01", "Đang làm");
        var formerOfficer = await SeedAsync("CB02", "Đã nghỉ");
        await _service.UpdateAsync(formerOfficer.Id, new SaveOfficerRequest("CB02", "Đã nghỉ", null, false, IsActive: false, RowVer: []));

        (await _service.SearchAsync(null, includeInactive: false)).Select(c => c.OfficerCode).ShouldBe(["CB01"]);
        (await _service.SearchAsync(null, includeInactive: true)).Select(c => c.OfficerCode).ShouldBe(["CB02", "CB01"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetActiveOfficers_ReturnsActiveStaff_OptionallyWardensOnly()
    {
        _db.Officer.AddRange(
            new Officer("QG1", "Quản giáo đang làm", null, isSupervisingOfficer: true),
            new Officer("QG2", "Quản giáo đã nghỉ", null, isSupervisingOfficer: true) { IsActive = false },
            new Officer("KT1", "Kế toán", null, isSupervisingOfficer: false));
        await _db.SaveChangesAsync();

        (await _service.GetActiveOfficersAsync(supervisingOnly: true)).Select(c => c.OfficerCode).ShouldBe(["QG1"]);
        (await _service.GetActiveOfficersAsync(supervisingOnly: false)).Select(c => c.OfficerCode).ShouldBe(["KT1", "QG1"], ignoreOrder: true);
    }
}
