using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.DanhMuc;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.DanhMuc;

public sealed class CanBoServiceTests : IDisposable
{
    private readonly InMemoryAppDbContext _db = new();
    private readonly CanBoService _service;

    public CanBoServiceTests()
    {
        _service = new CanBoService(_db, new LuuCanBoRequestValidator());
    }

    public void Dispose() => _db.Dispose();

    private static LuuCanBoRequest Request(string maCanBo = "CB01", string hoTen = "Nguyễn Văn An") =>
        new(maCanBo, hoTen, "Cán bộ quản giáo", LaQuanGiao: true, RowVer: []);

    private async Task<CanBo> SeedAsync(string maCanBo, string hoTen = "Lê Thị Bình")
    {
        var canBo = new CanBo(maCanBo, hoTen, null, laQuanGiao: false);
        _db.CanBo.Add(canBo);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
        return canBo;
    }

    private async Task<int> CountAsync() => await _db.CanBo.CountAsync();

    [Fact]
    public async Task Them_SavesNormalizedStaffMember()
    {
        var dto = await _service.ThemAsync(Request(" cb01 ", " Nguyễn Văn An "));

        dto.MaCanBo.ShouldBe("CB01");
        dto.HoTen.ShouldBe("Nguyễn Văn An");
        dto.ChucVu.ShouldBe("Cán bộ quản giáo");
        dto.LaQuanGiao.ShouldBeTrue();
        dto.DangCongTac.ShouldBeTrue();
        (await _db.CanBo.SingleAsync()).Id.ShouldBe(dto.Id);
    }

    [Fact]
    public async Task Them_DuplicateCode_IsRejected_AndNothingIsSaved()
    {
        await SeedAsync("CB01");

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(() => _service.ThemAsync(Request(" cb01 ")));

        loi.Message.ShouldBe("Mã cán bộ đã tồn tại");
        _db.SoLanLuu.ShouldBe(0);
        (await CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Them_InvalidRequest_IsRejectedWithEveryMessage_AndNothingIsSaved()
    {
        var loi = await Should.ThrowAsync<LoiNghiepVuException>(() => _service.ThemAsync(Request("C B", " ")));

        loi.Message.ShouldBe("Mã cán bộ chỉ gồm chữ không dấu, chữ số, dấu '-' hoặc '_'.\nHọ tên không được để trống.");
        _db.SoLanLuu.ShouldBe(0);
        (await CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Them_UniqueIndexViolation_IsReportedAsDuplicateCode()
    {
        // Another workstation saved the same code between the check and the save.
        _db.LoiKhiLuu = new TrungGiaTriDuyNhatException(new InvalidOperationException());

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(() => _service.ThemAsync(Request()));

        loi.Message.ShouldBe("Mã cán bộ đã tồn tại");
    }

    [Fact]
    public async Task Sua_KeepingItsOwnCode_Saves()
    {
        var canBo = await SeedAsync("CB01");

        var dto = await _service.SuaAsync(canBo.Id, Request("CB01", "Lê Thị Bình Minh") with { DangCongTac = false });

        dto.HoTen.ShouldBe("Lê Thị Bình Minh");
        dto.DangCongTac.ShouldBeFalse();
        var stored = await _db.CanBo.AsNoTracking().SingleAsync();
        stored.HoTen.ShouldBe("Lê Thị Bình Minh");
        stored.DangCongTac.ShouldBeFalse();
    }

    [Fact]
    public async Task Sua_ToAnotherStaffMembersCode_IsRejected()
    {
        await SeedAsync("CB01");
        var canBo = await SeedAsync("CB02");

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(() => _service.SuaAsync(canBo.Id, Request("cb01")));

        loi.Message.ShouldBe("Mã cán bộ đã tồn tại");
        _db.SoLanLuu.ShouldBe(0);
    }

    [Fact]
    public async Task Sua_UnknownStaffMember_IsRejected()
    {
        var loi = await Should.ThrowAsync<LoiNghiepVuException>(() => _service.SuaAsync(999, Request()));

        loi.Message.ShouldBe("Không tìm thấy cán bộ.");
    }

    [Fact]
    public async Task Sua_WithoutRowVersion_IsAProgrammingError()
    {
        var canBo = await SeedAsync("CB01");

        await Should.ThrowAsync<ArgumentException>(() => _service.SuaAsync(canBo.Id, Request() with { RowVer = null }));
    }

    [Fact]
    public async Task Sua_ConcurrencyConflict_PassesThroughAsBusinessError()
    {
        var canBo = await SeedAsync("CB01");
        _db.LoiKhiLuu = new XungDotDuLieuException(new InvalidOperationException());

        var loi = await Should.ThrowAsync<XungDotDuLieuException>(() => _service.SuaAsync(canBo.Id, Request("CB01", "Tên mới")));

        loi.Message.ShouldBe("Dữ liệu đã bị người khác thay đổi, vui lòng tải lại");
    }

    [Fact]
    public async Task Tim_HidesInactiveStaffUnlessAskedFor()
    {
        await SeedAsync("CB01", "Đang làm");
        var daNghi = await SeedAsync("CB02", "Đã nghỉ");
        await _service.SuaAsync(daNghi.Id, new LuuCanBoRequest("CB02", "Đã nghỉ", null, false, DangCongTac: false, RowVer: []));

        (await _service.TimAsync(null, baoGomNgungCongTac: false)).Select(c => c.MaCanBo).ShouldBe(["CB01"]);
        (await _service.TimAsync(null, baoGomNgungCongTac: true)).Select(c => c.MaCanBo).ShouldBe(["CB02", "CB01"], ignoreOrder: true);
    }

    [Fact]
    public async Task LayCanBoDangCongTac_ReturnsActiveStaff_OptionallyWardensOnly()
    {
        _db.CanBo.AddRange(
            new CanBo("QG1", "Quản giáo đang làm", null, laQuanGiao: true),
            new CanBo("QG2", "Quản giáo đã nghỉ", null, laQuanGiao: true) { DangCongTac = false },
            new CanBo("KT1", "Kế toán", null, laQuanGiao: false));
        await _db.SaveChangesAsync();

        (await _service.LayCanBoDangCongTacAsync(chiQuanGiao: true)).Select(c => c.MaCanBo).ShouldBe(["QG1"]);
        (await _service.LayCanBoDangCongTacAsync(chiQuanGiao: false)).Select(c => c.MaCanBo).ShouldBe(["KT1", "QG1"], ignoreOrder: true);
    }
}
