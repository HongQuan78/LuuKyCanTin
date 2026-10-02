using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

public class DangNhapServiceTests
{
    private const string HashHopLe = "PBKDF2-SHA256$1$abc$def";

    private readonly INguoiDungStore _store = Substitute.For<INguoiDungStore>();
    private readonly IMatKhauHasher _hasher = Substitute.For<IMatKhauHasher>();
    private readonly ICurrentUserSession _phien = Substitute.For<ICurrentUserSession>();
    private readonly IGhiNhatKy _ghiNhatKy = Substitute.For<IGhiNhatKy>();
    private readonly DangNhapService _service;

    public DangNhapServiceTests()
    {
        _service = new DangNhapService(_store, _hasher, _phien, _ghiNhatKy);
        _store.TimTheoDangNhapAsync("admin", Arg.Any<CancellationToken>()).Returns(new NguoiDung
        {
            Id = 3,
            TenDangNhap = "admin",
            MatKhauHash = HashHopLe,
            DangHoatDong = true,
        });
    }

    [Fact]
    public async Task CorrectPassword_SetsTheSessionAndLogsDangNhap()
    {
        _hasher.Verify("LuuKy@2026", HashHopLe).Returns(true);

        var ketQua = await _service.DangNhapAsync(" admin ", "LuuKy@2026");

        ketQua.ThanhCong.ShouldBeTrue();
        _phien.Received(1).DangNhap(3, "admin");
        await _ghiNhatKy.Received(1).GhiAsync(
            HanhDong.DangNhap, "NguoiDung", 3, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WrongPassword_FailsWithOneGenericMessage()
    {
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var ketQua = await _service.DangNhapAsync("admin", "sai");

        ketQua.ThanhCong.ShouldBeFalse();
        ketQua.ThongBao.ShouldBe(DangNhapService.SaiThongTin);
        _phien.DidNotReceive().DangNhap(Arg.Any<int>(), Arg.Any<string>());
        await _ghiNhatKy.DidNotReceive().GhiAsync(
            Arg.Any<HanhDong>(), Arg.Any<string?>(), Arg.Any<long?>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnInactiveAccount_FailsWithoutCheckingThePassword()
    {
        _store.TimTheoDangNhapAsync("admin", Arg.Any<CancellationToken>()).Returns(new NguoiDung
        {
            Id = 3,
            TenDangNhap = "admin",
            MatKhauHash = HashHopLe,
            DangHoatDong = false,
        });

        var ketQua = await _service.DangNhapAsync("admin", "LuuKy@2026");

        ketQua.ThanhCong.ShouldBeFalse();
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task AnUnknownUser_FailsWithTheSameMessage()
    {
        _store.TimTheoDangNhapAsync("khong-ton-tai", Arg.Any<CancellationToken>()).Returns((NguoiDung?)null);

        var ketQua = await _service.DangNhapAsync("khong-ton-tai", "bất kỳ");

        ketQua.ThanhCong.ShouldBeFalse();
        ketQua.ThongBao.ShouldBe(DangNhapService.SaiThongTin);
    }
}
