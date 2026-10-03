using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.HeThong;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

public class DangNhapServiceTests
{
    private const string HashHopLe = "PBKDF2-SHA256$1$abc$def";
    private static readonly DateTime BayGio = new(2026, 10, 2, 9, 0, 0);

    private readonly INguoiDungStore _store = Substitute.For<INguoiDungStore>();
    private readonly IMatKhauHasher _hasher = Substitute.For<IMatKhauHasher>();
    private readonly ICurrentUserSession _phien = Substitute.For<ICurrentUserSession>();
    private readonly IGhiNhatKy _ghiNhatKy = Substitute.For<IGhiNhatKy>();
    private readonly FakeClock _clock = new(BayGio);
    private readonly DangNhapOptions _options = new() { ThoiGianKhoaPhut = 15 };
    private readonly DangNhapService _service;
    private NguoiDung _nguoiDung = null!;

    public DangNhapServiceTests()
    {
        var ghiNhanSai = new GhiNhanDangNhapSaiService(_store, _clock, _options, _ghiNhatKy);
        _service = new DangNhapService(_store, _hasher, _phien, _ghiNhatKy, _clock, ghiNhanSai);
    }

    private void SeedTaiKhoan(byte soLanSai = 0, DateTime? khoaDen = null, bool phaiDoiMatKhau = false)
    {
        _nguoiDung = new NguoiDung
        {
            Id = 3,
            TenDangNhap = "admin",
            MatKhauHash = HashHopLe,
            DangHoatDong = true,
            PhaiDoiMatKhau = phaiDoiMatKhau,
            SoLanSai = soLanSai,
            KhoaDen = khoaDen,
        };
        _store.TimTheoDangNhapAsync("admin", Arg.Any<CancellationToken>()).Returns(_nguoiDung);
        _store.LuuAsync(_nguoiDung, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CorrectPassword_ResetsTheCountersSetsTheSessionAndLogs()
    {
        SeedTaiKhoan(soLanSai: 3, khoaDen: BayGio.AddMinutes(-1));
        _hasher.Verify("LuuKy@2026", HashHopLe).Returns(true);

        var ketQua = await _service.DangNhapAsync(" admin ", "LuuKy@2026");

        ketQua.ThanhCong.ShouldBeTrue();
        ketQua.PhaiDoiMatKhau.ShouldBeFalse();
        _nguoiDung.SoLanSai.ShouldBe((byte)0);
        _nguoiDung.KhoaDen.ShouldBeNull();
        await _store.Received(1).LuuAsync(_nguoiDung, Arg.Any<CancellationToken>());
        _phien.Received(1).DangNhap(3, "admin", null, "admin");
        await _ghiNhatKy.Received(1).GhiAsync(
            HanhDong.DangNhap, "NguoiDung", 3, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CorrectPasswordForAStaffAccount_LoadsTheStaffNameIntoTheSession()
    {
        SeedTaiKhoan();
        _nguoiDung.CanBoId = 7;
        _hasher.Verify("LuuKy@2026", HashHopLe).Returns(true);
        _store.LayHoTenCanBoAsync(7, Arg.Any<CancellationToken>()).Returns("Nguyễn Văn Thủ Quỹ");

        await _service.DangNhapAsync("admin", "LuuKy@2026");

        _phien.Received(1).DangNhap(3, "admin", 7, "Nguyễn Văn Thủ Quỹ");
    }

    [Fact]
    public async Task CorrectPasswordWithForcedChange_IsSuccessfulButAsksForANewPassword()
    {
        SeedTaiKhoan(phaiDoiMatKhau: true);
        _hasher.Verify("LuuKy@2026", HashHopLe).Returns(true);

        var ketQua = await _service.DangNhapAsync("admin", "LuuKy@2026");

        ketQua.ThanhCong.ShouldBeTrue();
        ketQua.PhaiDoiMatKhau.ShouldBeTrue();
        _phien.Received(1).DangNhap(3, "admin", null, "admin");
    }

    [Fact]
    public async Task WrongPassword_CountsTheAttemptAndWritesTheEvent()
    {
        SeedTaiKhoan();
        _hasher.Verify("sai", HashHopLe).Returns(false);

        var ketQua = await _service.DangNhapAsync("admin", "sai");

        ketQua.TrangThai.ShouldBe(TrangThaiDangNhap.SaiThongTin);
        ketQua.ThongBao.ShouldBe(DangNhapService.SaiThongTin);
        _nguoiDung.SoLanSai.ShouldBe((byte)1);
        await _store.Received(1).LuuAsync(_nguoiDung, Arg.Any<CancellationToken>());
        _phien.DidNotReceive().DangNhap(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>());
        await _ghiNhatKy.Received(1).GhiAsync(
            HanhDong.DangNhap, "NguoiDung", 3,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"SuKien = {SuKienDangNhap.DangNhapSai}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FifthWrongPassword_LocksTheAccountAndSaysSo()
    {
        SeedTaiKhoan(soLanSai: NguoiDung.SoLanSaiToiDa - 1);
        _hasher.Verify("sai", HashHopLe).Returns(false);

        var ketQua = await _service.DangNhapAsync("admin", "sai");

        ketQua.TrangThai.ShouldBe(TrangThaiDangNhap.TaiKhoanBiKhoa);
        ketQua.ThongBao.ShouldBe(DangNhapService.LoiTaiKhoanBiKhoa);
        _nguoiDung.KhoaDen.ShouldBe(BayGio.AddMinutes(15));
        await _ghiNhatKy.Received(1).GhiAsync(
            HanhDong.DangNhap, "NguoiDung", 3,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"SuKien = {SuKienDangNhap.KhoaTaiKhoan}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConcurrencyConflict_ReloadsAndReappliesTheFailedAttemptOnce()
    {
        SeedTaiKhoan();
        _hasher.Verify("sai", HashHopLe).Returns(false);
        var soLanLuu = 0;
        _store.LuuAsync(_nguoiDung, Arg.Any<CancellationToken>()).Returns(_ =>
        {
            soLanLuu++;
            if (soLanLuu == 1)
                throw new XungDotDuLieuException(new Exception("row version"));
            return Task.CompletedTask;
        });
        // The store's reload would restore what the other workstation wrote; emulate it.
        _store.TaiLaiAsync(_nguoiDung, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                _nguoiDung.SoLanSai = 0;
                _nguoiDung.KhoaDen = null;
                return Task.CompletedTask;
            });

        var ketQua = await _service.DangNhapAsync("admin", "sai");

        ketQua.TrangThai.ShouldBe(TrangThaiDangNhap.SaiThongTin);
        _nguoiDung.SoLanSai.ShouldBe((byte)1);
        await _store.Received(1).TaiLaiAsync(_nguoiDung, Arg.Any<CancellationToken>());
        await _store.Received(2).LuuAsync(_nguoiDung, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ALockedAccount_IsRefusedWithoutCheckingThePassword()
    {
        SeedTaiKhoan(khoaDen: BayGio.AddMinutes(5));

        var ketQua = await _service.DangNhapAsync("admin", "LuuKy@2026");

        ketQua.TrangThai.ShouldBe(TrangThaiDangNhap.TaiKhoanBiKhoa);
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        await _store.DidNotReceive().LuuAsync(Arg.Any<NguoiDung>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnInactiveAccount_IsRefusedWithoutCheckingThePassword_AndUsesTheLockedMessage()
    {
        SeedTaiKhoan();
        _nguoiDung.DangHoatDong = false;

        var ketQua = await _service.DangNhapAsync("admin", "LuuKy@2026");

        ketQua.TrangThai.ShouldBe(TrangThaiDangNhap.TaiKhoanNgungHoatDong);
        ketQua.ThongBao.ShouldBe(DangNhapService.LoiTaiKhoanBiKhoa);
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task AnExpiredLock_LetsTheRightPasswordThrough()
    {
        SeedTaiKhoan(khoaDen: BayGio.AddMinutes(-1));
        _hasher.Verify("LuuKy@2026", HashHopLe).Returns(true);
        _clock.Advance(TimeSpan.FromMinutes(16));

        var ketQua = await _service.DangNhapAsync("admin", "LuuKy@2026");

        ketQua.ThanhCong.ShouldBeTrue();
        _nguoiDung.KhoaDen.ShouldBeNull();
    }

    [Fact]
    public async Task AnUnknownUser_StillVerifiesAgainstTheDummyHash_AndFailsTheSameWay()
    {
        _store.TimTheoDangNhapAsync("khong-ton-tai", Arg.Any<CancellationToken>()).Returns((NguoiDung?)null);

        var ketQua = await _service.DangNhapAsync("khong-ton-tai", "bất kỳ");

        ketQua.TrangThai.ShouldBe(TrangThaiDangNhap.SaiThongTin);
        ketQua.ThongBao.ShouldBe(DangNhapService.SaiThongTin);
        _hasher.Received(1).Verify("bất kỳ", Arg.Any<string>());
    }

    [Fact]
    public async Task DangXuat_WritesTheEventThenClearsTheSession()
    {
        _phien.NguoiDungId.Returns(3);
        _phien.TenDangNhap.Returns("admin");

        await _service.DangXuatAsync();

        await _ghiNhatKy.Received(1).GhiAsync(
            HanhDong.DangNhap, "NguoiDung", 3,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"SuKien = {SuKienDangNhap.DangXuat}")),
            Arg.Any<CancellationToken>());
        Received.InOrder(() =>
        {
            _ghiNhatKy.GhiAsync(Arg.Any<HanhDong>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
            _phien.DangXuat();
        });
    }

    [Fact]
    public async Task DangXuat_WithoutASession_WritesNothing()
    {
        _phien.NguoiDungId.Returns((int?)null);

        await _service.DangXuatAsync();

        await _ghiNhatKy.DidNotReceive().GhiAsync(
            Arg.Any<HanhDong>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
        _phien.DidNotReceive().DangXuat();
    }
}
