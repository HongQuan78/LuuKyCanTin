using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.HeThong;

public sealed class DangNhapServiceTests : IClassFixture<AppDatabaseFixture>, IAsyncLifetime
{
    private const string MatKhauBanDau = "LuuKy@2026";
    private const string MatKhauMoi = "Moi@2026a";

    private readonly AppDatabaseFixture _fixture;
    private readonly IMatKhauHasher _hasher = new Pbkdf2MatKhauHasher();
    private readonly DangNhapOptions _options = new() { ThoiGianKhoaPhut = 15 };
    private readonly List<AppDbContext> _contexts = [];

    public DangNhapServiceTests(AppDatabaseFixture fixture) => _fixture = fixture;

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

    private DangNhapService TaoDangNhapService(AppDbContext db)
    {
        var store = new NguoiDungStore(db);
        var ghiNhatKy = new GhiNhatKy(db, new NhatKyFactory(_fixture.Clock, _fixture.User));
        var ghiNhanSai = new GhiNhanDangNhapSaiService(store, _fixture.Clock, _options, ghiNhatKy);
        return new DangNhapService(store, _hasher, _fixture.User, ghiNhatKy, _fixture.Clock, ghiNhanSai);
    }

    private DoiMatKhauService TaoDoiMatKhauService(AppDbContext db)
    {
        var store = new NguoiDungStore(db);
        var ghiNhatKy = new GhiNhatKy(db, new NhatKyFactory(_fixture.Clock, _fixture.User));
        var ghiNhanSai = new GhiNhanDangNhapSaiService(store, _fixture.Clock, _options, ghiNhatKy);
        return new DoiMatKhauService(store, _hasher, _fixture.User, ghiNhatKy, ghiNhanSai);
    }

    private async Task<NguoiDung> TaoTaiKhoanAsync(bool phaiDoiMatKhau = false)
    {
        await using var db = NewContext();
        var nguoiDung = new NguoiDung
        {
            TenDangNhap = "u" + Guid.NewGuid().ToString("N")[..12],
            MatKhauHash = _hasher.Hash(MatKhauBanDau),
            DangHoatDong = true,
            PhaiDoiMatKhau = phaiDoiMatKhau,
        };
        db.NguoiDung.Add(nguoiDung);
        await db.SaveChangesAsync();
        return nguoiDung;
    }

