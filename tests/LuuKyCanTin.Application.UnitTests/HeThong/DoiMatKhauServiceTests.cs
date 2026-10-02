using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.HeThong;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

public class DoiMatKhauServiceTests
{
    private const string HashCu = "PBKDF2-SHA256$1$cu$cu";
    private static readonly DateTime BayGio = new(2026, 10, 2, 9, 0, 0);

    private readonly INguoiDungStore _store = Substitute.For<INguoiDungStore>();
    private readonly IMatKhauHasher _hasher = Substitute.For<IMatKhauHasher>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IGhiNhatKy _ghiNhatKy = Substitute.For<IGhiNhatKy>();
    private readonly DangNhapOptions _options = new() { ThoiGianKhoaPhut = 15 };
    private readonly DoiMatKhauService _service;
    private NguoiDung _nguoiDung = null!;

    public DoiMatKhauServiceTests()
    {
        var ghiNhanSai = new GhiNhanDangNhapSaiService(_store, new FakeClock(BayGio), _options, _ghiNhatKy);
        _service = new DoiMatKhauService(_store, _hasher, _currentUser, _ghiNhatKy, ghiNhanSai);
    }

    private void SeedNguoiDung(bool phaiDoiMatKhau = false)
    {
        _currentUser.NguoiDungId.Returns(7);
        _nguoiDung = new NguoiDung
        {
            Id = 7,
            TenDangNhap = "luuky",
            MatKhauHash = HashCu,
            DangHoatDong = true,
            PhaiDoiMatKhau = phaiDoiMatKhau,
        };
        _store.TimTheoIdAsync(7, Arg.Any<CancellationToken>()).Returns(_nguoiDung);
        _store.LuuAsync(_nguoiDung, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task DoiMatKhau_WithAValidNewPassword_StoresTheHashAndLogs()
    {
        SeedNguoiDung(phaiDoiMatKhau: true);
        _hasher.Verify("LuuKy@2026", HashCu).Returns(true);
        _hasher.Hash("Moi@2026a").Returns("hash-moi");

        await _service.DoiMatKhauAsync("LuuKy@2026", "Moi@2026a", "Moi@2026a");

        _nguoiDung.MatKhauHash.ShouldBe("hash-moi");
        _nguoiDung.PhaiDoiMatKhau.ShouldBeFalse();
        await _store.Received(1).LuuAsync(_nguoiDung, Arg.Any<CancellationToken>());
        await _ghiNhatKy.Received(1).GhiAsync(
            HanhDong.DangNhap, "NguoiDung", 7,
            Arg.Is<object?>(o => o!.ToString()!.Contains($"SuKien = {SuKienDangNhap.DoiMatKhau}")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DoiMatKhau_WithAWrongCurrentPassword_CountsTowardLockout()
    {
        SeedNguoiDung();
        _hasher.Verify("sai", HashCu).Returns(false);

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => _service.DoiMatKhauAsync("sai", "Moi@2026a", "Moi@2026a"));

        loi.Message.ShouldBe(DangNhapService.SaiThongTin);
        _nguoiDung.SoLanSai.ShouldBe((byte)1);
        await _store.Received(1).LuuAsync(_nguoiDung, Arg.Any<CancellationToken>());
        _hasher.DidNotReceive().Hash(Arg.Any<string>());
    }

    [Fact]
    public async Task DoiMatKhau_WithAWeakPassword_ReportsEveryBrokenRuleAndSavesNothing()
    {
        SeedNguoiDung();
        _hasher.Verify("LuuKy@2026", HashCu).Returns(true);

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => _service.DoiMatKhauAsync("LuuKy@2026", "abc", "abc"));

        loi.Message.ShouldContain(ChinhSachMatKhau.LoiQuaNgan);
        loi.Message.ShouldContain(ChinhSachMatKhau.LoiThieuChuHoa);
        loi.Message.ShouldContain(ChinhSachMatKhau.LoiThieuChuSo);
        await _store.DidNotReceive().LuuAsync(Arg.Any<NguoiDung>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DoiMatKhau_WithTheCurrentPassword_IsRejected()
    {
        SeedNguoiDung();
        _hasher.Verify("LuuKy@2026", HashCu).Returns(true);

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => _service.DoiMatKhauAsync("LuuKy@2026", "LuuKy@2026", "LuuKy@2026"));

        loi.Message.ShouldBe(ChinhSachMatKhau.LoiTrungMatKhauCu);
    }

    [Fact]
    public async Task DoiMatKhau_WithAMismatchedConfirmation_IsRejected()
    {
        SeedNguoiDung();
        _hasher.Verify("LuuKy@2026", HashCu).Returns(true);

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => _service.DoiMatKhauAsync("LuuKy@2026", "Moi@2026a", "Khac@2026a"));

        loi.Message.ShouldBe(ChinhSachMatKhau.LoiXacNhanKhongKhop);
    }

    [Fact]
    public async Task DoiMatKhau_WithoutASignedInUser_IsRejected()
    {
        _currentUser.NguoiDungId.Returns((int?)null);

        var loi = await Should.ThrowAsync<LoiNghiepVuException>(
            () => _service.DoiMatKhauAsync("a", "Moi@2026a", "Moi@2026a"));

        loi.Message.ShouldBe(DoiMatKhauService.LoiChuaDangNhap);
    }
}
