using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.HeThong;

public sealed class VaiTroServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private readonly AppDatabaseFixture _fixture;
    private readonly List<AppDbContext> _contexts = [];

    public VaiTroServiceTests(AppDatabaseFixture fixture) => _fixture = fixture;

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

    private VaiTroService TaoService(AppDbContext db)
    {
        var ghiNhatKy = new GhiNhatKy(db, new NhatKyFactory(_fixture.Clock, _fixture.User));
        return new VaiTroService(
            db, new KiemTraQuyen(db, _fixture.User), ghiNhatKy, new KiemTraConQuanTri(db));
    }

    private IKiemTraQuyen TaoKiemTraQuyen(AppDbContext db) => new KiemTraQuyen(db, _fixture.User);

    private async Task NguoiDungDangNhapAsync(string tenDangNhap)
    {
        await using var db = _fixture.Database.CreateDbContext();
        var nguoiDung = await db.NguoiDung.SingleAsync(u => u.TenDangNhap == tenDangNhap);
        _fixture.User.DangNhap(nguoiDung.Id, nguoiDung.TenDangNhap, nguoiDung.CanBoId, nguoiDung.TenDangNhap);
    }

    private async Task<NguoiDung> TaoNguoiDungAsync(string? vaiTroMa)
    {
        await using var db = NewContext();
        // Every account except the built-in admin must link to a staff member (CK_NguoiDung_CanBoId).
        var canBo = new CanBo(
            "T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            "Cán bộ " + Guid.NewGuid().ToString("N")[..6],
            "Cán bộ",
            laQuanGiao: false);
        db.CanBo.Add(canBo);
        await db.SaveChangesAsync();

        var nguoiDung = new NguoiDung
        {
            TenDangNhap = "u" + Guid.NewGuid().ToString("N")[..12],
            CanBoId = canBo.Id,
            MatKhauHash = "PBKDF2-SHA256$1$abc$def",
            DangHoatDong = true,
        };
        db.NguoiDung.Add(nguoiDung);
        await db.SaveChangesAsync();

        if (vaiTroMa is not null)
        {
            var vaiTroId = await db.VaiTro.Where(v => v.Ma == vaiTroMa).Select(v => v.Id).SingleAsync();
            db.NguoiDungVaiTro.Add(new NguoiDungVaiTro { NguoiDungId = nguoiDung.Id, VaiTroId = vaiTroId });
            await db.SaveChangesAsync();
        }

        return nguoiDung;
    }

    private async Task<IReadOnlyList<string>> QuyenCuaVaiTroAsync(string maVaiTro)
    {
        await using var db = NewContext();
        var vaiTroId = await db.VaiTro.Where(v => v.Ma == maVaiTro).Select(v => v.Id).SingleAsync();
        return await TaoService(db).LayQuyenCuaVaiTroAsync(vaiTroId);
    }

    private async Task<List<NhatKyThaoTac>> NhatKyCuaVaiTroAsync(int vaiTroId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.NhatKyThaoTac
            .Where(n => n.TenBang == "VaiTro" && n.BanGhiId == vaiTroId)
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    [SqlServerFact]
    public async Task Quyen_Seed_KhopHoanToanVoiDanhMuc()
    {
        await using var db = _fixture.Database.CreateDbContext();
        var trongDb = await db.Quyen.AsNoTracking().OrderBy(q => q.Id).ToListAsync();

        trongDb.Count.ShouldBe(MaQuyen.TatCa.Count);
        trongDb.Select(q => (q.Id, q.Ma, q.Ten, q.Module))
            .ShouldBe(MaQuyen.TatCa.Select(q => (q.Id, q.Ma, q.Ten, q.Module)));
    }

    [SqlServerFact]
    public async Task VaiTro_Seed_DuSauVaiTroVoiMaChuan()
    {
        await using var db = _fixture.Database.CreateDbContext();
        var ma = await db.VaiTro.AsNoTracking().OrderBy(v => v.Id).Select(v => v.Ma).ToListAsync();

        ma.ShouldBe(
        [
            MaVaiTro.QuanTri,
            MaVaiTro.LuuKy,
            MaVaiTro.CanTin,
            MaVaiTro.QuanGiao,
            MaVaiTro.LanhDao,
            MaVaiTro.KeToan,
        ]);
    }

    [SqlServerFact]
    public async Task VaiTroQuyen_Seed_DungQuyenMacDinh()
    {
        var quanTri = await QuyenCuaVaiTroAsync(MaVaiTro.QuanTri);
        quanTri.Count.ShouldBe(12);
        quanTri.ShouldContain(MaQuyen.HT.Sua);
        quanTri.ShouldContain(MaQuyen.DM.Them);
        quanTri.ShouldNotContain(MaQuyen.LKT.Xem);

        var luuKy = await QuyenCuaVaiTroAsync(MaVaiTro.LuuKy);
        luuKy.Count.ShouldBe(16);
        luuKy.ShouldContain(MaQuyen.DM.Xem);
        luuKy.ShouldNotContain(MaQuyen.LKT.Duyet);
        luuKy.ShouldNotContain(MaQuyen.LKC.Duyet);
        luuKy.ShouldNotContain(MaQuyen.LKBC.Duyet);

        var canTin = await QuyenCuaVaiTroAsync(MaVaiTro.CanTin);
        canTin.Count.ShouldBe(19);
        canTin.ShouldContain(MaQuyen.NH.Duyet);
        canTin.ShouldContain(MaQuyen.BH.In);
        canTin.ShouldContain(MaQuyen.LKBC.Xem);

        (await QuyenCuaVaiTroAsync(MaVaiTro.QuanGiao)).ShouldBe([MaQuyen.LKBC.Xem]);

        var lanhDao = await QuyenCuaVaiTroAsync(MaVaiTro.LanhDao);
        lanhDao.Count.ShouldBe(6);
        lanhDao.ShouldBe(
        [
            MaQuyen.HT.Xem,
            MaQuyen.LKT.Duyet,
            MaQuyen.LKC.Duyet,
            MaQuyen.NH.Duyet,
            MaQuyen.LKBC.Xem,
            MaQuyen.HHBC.Xem,
        ], ignoreOrder: true);

        (await QuyenCuaVaiTroAsync(MaVaiTro.KeToan)).ShouldBe([MaQuyen.LKBC.Xem, MaQuyen.HHBC.Xem]);
    }

    [SqlServerFact]
    public async Task NguoiDungVaiTro_AdminDuocGanQuanTri()
    {
        await using var db = _fixture.Database.CreateDbContext();
        var adminId = await db.NguoiDung.Where(u => u.TenDangNhap == "admin").Select(u => u.Id).SingleAsync();
        var maVaiTro = await (from nguoiDungVaiTro in db.NguoiDungVaiTro
                              join vaiTro in db.VaiTro on nguoiDungVaiTro.VaiTroId equals vaiTro.Id
                              where nguoiDungVaiTro.NguoiDungId == adminId
                              select vaiTro.Ma).ToListAsync();

        maVaiTro.ShouldBe([MaVaiTro.QuanTri]);
    }

    [SqlServerFact]
    public async Task CapNhatQuyen_LuuThayDoiVaLuuMotDongSuaVoiHaiDanhSachDaSapXep()
    {
        await NguoiDungDangNhapAsync("admin");
        await using var db = NewContext();
        var service = TaoService(db);
        var vaiTro = (await service.LayDanhSachAsync()).Single(v => v.Ma == MaVaiTro.QuanGiao);

        await service.CapNhatQuyenAsync(vaiTro.Id, [MaQuyen.LKBC.In, MaQuyen.LKBC.Xem], vaiTro.RowVer);

        (await service.LayQuyenCuaVaiTroAsync(vaiTro.Id)).ShouldBe([MaQuyen.LKBC.In, MaQuyen.LKBC.Xem], ignoreOrder: true);
        var log = await NhatKyCuaVaiTroAsync(vaiTro.Id);
        log.Count.ShouldBe(1);
        log[0].HanhDong.ShouldBe(HanhDong.Sua);
        log[0].DuLieuCu.ShouldBe("""{"Quyen":["LK-BC.Xem"]}""");
        log[0].DuLieuMoi.ShouldBe("""{"Quyen":["LK-BC.In","LK-BC.Xem"]}""");

        // Put the seeded grant back for the other tests in this class.
        var sau = (await service.LayDanhSachAsync()).Single(v => v.Ma == MaVaiTro.QuanGiao);
        await service.CapNhatQuyenAsync(sau.Id, [MaQuyen.LKBC.Xem], sau.RowVer);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task CapNhatQuyen_RowVerCu_XungDot()
    {
        await NguoiDungDangNhapAsync("admin");
        await using var db = NewContext();
        var service = TaoService(db);
        var vaiTro = (await service.LayDanhSachAsync()).Single(v => v.Ma == MaVaiTro.CanTin);
        var rowVerCu = vaiTro.RowVer;

        // A first save touches the role; the row version the caller still holds is now stale.
        await service.CapNhatQuyenAsync(vaiTro.Id, await QuyenCuaVaiTroAsync(MaVaiTro.CanTin), rowVerCu);

        await Should.ThrowAsync<XungDotDuLieuException>(
            () => service.CapNhatQuyenAsync(vaiTro.Id, [MaQuyen.LKBC.In, MaQuyen.LKBC.Xem], rowVerCu));

        // The stale call changed nothing.
        (await QuyenCuaVaiTroAsync(MaVaiTro.CanTin)).ShouldNotContain(MaQuyen.LKBC.In);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task CapNhatQuyen_KhongCoQuyen_KhongGhiGiCa()
    {
        var nguoiDung = await TaoNguoiDungAsync(vaiTroMa: null);
        _fixture.User.DangNhap(nguoiDung.Id, nguoiDung.TenDangNhap, nguoiDung.CanBoId, nguoiDung.TenDangNhap);

        await using var db = NewContext();
        var service = TaoService(db);
        var vaiTro = (await service.LayDanhSachAsync()).Single(v => v.Ma == MaVaiTro.KeToan);
        var logTruoc = await NhatKyCuaVaiTroAsync(vaiTro.Id);

        await Should.ThrowAsync<KhongCoQuyenException>(
            () => service.CapNhatQuyenAsync(vaiTro.Id, [MaQuyen.HT.Sua], vaiTro.RowVer));

        (await service.LayQuyenCuaVaiTroAsync(vaiTro.Id)).ShouldBe([MaQuyen.LKBC.Xem, MaQuyen.HHBC.Xem]);
        (await NhatKyCuaVaiTroAsync(vaiTro.Id)).Count.ShouldBe(logTruoc.Count);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task CapNhatQuyen_MaKhongCoTrongDanhMuc_BiTuChoi()
    {
        await NguoiDungDangNhapAsync("admin");
        await using var db = NewContext();
        var service = TaoService(db);
        var vaiTro = (await service.LayDanhSachAsync()).Single(v => v.Ma == MaVaiTro.KeToan);

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => service.CapNhatQuyenAsync(vaiTro.Id, ["HT.KhongCoThat"], vaiTro.RowVer));

        loi.Message.ShouldContain("HT.KhongCoThat");
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task KiemTraQuyen_AdminCoQuyen_ChoPhep()
    {
        await NguoiDungDangNhapAsync("admin");
        await using var db = NewContext();

        await Should.NotThrowAsync(() => TaoKiemTraQuyen(db).YeuCauAsync(MaQuyen.HT.Sua));
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task KiemTraQuyen_KhongCoVaiTro_BiTuChoi()
    {
        var nguoiDung = await TaoNguoiDungAsync(vaiTroMa: null);
        _fixture.User.DangNhap(nguoiDung.Id, nguoiDung.TenDangNhap, nguoiDung.CanBoId, nguoiDung.TenDangNhap);
        await using var db = NewContext();

        await Should.ThrowAsync<KhongCoQuyenException>(() => TaoKiemTraQuyen(db).YeuCauAsync(MaQuyen.HT.Sua));
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task KiemTraQuyen_TaiKhoanNgungHoatDong_BiTuChoi()
    {
        var nguoiDung = await TaoNguoiDungAsync(MaVaiTro.QuanTri);
        await using (var db = NewContext())
        {
            var tracked = await db.NguoiDung.SingleAsync(u => u.Id == nguoiDung.Id);
            tracked.DangHoatDong = false;
            await db.SaveChangesAsync();
        }

        _fixture.User.DangNhap(nguoiDung.Id, nguoiDung.TenDangNhap, nguoiDung.CanBoId, nguoiDung.TenDangNhap);
        await using var checkDb = NewContext();

        await Should.ThrowAsync<KhongCoQuyenException>(() => TaoKiemTraQuyen(checkDb).YeuCauAsync(MaQuyen.HT.Sua));
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task KiemTraQuyen_ThuHoiVaiTro_MatQuyenNgay()
    {
        var nguoiDung = await TaoNguoiDungAsync(MaVaiTro.QuanTri);
        _fixture.User.DangNhap(nguoiDung.Id, nguoiDung.TenDangNhap, nguoiDung.CanBoId, nguoiDung.TenDangNhap);
        await using (var db = NewContext())
            await Should.NotThrowAsync(() => TaoKiemTraQuyen(db).YeuCauAsync(MaQuyen.HT.Sua));

        // The check reads the database every time: removing the role takes effect at the next call.
        await using (var db = NewContext())
            await db.NguoiDungVaiTro.Where(v => v.NguoiDungId == nguoiDung.Id).ExecuteDeleteAsync();

        await using (var db = NewContext())
            await Should.ThrowAsync<KhongCoQuyenException>(() => TaoKiemTraQuyen(db).YeuCauAsync(MaQuyen.HT.Sua));

        _fixture.User.DangXuat();
    }
}
