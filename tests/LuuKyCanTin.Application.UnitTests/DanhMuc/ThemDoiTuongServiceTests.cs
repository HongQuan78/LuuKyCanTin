using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.DanhMuc;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.DanhMuc;

public class ThemDoiTuongServiceTests
{
    private readonly IDoiTuongStore _store = Substitute.For<IDoiTuongStore>();
    private readonly FakeClock _clock = new(new DateTime(2026, 10, 1, 8, 0, 0));
    private readonly ThemDoiTuongService _service;

    public ThemDoiTuongServiceTests()
    {
        _store.ThemAsync(Arg.Any<DoiTuong>(), Arg.Any<CancellationToken>()).Returns(true);
        _service = new ThemDoiTuongService(_store, _clock);
    }

    private static ThemDoiTuongRequest Request(string maSo = "DT-0001") =>
        new(maSo, "Nguyễn Văn A", 1990, LoaiDoiTuong.TamGiuTamGiam, new DateOnly(2026, 9, 30), "A3");

    [Fact]
    public async Task HappyPath_SavesAManagedDetaineeWithAZeroBalance()
    {
        DoiTuong? daThem = null;
        _store.When(s => s.ThemAsync(Arg.Any<DoiTuong>(), Arg.Any<CancellationToken>()))
            .Do(call => daThem = call.Arg<DoiTuong>());

        var ketQua = await _service.ThemAsync(Request());

        ketQua.ThanhCong.ShouldBeTrue();
        daThem.ShouldNotBeNull();
        daThem.TrangThai.ShouldBe(TrangThaiDoiTuong.DangQuanLy);
        daThem.SoDuLuuKy.ShouldBe(0m);
        daThem.HoTen.ShouldBe("Nguyễn Văn A");
        await _store.Received(1).ThemAsync(Arg.Any<DoiTuong>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ADuplicateCode_ReturnsTheFriendlyMessageAndSavesNothing()
    {
        _store.MaSoDaTonTaiAsync("DT-0001", Arg.Any<CancellationToken>()).Returns(true);

        var ketQua = await _service.ThemAsync(Request());

        ketQua.ThanhCong.ShouldBeFalse();
        ketQua.ThongBao.ShouldBe(ThemDoiTuongService.MaSoDaTonTai);
        await _store.DidNotReceive().ThemAsync(Arg.Any<DoiTuong>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AConcurrentInsertThatWinsTheRace_ReturnsTheSameFriendlyMessage()
    {
        // The pre-check saw nothing; the unique index rejected the insert inside the store.
        _store.MaSoDaTonTaiAsync("DT-0001", Arg.Any<CancellationToken>()).Returns(false);
        _store.ThemAsync(Arg.Any<DoiTuong>(), Arg.Any<CancellationToken>()).Returns(false);

        var ketQua = await _service.ThemAsync(Request());

        ketQua.ThanhCong.ShouldBeFalse();
        ketQua.ThongBao.ShouldBe(ThemDoiTuongService.MaSoDaTonTai);
    }

    [Fact]
    public async Task ABlankCode_FailsValidationBeforeTouchingTheStore()
    {
        var ketQua = await _service.ThemAsync(Request(maSo: "  "));

        ketQua.ThanhCong.ShouldBeFalse();
        await _store.DidNotReceive().MaSoDaTonTaiAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _store.DidNotReceive().ThemAsync(Arg.Any<DoiTuong>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AFutureEntryDate_FailsValidation()
    {
        var request = Request() with { NgayVao = _clock.Today.AddDays(1) };

        (await _service.ThemAsync(request)).ThanhCong.ShouldBeFalse();
    }
}
