using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.DanhMuc;

public sealed class CanBoServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private const int NguoiDungId = 3;
    private readonly AppDatabaseFixture _fixture;
    private readonly List<AppDbContext> _contexts = [];

    public CanBoServiceTests(AppDatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.User.DangNhap(NguoiDungId, "admin");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var context in _contexts)
            await context.DisposeAsync();
    }

    /// <summary>A service on its own context, like one operation in the app.</summary>
    private CanBoService NewService()
    {
        var context = _fixture.CreateAuditedContext();
        _contexts.Add(context);
        return new CanBoService(context, new LuuCanBoRequestValidator());
    }

    // The database is shared by the class, so every test picks codes nobody else uses.
    private static string NewCode() => "T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

    private static LuuCanBoRequest Request(string maCanBo, string hoTen = "Nguyễn Văn Ánh", bool laQuanGiao = true) =>
        new(maCanBo, hoTen, "Cán bộ quản giáo", laQuanGiao);

    private static LuuCanBoRequest EditOf(CanBoDto dto) =>
        new(dto.MaCanBo, dto.HoTen, dto.ChucVu, dto.LaQuanGiao, dto.DangCongTac, dto.RowVer);

    private async Task<List<NhatKyThaoTac>> LogOfAsync(int id)
    {
        await using var db = _fixture.Database.TaoDbContext();
        return await db.NhatKyThaoTac.Where(n => n.TenBang == "CanBo" && n.BanGhiId == id).OrderBy(n => n.Id).ToListAsync();
    }

    [SqlServerFact]
    public async Task Them_IsSaved_AndFoundByNameWithoutDiacritics()
    {
        var ma = NewCode();
        var dto = await NewService().ThemAsync(Request(ma.ToLowerInvariant(), "Nguyễn Văn Ánh"));

        dto.Id.ShouldBeGreaterThan(0);
        dto.MaCanBo.ShouldBe(ma);
        dto.RowVer.ShouldNotBeEmpty();
        (await NewService().TimAsync("nguyen van anh", baoGomNgungCongTac: false)).ShouldContain(c => c.Id == dto.Id);
        (await NewService().TimAsync("NGUYỄN VĂN", baoGomNgungCongTac: false)).ShouldContain(c => c.Id == dto.Id);
    }

    [SqlServerFact]
    public async Task Tim_FoldsEveryVietnameseLetter_IncludingDAndHornedVowels()
    {
        var dto = await NewService().ThemAsync(Request(NewCode(), "Đặng Thị Ngọc Ưng Ơn"));

        (await NewService().TimAsync("dang thi ngoc ung on", baoGomNgungCongTac: false)).ShouldContain(c => c.Id == dto.Id);
    }

    [SqlServerFact]
    public async Task Tim_MatchesPartOfTheCode_InAnyCase()
    {
        var ma = NewCode();
        var dto = await NewService().ThemAsync(Request(ma, "Trần Thị Cúc"));

        var ketQua = await NewService().TimAsync(ma[3..9].ToLowerInvariant(), baoGomNgungCongTac: false);

        ketQua.ShouldHaveSingleItem().Id.ShouldBe(dto.Id);
    }

    [SqlServerFact]
    public async Task Tim_TreatsLikeWildcardsInTheKeywordAsText()
    {
        await NewService().ThemAsync(Request(NewCode(), "Phạm Văn Đức"));

        (await NewService().TimAsync("%", baoGomNgungCongTac: true)).ShouldBeEmpty();
    }

    [SqlServerFact]
    public async Task Them_DuplicateCode_IsRejected()
    {
        var ma = NewCode();
        await NewService().ThemAsync(Request(ma));

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(() => NewService().ThemAsync(Request(ma.ToLowerInvariant(), "Người khác")));

        loi.Message.ShouldBe("Mã cán bộ đã tồn tại");
        (await NewService().TimAsync(ma, baoGomNgungCongTac: true)).ShouldHaveSingleItem();
    }

    [SqlServerFact]
    public async Task UniqueIndex_IsTranslatedForTheService()
    {
        // Two workstations passing the duplicate check at the same moment: only the unique index stops the second.
        var ma = NewCode();
        await using var db = _fixture.CreateAuditedContext();
        db.CanBo.AddRange(new CanBo(ma, "Một", null, false), new CanBo(ma, "Hai", null, false));

        await Should.ThrowAsync<TrungGiaTriDuyNhatException>(() => ((IAppDbContext)db).SaveChangesAsync());
    }

    [SqlServerFact]
    public async Task StaffWhoLeft_AreHiddenFromSelectionLists_ButStillFound()
    {
        var ma = NewCode();
        var dto = await NewService().ThemAsync(Request(ma, laQuanGiao: true));
        (await NewService().LayCanBoDangCongTacAsync(chiQuanGiao: true)).ShouldContain(c => c.Id == dto.Id);

        await NewService().SuaAsync(dto.Id, EditOf(dto) with { DangCongTac = false });

        (await NewService().LayCanBoDangCongTacAsync(chiQuanGiao: true)).ShouldNotContain(c => c.Id == dto.Id);
        (await NewService().LayCanBoDangCongTacAsync(chiQuanGiao: false)).ShouldNotContain(c => c.Id == dto.Id);
        (await NewService().TimAsync(ma, baoGomNgungCongTac: false)).ShouldBeEmpty();
        (await NewService().TimAsync(ma, baoGomNgungCongTac: true)).ShouldHaveSingleItem().DangCongTac.ShouldBeFalse();
    }

    [SqlServerFact]
    public async Task Them_SomeoneWhoHasAlreadyLeft_IsStoredInactive()
    {
        var ma = NewCode();
        await NewService().ThemAsync(Request(ma) with { DangCongTac = false });

        (await NewService().TimAsync(ma, baoGomNgungCongTac: true)).ShouldHaveSingleItem().DangCongTac.ShouldBeFalse();
    }

    [SqlServerFact]
    public async Task SelectionList_ForWardens_LeavesOutOtherStaff()
    {
        var keToan = await NewService().ThemAsync(Request(NewCode(), "Kế toán", laQuanGiao: false));

        (await NewService().LayCanBoDangCongTacAsync(chiQuanGiao: true)).ShouldNotContain(c => c.Id == keToan.Id);
        (await NewService().LayCanBoDangCongTacAsync(chiQuanGiao: false)).ShouldContain(c => c.Id == keToan.Id);
    }

    [SqlServerFact]
    public async Task SecondEditOfTheSameVersion_IsAConflict_AndDoesNotOverwrite()
    {
        var dto = await NewService().ThemAsync(Request(NewCode(), "Bản gốc"));
        var nguoiThuNhat = EditOf(dto) with { HoTen = "Người thứ nhất sửa" };
        var nguoiThuHai = EditOf(dto) with { HoTen = "Người thứ hai sửa" };

        await NewService().SuaAsync(dto.Id, nguoiThuNhat);
        var loi = await Should.ThrowAsync<XungDotDuLieuException>(() => NewService().SuaAsync(dto.Id, nguoiThuHai));

        loi.Message.ShouldBe("Dữ liệu đã bị người khác thay đổi, vui lòng tải lại");
        (await NewService().TimAsync(dto.MaCanBo, baoGomNgungCongTac: true)).ShouldHaveSingleItem().HoTen.ShouldBe("Người thứ nhất sửa");
    }

    [SqlServerFact]
    public async Task EditWithTheReturnedVersion_Succeeds()
    {
        var dto = await NewService().ThemAsync(Request(NewCode(), "Lần một"));
        var sauLanMot = await NewService().SuaAsync(dto.Id, EditOf(dto) with { HoTen = "Lần hai" });

        var sauLanHai = await NewService().SuaAsync(dto.Id, EditOf(sauLanMot) with { HoTen = "Lần ba" });

        sauLanHai.HoTen.ShouldBe("Lần ba");
        sauLanHai.RowVer.ShouldNotBe(sauLanMot.RowVer);
    }

    [SqlServerFact]
    public async Task AddAndEdit_EachWriteOneAuditRow()
    {
        var dto = await NewService().ThemAsync(Request(NewCode(), "Hoàng Văn Em"));
        await NewService().SuaAsync(dto.Id, EditOf(dto) with { ChucVu = "Chỉ huy phụ trách" });

        var log = await LogOfAsync(dto.Id);

        log.Select(n => n.HanhDong).ShouldBe([HanhDong.Them, HanhDong.Sua]);
        log.ShouldAllBe(n => n.NguoiDungId == NguoiDungId);
        log[1].DuLieuCu.ShouldBe("""{"ChucVu":"Cán bộ quản giáo"}""");
        log[1].DuLieuMoi.ShouldBe("""{"ChucVu":"Chỉ huy phụ trách"}""");
    }

    [SqlServerFact]
    public async Task Staff_CanNeverBeHardDeleted()
    {
        var dto = await NewService().ThemAsync(Request(NewCode()));
        await using var db = _fixture.CreateAuditedContext();
        db.CanBo.Remove(await db.CanBo.SingleAsync(c => c.Id == dto.Id));

        await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [SqlServerFact]
    public async Task NewStaffMember_IsActive_AndNotAWarden_ByDatabaseDefault()
    {
        var ma = NewCode();
        await _fixture.Database.ThucThiAsync($"INSERT INTO CanBo (MaCanBo, HoTen, NgayTao, NguoiTaoId) VALUES ('{ma}', N'Mặc định', '2026-10-02', 0)");

        (await _fixture.Database.LayGiaTriAsync($"SELECT CONCAT(DangCongTac, LaQuanGiao) FROM CanBo WHERE MaCanBo = '{ma}'")).ShouldBe("10");
    }
}
