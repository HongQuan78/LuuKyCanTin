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

public sealed class TaiKhoanServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private readonly AppDatabaseFixture _fixture;
    private readonly IMatKhauHasher _hasher = new Pbkdf2MatKhauHasher();
    private readonly List<AppDbContext> _contexts = [];

    public TaiKhoanServiceTests(AppDatabaseFixture fixture) => _fixture = fixture;

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

    private TaiKhoanService TaoService(AppDbContext db) =>
        new(
            db,
            new KiemTraQuyen(db, _fixture.User),
            new GhiNhatKy(db, new NhatKyFactory(_fixture.Clock, _fixture.User)),
            _hasher,
            _fixture.Clock,
            _fixture.User,
            new TaoTaiKhoanRequestValidator(),
            new KiemTraConQuanTri(db));

    private async Task DangNhapAsync(string tenDangNhap)
    {
        await using var db = _fixture.Database.CreateDbContext();
        var nguoiDung = await db.NguoiDung.SingleAsync(u => u.TenDangNhap == tenDangNhap);
        _fixture.User.DangNhap(nguoiDung.Id, nguoiDung.TenDangNhap, nguoiDung.CanBoId, nguoiDung.TenDangNhap);
    }

    private static string TenDangNhapMoi() => "u" + Guid.NewGuid().ToString("N")[..12];

    private async Task<CanBo> TaoCanBoAsync()
    {
        await using var db = NewContext();
        var canBo = new CanBo(
            "T" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            "Cán bộ " + Guid.NewGuid().ToString("N")[..6],
            "Cán bộ",
            laQuanGiao: false);
        db.CanBo.Add(canBo);
        await db.SaveChangesAsync();
        return canBo;
    }

    private async Task<int> LayVaiTroIdAsync(string maVaiTro)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.VaiTro.Where(v => v.Ma == maVaiTro).Select(v => v.Id).SingleAsync();
    }

    private async Task<NguoiDung> DocNguoiDungAsync(int nguoiDungId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.NguoiDung.AsNoTracking().SingleAsync(u => u.Id == nguoiDungId);
    }

    private async Task<List<string>> VaiTroCuaTaiKhoanAsync(int nguoiDungId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await (from nguoiDungVaiTro in db.NguoiDungVaiTro
                      join vaiTro in db.VaiTro on nguoiDungVaiTro.VaiTroId equals vaiTro.Id
                      where nguoiDungVaiTro.NguoiDungId == nguoiDungId
                      select vaiTro.Ma).ToListAsync();
    }

    private async Task<List<NhatKyThaoTac>> NhatKyCuaTaiKhoanAsync(int nguoiDungId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.NhatKyThaoTac
            .Where(n => n.TenBang == "NguoiDung" && n.BanGhiId == nguoiDungId)
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    private async Task DatHoatDongAsync(int nguoiDungId, bool dangHoatDong)
    {
        await using var db = _fixture.Database.CreateDbContext();
        var nguoiDung = await db.NguoiDung.SingleAsync(u => u.Id == nguoiDungId);
        nguoiDung.DangHoatDong = dangHoatDong;
        await db.SaveChangesAsync();
    }

    private async Task<int> DemNguoiDungAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.NguoiDung.CountAsync();
    }

    private async Task<int> DemNhatKyAsync()
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.NhatKyThaoTac.CountAsync();
    }

    [SqlServerFact]
    public async Task Tao_TaiKhoanMoi_PhaiDoiMatKhau_LuuVaiTro_VaGhiThemKhongCoHash()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var vaiTroId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);
        var tenDangNhap = TenDangNhapMoi();

        await using var db = NewContext();
        var ketQua = await TaoService(db).TaoAsync(new TaoTaiKhoanRequest(tenDangNhap, canBo.Id, [vaiTroId]));

        ketQua.MatKhauTam.Length.ShouldBe(MatKhauTam.DoDai);
        ChinhSachMatKhau.KiemTra(ketQua.MatKhauTam).ShouldBeEmpty();

        var nguoiDung = await DocNguoiDungAsync(ketQua.NguoiDungId);
        nguoiDung.TenDangNhap.ShouldBe(tenDangNhap);
        nguoiDung.CanBoId.ShouldBe(canBo.Id);
        nguoiDung.DangHoatDong.ShouldBeTrue();
        nguoiDung.PhaiDoiMatKhau.ShouldBeTrue();
        (await VaiTroCuaTaiKhoanAsync(ketQua.NguoiDungId)).ShouldBe([MaVaiTro.LuuKy]);

        var log = await NhatKyCuaTaiKhoanAsync(ketQua.NguoiDungId);
        log.ShouldContain(n => n.HanhDong == HanhDong.Them);
        log.ShouldAllBe(n => !(n.DuLieuCu ?? "").Contains("PBKDF2")
            && !(n.DuLieuMoi ?? "").Contains("PBKDF2")
            && !(n.DuLieuCu ?? "").Contains(ketQua.MatKhauTam)
            && !(n.DuLieuMoi ?? "").Contains(ketQua.MatKhauTam));
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task Tao_TenDangNhapDaTonTai_BiTuChoi()
    {
        await DangNhapAsync("admin");
        var canBo1 = await TaoCanBoAsync();
        var canBo2 = await TaoCanBoAsync();
        var vaiTroId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);
        var tenDangNhap = TenDangNhapMoi();

        await using var db = NewContext();
        var service = TaoService(db);
        await service.TaoAsync(new TaoTaiKhoanRequest(tenDangNhap, canBo1.Id, [vaiTroId]));

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => service.TaoAsync(new TaoTaiKhoanRequest(tenDangNhap, canBo2.Id, [vaiTroId])));

        loi.Message.ShouldBe(TaiKhoanService.LoiTrungTenDangNhap);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task Tao_CanBoDaCoTaiKhoanHoatDong_BiTuChoi()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var vaiTroId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);

        await using var db = NewContext();
        var service = TaoService(db);
        await service.TaoAsync(new TaoTaiKhoanRequest(TenDangNhapMoi(), canBo.Id, [vaiTroId]));

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => service.TaoAsync(new TaoTaiKhoanRequest(TenDangNhapMoi(), canBo.Id, [vaiTroId])));

        loi.Message.ShouldBe(TaiKhoanService.LoiCanBoDaCoTaiKhoan);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task Tao_TaiKhoanHoatDongThuHaiChoCungCanBo_BiChiMucLocChan()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var vaiTroId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);

        await using (var db = NewContext())
            await TaoService(db).TaoAsync(new TaoTaiKhoanRequest(TenDangNhapMoi(), canBo.Id, [vaiTroId]));

        // The service's pre-check is bypassed on purpose: the filtered unique index is the backstop.
        await using var raw = NewContext();
        raw.NguoiDung.Add(new NguoiDung
        {
            TenDangNhap = TenDangNhapMoi(),
            CanBoId = canBo.Id,
            MatKhauHash = "PBKDF2-SHA256$1$abc$def",
            DangHoatDong = true,
        });

        await Should.ThrowAsync<TrungGiaTriDuyNhatException>(() => ((IAppDbContext)raw).SaveChangesAsync());
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task LayDanhSach_HienThiCanBo_VaiTro_VaTrangThai()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var vaiTroId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);
        var tenDangNhap = TenDangNhapMoi();

        int nguoiDungId;
        await using (var db = NewContext())
            nguoiDungId = (await TaoService(db).TaoAsync(new TaoTaiKhoanRequest(tenDangNhap, canBo.Id, [vaiTroId]))).NguoiDungId;

        await using (var db = _fixture.Database.CreateDbContext())
        {
            var nguoiDung = await db.NguoiDung.SingleAsync(u => u.Id == nguoiDungId);
            nguoiDung.KhoaDen = _fixture.Clock.Now.AddMinutes(10);
            await db.SaveChangesAsync();
        }

        await using (var db = NewContext())
        {
            var dto = (await TaoService(db).LayDanhSachAsync()).Single(t => t.Id == nguoiDungId);

            dto.TenDangNhap.ShouldBe(tenDangNhap);
            dto.HoTenCanBo.ShouldBe(canBo.HoTen);
            dto.TenVaiTro.ShouldBe("Cán bộ theo dõi tiền lưu ký");
            dto.VaiTroIds.ShouldBe([vaiTroId]);
            dto.DangHoatDong.ShouldBeTrue();
            dto.DangBiKhoa.ShouldBeTrue();
            dto.PhaiDoiMatKhau.ShouldBeTrue();
        }

        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task LayCanBoDeTaoTaiKhoan_BoNguoiDaCoTaiKhoanHoatDong()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var canBoMoi = await TaoCanBoAsync();
        var vaiTroId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);

        await using var db = NewContext();
        var service = TaoService(db);
        await service.TaoAsync(new TaoTaiKhoanRequest(TenDangNhapMoi(), canBo.Id, [vaiTroId]));

        var danhSach = await service.LayCanBoDeTaoTaiKhoanAsync();

        danhSach.ShouldNotContain(c => c.Id == canBo.Id);
        danhSach.ShouldContain(c => c.Id == canBoMoi.Id);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task NgungHoatDong_HaiQuanTri_MotTaiKhoanBiNgung()
    {
        await DangNhapAsync("admin");
        var canBoA = await TaoCanBoAsync();
        var canBoB = await TaoCanBoAsync();
        var quanTriId = await LayVaiTroIdAsync(MaVaiTro.QuanTri);
        var tenA = TenDangNhapMoi();
        var tenB = TenDangNhapMoi();

        int idA;
        int idB;
        await using (var db = NewContext())
        {
            var service = TaoService(db);
            idA = (await service.TaoAsync(new TaoTaiKhoanRequest(tenA, canBoA.Id, [quanTriId]))).NguoiDungId;
            idB = (await service.TaoAsync(new TaoTaiKhoanRequest(tenB, canBoB.Id, [quanTriId]))).NguoiDungId;
        }

        // The actor is A; the seeded admin plus A stay, so B may be deactivated.
        await DangNhapAsync(tenA);
        await using (var db = NewContext())
            await TaoService(db).NgungHoatDongAsync(idB);

        (await DocNguoiDungAsync(idB)).DangHoatDong.ShouldBeFalse();
        var log = await NhatKyCuaTaiKhoanAsync(idB);
        log.ShouldContain(n => n.HanhDong == HanhDong.Sua
            && n.DuLieuCu!.Contains("DangHoatDong") && n.DuLieuMoi!.Contains("DangHoatDong"));

        await DatHoatDongAsync(idA, false);
        await DatHoatDongAsync(idB, false);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task NgungHoatDong_TaiKhoanCuaChinhMinh_BiTuChoi()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var quanTriId = await LayVaiTroIdAsync(MaVaiTro.QuanTri);
        var tenDangNhap = TenDangNhapMoi();

        int nguoiDungId;
        await using (var db = NewContext())
            nguoiDungId = (await TaoService(db).TaoAsync(new TaoTaiKhoanRequest(tenDangNhap, canBo.Id, [quanTriId]))).NguoiDungId;

        await DangNhapAsync(tenDangNhap);
        await using (var db = NewContext())
        {
            var loi = await Should.ThrowAsync<LoiNghiepVuException>(
                () => TaoService(db).NgungHoatDongAsync(nguoiDungId));
            loi.Message.ShouldBe(TaiKhoanService.LoiKhongTheTuNgung);
        }

        (await DocNguoiDungAsync(nguoiDungId)).DangHoatDong.ShouldBeTrue();
        await DatHoatDongAsync(nguoiDungId, false);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task KichHoatLai_CanBoDaCoTaiKhoanMoi_BiTuChoi()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var vaiTroId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);
        var tenCu = TenDangNhapMoi();

        int idCu;
        await using (var db = NewContext())
            idCu = (await TaoService(db).TaoAsync(new TaoTaiKhoanRequest(tenCu, canBo.Id, [vaiTroId]))).NguoiDungId;
        await DatHoatDongAsync(idCu, false);

        int idMoi;
        await using (var db = NewContext())
            idMoi = (await TaoService(db).TaoAsync(new TaoTaiKhoanRequest(TenDangNhapMoi(), canBo.Id, [vaiTroId]))).NguoiDungId;

        await using (var db = NewContext())
        {
            var loi = await Should.ThrowAsync<LoiNghiepVuException>(
                () => TaoService(db).KichHoatLaiAsync(idCu));
            loi.Message.ShouldBe(TaiKhoanService.LoiCanBoDaCoTaiKhoan);
        }

        (await DocNguoiDungAsync(idCu)).DangHoatDong.ShouldBeFalse();
        await DatHoatDongAsync(idMoi, false);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task MoKhoa_XoaSoLanSaiVaKhoaDen()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var vaiTroId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);

        int nguoiDungId;
        await using (var db = NewContext())
            nguoiDungId = (await TaoService(db).TaoAsync(new TaoTaiKhoanRequest(TenDangNhapMoi(), canBo.Id, [vaiTroId]))).NguoiDungId;

        await using (var db = _fixture.Database.CreateDbContext())
        {
            var nguoiDung = await db.NguoiDung.SingleAsync(u => u.Id == nguoiDungId);
            nguoiDung.SoLanSai = NguoiDung.SoLanSaiToiDa;
            nguoiDung.KhoaDen = _fixture.Clock.Now.AddMinutes(15);
            await db.SaveChangesAsync();
        }

        await using (var db = NewContext())
            await TaoService(db).MoKhoaAsync(nguoiDungId);

        var sau = await DocNguoiDungAsync(nguoiDungId);
        sau.SoLanSai.ShouldBe((byte)0);
        sau.KhoaDen.ShouldBeNull();
        var log = await NhatKyCuaTaiKhoanAsync(nguoiDungId);
        log.ShouldContain(n => n.HanhDong == HanhDong.Sua
            && n.DuLieuCu!.Contains("SoLanSai") && n.DuLieuMoi!.Contains("SoLanSai"));
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task DatLaiMatKhau_MatKhauTamMoi_BuocDoi_XoaKhoa_VaKhongVaoNhatKy()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var vaiTroId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);
        var tenDangNhap = TenDangNhapMoi();

        int nguoiDungId;
        await using (var db = NewContext())
            nguoiDungId = (await TaoService(db).TaoAsync(new TaoTaiKhoanRequest(tenDangNhap, canBo.Id, [vaiTroId]))).NguoiDungId;

        // A reset after the first change, with the account locked, is the realistic case.
        await using (var db = _fixture.Database.CreateDbContext())
        {
            var nguoiDung = await db.NguoiDung.SingleAsync(u => u.Id == nguoiDungId);
            nguoiDung.PhaiDoiMatKhau = false;
            nguoiDung.SoLanSai = NguoiDung.SoLanSaiToiDa;
            nguoiDung.KhoaDen = _fixture.Clock.Now.AddMinutes(15);
            await db.SaveChangesAsync();
        }

        string matKhauTam;
        await using (var db = NewContext())
            matKhauTam = await TaoService(db).DatLaiMatKhauAsync(nguoiDungId);

        ChinhSachMatKhau.KiemTra(matKhauTam).ShouldBeEmpty();
        var sau = await DocNguoiDungAsync(nguoiDungId);
        sau.PhaiDoiMatKhau.ShouldBeTrue();
        sau.SoLanSai.ShouldBe((byte)0);
        sau.KhoaDen.ShouldBeNull();

        var log = await NhatKyCuaTaiKhoanAsync(nguoiDungId);
        log.ShouldContain(n => n.DuLieuMoi != null
            && n.DuLieuMoi.Contains($"\"SuKien\":\"{SuKienTaiKhoan.DatLaiMatKhau}\""));
        log.ShouldAllBe(n => !(n.DuLieuCu ?? "").Contains("PBKDF2")
            && !(n.DuLieuMoi ?? "").Contains("PBKDF2")
            && !(n.DuLieuCu ?? "").Contains(matKhauTam)
            && !(n.DuLieuMoi ?? "").Contains(matKhauTam));

        // The next sign-in with the temporary password is accepted but must change it.
        _fixture.User.DangXuat();
        await using (var db = NewContext())
        {
            var store = new NguoiDungStore(db);
            var ghiNhatKy = new GhiNhatKy(db, new NhatKyFactory(_fixture.Clock, _fixture.User));
            var ghiNhanSai = new GhiNhanDangNhapSaiService(store, _fixture.Clock, new DangNhapOptions { ThoiGianKhoaPhut = 15 }, ghiNhatKy);
            var dangNhap = new DangNhapService(store, _hasher, _fixture.User, ghiNhatKy, _fixture.Clock, ghiNhanSai);

            var ketQua = await dangNhap.DangNhapAsync(tenDangNhap, matKhauTam);

            ketQua.ThanhCong.ShouldBeTrue();
            ketQua.PhaiDoiMatKhau.ShouldBeTrue();
        }

        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task CapNhatVaiTro_ThayVaiTro_LuuMotDongSuaVoiHaiDanhSachDaSapXep()
    {
        await DangNhapAsync("admin");
        var canBo = await TaoCanBoAsync();
        var luuKyId = await LayVaiTroIdAsync(MaVaiTro.LuuKy);
        var keToanId = await LayVaiTroIdAsync(MaVaiTro.KeToan);

        int nguoiDungId;
        await using (var db = NewContext())
            nguoiDungId = (await TaoService(db).TaoAsync(new TaoTaiKhoanRequest(TenDangNhapMoi(), canBo.Id, [luuKyId]))).NguoiDungId;

        await using (var db = NewContext())
            await TaoService(db).CapNhatVaiTroAsync(nguoiDungId, [keToanId]);

        (await VaiTroCuaTaiKhoanAsync(nguoiDungId)).ShouldBe([MaVaiTro.KeToan]);
        var log = await NhatKyCuaTaiKhoanAsync(nguoiDungId);
        var sua = log.Single(n => n.HanhDong == HanhDong.Sua && n.DuLieuCu?.Contains("\"VaiTro\"") == true);
        sua.DuLieuCu.ShouldBe("""{"VaiTro":["LUU_KY"]}""");
        sua.DuLieuMoi.ShouldBe("""{"VaiTro":["KE_TOAN"]}""");
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task CapNhatVaiTro_BoVaiTroQuanTriCuaQuanTriDuyNhat_BiTuChoi()
    {
        // A database of its own: the seeded admin is then the only administrator, deterministically.
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateDbContext();
        var admin = await db.NguoiDung.SingleAsync(u => u.TenDangNhap == "admin");
        _fixture.User.DangNhap(admin.Id, admin.TenDangNhap, admin.CanBoId, admin.TenDangNhap);

        var keToanId = await db.VaiTro.Where(v => v.Ma == MaVaiTro.KeToan).Select(v => v.Id).SingleAsync();
        var service = TaoService(db);

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => service.CapNhatVaiTroAsync(admin.Id, [keToanId]));

        loi.Message.ShouldBe(KiemTraConQuanTri.LoiPhaiConQuanTri);
        var vaiTroSau = await (from nguoiDungVaiTro in db.NguoiDungVaiTro
                               join vaiTro in db.VaiTro on nguoiDungVaiTro.VaiTroId equals vaiTro.Id
                               where nguoiDungVaiTro.NguoiDungId == admin.Id
                               select vaiTro.Ma).ToListAsync();
        vaiTroSau.ShouldBe([MaVaiTro.QuanTri]);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task KiemTraConQuanTri_NgungHoatDongQuanTriDuyNhat_BiTuChoi()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateDbContext();
        var adminId = await db.NguoiDung.Where(u => u.TenDangNhap == "admin").Select(u => u.Id).SingleAsync();

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => new KiemTraConQuanTri(db).YeuCauKhiNgungHoatDongAsync(adminId));

        loi.Message.ShouldBe(KiemTraConQuanTri.LoiPhaiConQuanTri);
    }

    [SqlServerFact]
    public async Task VaiTroService_BoQuyenQuanTriKhoiVaiTroDuyNhat_BiTuChoi()
    {
        // Story 2.3's role editor must refuse removing HT.Sua from the last administrator role.
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateDbContext();
        var admin = await db.NguoiDung.SingleAsync(u => u.TenDangNhap == "admin");
        _fixture.User.DangNhap(admin.Id, admin.TenDangNhap, admin.CanBoId, admin.TenDangNhap);

        var ghiNhatKy = new GhiNhatKy(db, new NhatKyFactory(_fixture.Clock, _fixture.User));
        var vaiTroService = new VaiTroService(db, new KiemTraQuyen(db, _fixture.User), ghiNhatKy, new KiemTraConQuanTri(db));
        var vaiTro = (await vaiTroService.LayDanhSachAsync()).Single(v => v.Ma == MaVaiTro.QuanTri);

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => vaiTroService.CapNhatQuyenAsync(vaiTro.Id, [MaQuyen.HT.Xem], vaiTro.RowVer));

        loi.Message.ShouldBe(KiemTraConQuanTri.LoiPhaiConQuanTri);
        (await vaiTroService.LayQuyenCuaVaiTroAsync(vaiTro.Id)).ShouldContain(MaQuyen.HT.Sua);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task Tao_KhongCoQuyen_KhongGhiGiCa()
    {
        // The leadership role has HT.Xem but not HT.Sua.
        var canBo = await TaoCanBoAsync();
        var lanhDaoId = await LayVaiTroIdAsync(MaVaiTro.LanhDao);
        var tenDangNhap = TenDangNhapMoi();

        int nguoiDungId;
        await using (var db = NewContext())
        {
            var nguoiDung = new NguoiDung
            {
                TenDangNhap = tenDangNhap,
                CanBoId = canBo.Id,
                MatKhauHash = "PBKDF2-SHA256$1$abc$def",
                DangHoatDong = true,
            };
            db.NguoiDung.Add(nguoiDung);
            await db.SaveChangesAsync();
            nguoiDungId = nguoiDung.Id;
            db.NguoiDungVaiTro.Add(new NguoiDungVaiTro { NguoiDungId = nguoiDungId, VaiTroId = lanhDaoId });
            await db.SaveChangesAsync();
        }

        _fixture.User.DangNhap(nguoiDungId, tenDangNhap, canBo.Id, "Cán bộ");
        var soNguoiDungTruoc = await DemNguoiDungAsync();
        var soNhatKyTruoc = await DemNhatKyAsync();

        await using (var db = NewContext())
        {
            await Should.ThrowAsync<KhongCoQuyenException>(
                () => TaoService(db).TaoAsync(new TaoTaiKhoanRequest(TenDangNhapMoi(), canBo.Id, [lanhDaoId])));
        }

        (await DemNguoiDungAsync()).ShouldBe(soNguoiDungTruoc);
        (await DemNhatKyAsync()).ShouldBe(soNhatKyTruoc);
        _fixture.User.DangXuat();
    }
}
