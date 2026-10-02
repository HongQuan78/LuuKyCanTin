using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.LuuKy;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.LuuKy;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.LuuKy;

public class GhiSoLuuKyServiceTests
{
    private readonly IDoiTuongStore _doiTuongStore = Substitute.For<IDoiTuongStore>();
    private readonly IChungTuLuuKyStore _chungTuStore = Substitute.For<IChungTuLuuKyStore>();
    private readonly IAppDbContext _db = Substitute.For<IAppDbContext>();
    private readonly INumberingService _numbering = Substitute.For<INumberingService>();
    private readonly ISoDuLuuKyWriter _soDuWriter = Substitute.For<ISoDuLuuKyWriter>();
    private readonly IAppTransaction _giaoDich = Substitute.For<IAppTransaction>();
    private readonly FakeClock _clock = new(new DateTime(2026, 10, 1, 8, 0, 0));

    private readonly GhiSoLuuKyService _service;

    public GhiSoLuuKyServiceTests()
    {
        _db.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_giaoDich);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _service = new GhiSoLuuKyService(_doiTuongStore, _chungTuStore, _db, _numbering, _soDuWriter, _clock);

        _doiTuongStore.TimTheoIdAsync(7, Arg.Any<CancellationToken>()).Returns(new DoiTuong
        {
            Id = 7,
            MaSo = "DT-0001",
            HoTen = "Nguyễn Văn A",
            LoaiDoiTuong = LoaiDoiTuong.TamGiuTamGiam,
            NgayVao = new DateOnly(2026, 9, 1),
            TrangThai = TrangThaiDoiTuong.DangQuanLy,
        });
        _numbering.CapSoAsync("BNT", 2026, Arg.Any<CancellationToken>()).Returns("BNT-2026-00001");
        _soDuWriter.CongAsync(7, 500_000, Arg.Any<CancellationToken>()).Returns((0m, 500_000m));
    }

    private static GhiSoBienNhanThuRequest Request(decimal soTien = 500_000) => new()
    {
        DoiTuongId = 7,
        NgayChungTu = new DateOnly(2026, 10, 1),
        NghiepVu = NghiepVu.NguoiThanGui,
        HinhThuc = HinhThuc.TienMat,
        NguoiGuiHoTen = "Trần Thị B",
        QuanHe = "Mẹ",
        SoTien = soTien,
    };

    [Fact]
    public async Task HappyPath_BuildsThePostedReceiptWithSnapshotWordsAndBalances()
    {
        ChungTuLuuKy? daThem = null;
        _chungTuStore.When(s => s.Them(Arg.Any<ChungTuLuuKy>())).Do(call => daThem = call.Arg<ChungTuLuuKy>());

        var ketQua = await _service.GhiSoBienNhanThuAsync(Request());

        ketQua.ThanhCong.ShouldBeTrue();
        ketQua.SoChungTu.ShouldBe("BNT-2026-00001");
        ketQua.SoDuSau.ShouldBe(500_000m);

        daThem.ShouldNotBeNull();
        daThem.SoChungTu.ShouldBe("BNT-2026-00001");
        daThem.TrangThai.ShouldBe(TrangThaiChungTu.DaGhiSo);
        daThem.HoTenDoiTuong.ShouldBe("Nguyễn Văn A");
        daThem.LoaiDoiTuong.ShouldBe(LoaiDoiTuong.TamGiuTamGiam);
        daThem.SoDuTruoc.ShouldBe(0m);
        daThem.SoDuSau.ShouldBe(500_000m);
        daThem.SoTienBangChu.ShouldBe("Năm trăm nghìn đồng");

        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _giaoDich.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Numbering_UsesTheWorkingYearNotTheDocumentDate()
    {
        var request = Request() with { NgayChungTu = new DateOnly(2025, 12, 31) };

        (await _service.GhiSoBienNhanThuAsync(request)).ThanhCong.ShouldBeTrue();

        await _numbering.Received(1).CapSoAsync("BNT", 2026, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BalanceUpdateRefused_ReturnsABusinessErrorAndSavesNothing()
    {
        _soDuWriter.CongAsync(7, 500_000, Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<(decimal SoDuTruoc, decimal SoDuSau)?>(null));

        var ketQua = await _service.GhiSoBienNhanThuAsync(Request());

        ketQua.ThanhCong.ShouldBeFalse();
        ketQua.ThongBao.ShouldNotBeNullOrEmpty();
        _chungTuStore.DidNotReceive().Them(Arg.Any<ChungTuLuuKy>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _giaoDich.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidationFailure_StopsBeforeNumbering()
    {
        var ketQua = await _service.GhiSoBienNhanThuAsync(Request(soTien: 0));

        ketQua.ThanhCong.ShouldBeFalse();
        await _numbering.DidNotReceive().CapSoAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _giaoDich.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ABlankSender_StopsBeforeNumbering(string nguoiGui)
    {
        var ketQua = await _service.GhiSoBienNhanThuAsync(Request() with { NguoiGuiHoTen = nguoiGui });

        ketQua.ThanhCong.ShouldBeFalse();
        await _numbering.DidNotReceive().CapSoAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task APayoutBusinessCode_StopsBeforeNumbering()
    {
        var ketQua = await _service.GhiSoBienNhanThuAsync(Request() with { NghiepVu = NghiepVu.ChoTien });

        ketQua.ThanhCong.ShouldBeFalse();
        await _numbering.DidNotReceive().CapSoAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ATransferWithoutAnAccount_StopsBeforeNumbering()
    {
        var request = Request() with { HinhThuc = HinhThuc.ChuyenKhoan, SoTaiKhoanNguoiGui = null };

        var ketQua = await _service.GhiSoBienNhanThuAsync(request);

        ketQua.ThanhCong.ShouldBeFalse();
        await _numbering.DidNotReceive().CapSoAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnAmountBeyondDecimal18_StopsBeforeNumbering()
    {
        var ketQua = await _service.GhiSoBienNhanThuAsync(Request(soTien: 1_000_000_000_000_000_000m));

        ketQua.ThanhCong.ShouldBeFalse();
        await _numbering.DidNotReceive().CapSoAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnInactiveDetainee_IsRefusedBeforeNumbering()
    {
        var doiTuong = new DoiTuong
        {
            Id = 7,
            MaSo = "DT-0001",
            HoTen = "Nguyễn Văn A",
            NgayVao = new DateOnly(2026, 9, 1),
            TrangThai = TrangThaiDoiTuong.DaChuyenTrai,
            NgayRa = new DateOnly(2026, 9, 30),
        };
        _doiTuongStore.TimTheoIdAsync(7, Arg.Any<CancellationToken>()).Returns(doiTuong);

        var ketQua = await _service.GhiSoBienNhanThuAsync(Request());

        ketQua.ThanhCong.ShouldBeFalse();
        await _numbering.DidNotReceive().CapSoAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _giaoDich.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AMissingCounter_RollsBackTheTransactionAndSavesNothing()
    {
        _numbering.CapSoAsync("BNT", 2026, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Không có bộ đếm cho loại chứng từ BNT năm 2026."));

        await Should.ThrowAsync<InvalidOperationException>(() => _service.GhiSoBienNhanThuAsync(Request()));

        _chungTuStore.DidNotReceive().Them(Arg.Any<ChungTuLuuKy>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

}