    private async Task<List<NhatKyThaoTac>> LogCuaTaiKhoanAsync(int nguoiDungId)
    {
        await using var db = _fixture.Database.CreateDbContext();
        return await db.NhatKyThaoTac
            .Where(n => n.TenBang == "NguoiDung" && n.BanGhiId == nguoiDungId)
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    // "TenDangNhap" and "PhaiDoiMatKhau" contain the event names as substrings, so match the JSON field exactly.
    private static bool CoSuKien(NhatKyThaoTac nhatKy, string suKien) =>
        nhatKy.DuLieuMoi?.Contains($"\"SuKien\":\"{suKien}\"") == true;

    [SqlServerFact]
    public async Task DangNhap_DungMatKhau_DatPhienVaLuuSuKien()
    {
        var taiKhoan = await TaoTaiKhoanAsync();
        await using var db = NewContext();

        var ketQua = await TaoDangNhapService(db).DangNhapAsync(taiKhoan.TenDangNhap, MatKhauBanDau);

        ketQua.ThanhCong.ShouldBeTrue();
        _fixture.User.NguoiDungId.ShouldBe(taiKhoan.Id);
        var log = await LogCuaTaiKhoanAsync(taiKhoan.Id);
        log.ShouldContain(n => n.HanhDong == HanhDong.DangNhap && CoSuKien(n, SuKienDangNhap.DangNhap));
        await using var check = _fixture.Database.CreateDbContext();
        (await check.NguoiDung.SingleAsync(u => u.Id == taiKhoan.Id)).SoLanSai.ShouldBe((byte)0);
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task DangNhap_NamLanSai_KhoaTaiKhoanVaLuuSuKien()
    {
        var taiKhoan = await TaoTaiKhoanAsync();
        await using var db = NewContext();
        var service = TaoDangNhapService(db);

        KetQuaDangNhap? ketQuaCuoi = null;
        for (var lan = 0; lan < NguoiDung.SoLanSaiToiDa; lan++)
            ketQuaCuoi = await service.DangNhapAsync(taiKhoan.TenDangNhap, "sai-mat-khau");

        ketQuaCuoi!.TrangThai.ShouldBe(TrangThaiDangNhap.TaiKhoanBiKhoa);
        await using (var check = _fixture.Database.CreateDbContext())
        {
            var sau = await check.NguoiDung.SingleAsync(u => u.Id == taiKhoan.Id);
            sau.SoLanSai.ShouldBe(NguoiDung.SoLanSaiToiDa);
            sau.KhoaDen.ShouldBe(_fixture.Clock.Now.AddMinutes(15));
        }

        var log = await LogCuaTaiKhoanAsync(taiKhoan.Id);
        log.Count(n => n.HanhDong == HanhDong.DangNhap && CoSuKien(n, SuKienDangNhap.DangNhapSai)).ShouldBe(4);
        log.Count(n => n.HanhDong == HanhDong.DangNhap && CoSuKien(n, SuKienDangNhap.KhoaTaiKhoan)).ShouldBe(1);
    }

    [SqlServerFact]
    public async Task DangNhap_TaiKhoanDangBiKhoa_DungMatKhauVanBiTuChoi()
    {
        var taiKhoan = await TaoTaiKhoanAsync();
        await using (var lockDb = NewContext())
        {
            var lockUser = await lockDb.NguoiDung.SingleAsync(u => u.Id == taiKhoan.Id);
            lockUser.KhoaDen = _fixture.Clock.Now.AddMinutes(10);
            lockUser.SoLanSai = NguoiDung.SoLanSaiToiDa;
            await lockDb.SaveChangesAsync();
        }

        await using var db = NewContext();
        var ketQua = await TaoDangNhapService(db).DangNhapAsync(taiKhoan.TenDangNhap, MatKhauBanDau);

        ketQua.TrangThai.ShouldBe(TrangThaiDangNhap.TaiKhoanBiKhoa);
        _fixture.User.NguoiDungId.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task DangNhap_HetThoiGianKhoa_DatVaoVaLamMoiSoLanSai()
    {
        var taiKhoan = await TaoTaiKhoanAsync();
        await using (var lockDb = NewContext())
        {
            var lockUser = await lockDb.NguoiDung.SingleAsync(u => u.Id == taiKhoan.Id);
            lockUser.KhoaDen = _fixture.Clock.Now.AddMinutes(1);
            lockUser.SoLanSai = NguoiDung.SoLanSaiToiDa;
            await lockDb.SaveChangesAsync();
        }

        _fixture.Clock.Advance(TimeSpan.FromMinutes(16));

        await using var db = NewContext();
        var ketQua = await TaoDangNhapService(db).DangNhapAsync(taiKhoan.TenDangNhap, MatKhauBanDau);

        ketQua.ThanhCong.ShouldBeTrue();
        await using var check = _fixture.Database.CreateDbContext();
        var sau = await check.NguoiDung.SingleAsync(u => u.Id == taiKhoan.Id);
        sau.SoLanSai.ShouldBe((byte)0);
        sau.KhoaDen.ShouldBeNull();
        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task DangNhap_TaiKhoanNgungHoatDong_BiTuChoiVoiCungThongBaoKhoa()
    {
        var taiKhoan = await TaoTaiKhoanAsync();
        await using (var db = NewContext())
        {
            var nguoiDung = await db.NguoiDung.SingleAsync(u => u.Id == taiKhoan.Id);
            nguoiDung.DangHoatDong = false;
            await db.SaveChangesAsync();
        }

        await using var checkDb = NewContext();
        var ketQua = await TaoDangNhapService(checkDb).DangNhapAsync(taiKhoan.TenDangNhap, MatKhauBanDau);

        ketQua.TrangThai.ShouldBe(TrangThaiDangNhap.TaiKhoanNgungHoatDong);
        ketQua.ThongBao.ShouldBe(DangNhapService.LoiTaiKhoanBiKhoa);
    }

    [SqlServerFact]
    public async Task DangNhap_AdminDaSeed_PhaiDoiMatKhau_RoiDangNhapLaiBinhThuong()
    {
        // The admin that ships with the database is forced to change its password on the first sign-in.
        await using (var db = NewContext())
        {
            var admin = await db.NguoiDung.SingleAsync(u => u.TenDangNhap == "admin");
            admin.PhaiDoiMatKhau.ShouldBeTrue();

            var ketQua = await TaoDangNhapService(db).DangNhapAsync("admin", MatKhauBanDau);
            ketQua.ThanhCong.ShouldBeTrue();
            ketQua.PhaiDoiMatKhau.ShouldBeTrue();
        }

        await using (var db = NewContext())
            await TaoDoiMatKhauService(db).DoiMatKhauAsync(MatKhauBanDau, MatKhauMoi, MatKhauMoi);

        await using (var db = NewContext())
        {
            var ketQua = await TaoDangNhapService(db).DangNhapAsync("admin", MatKhauMoi);
            ketQua.ThanhCong.ShouldBeTrue();
            ketQua.PhaiDoiMatKhau.ShouldBeFalse();
        }

        // Put the seeded admin back so other tests in this class still find it as shipped.
        await using (var db = NewContext())
        {
            var admin = await db.NguoiDung.SingleAsync(u => u.TenDangNhap == "admin");
            admin.MatKhauHash = _hasher.Hash(MatKhauBanDau);
            admin.PhaiDoiMatKhau = true;
            await db.SaveChangesAsync();
        }

        _fixture.User.DangXuat();
    }

    [SqlServerFact]
    public async Task DoiMatKhau_KhongBaoGioGhiMatKhauHashVaoNhatKy()
    {
        var taiKhoan = await TaoTaiKhoanAsync(phaiDoiMatKhau: true);
        await using (var db = NewContext())
        {
            var ketQua = await TaoDangNhapService(db).DangNhapAsync(taiKhoan.TenDangNhap, MatKhauBanDau);
            ketQua.PhaiDoiMatKhau.ShouldBeTrue();
        }

        await using (var db = NewContext())
            await TaoDoiMatKhauService(db).DoiMatKhauAsync(MatKhauBanDau, MatKhauMoi, MatKhauMoi);

        var log = await LogCuaTaiKhoanAsync(taiKhoan.Id);
        log.Count(n => CoSuKien(n, SuKienDangNhap.DoiMatKhau)).ShouldBe(1);
        log.ShouldAllBe(n => !(n.DuLieuCu ?? "").Contains("PBKDF2") && !(n.DuLieuMoi ?? "").Contains("PBKDF2"));
        _fixture.User.DangXuat();
    }
}
