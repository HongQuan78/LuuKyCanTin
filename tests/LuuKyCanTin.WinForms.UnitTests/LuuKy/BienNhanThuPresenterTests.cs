using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.BaoCao;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Application.LuuKy;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Domain.LuuKy;
using LuuKyCanTin.WinForms.LuuKy;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.LuuKy;

public class BienNhanThuPresenterTests
{
    private readonly IDoiTuongStore _doiTuongStore = Substitute.For<IDoiTuongStore>();
    private readonly IChungTuLuuKyStore _chungTuStore = Substitute.For<IChungTuLuuKyStore>();
    private readonly IAppDbContext _db = Substitute.For<IAppDbContext>();
    private readonly INumberingService _numbering = Substitute.For<INumberingService>();
    private readonly ISoDuLuuKyWriter _soDuWriter = Substitute.For<ISoDuLuuKyWriter>();
    private readonly IThongTinDonViStore _donViStore = Substitute.For<IThongTinDonViStore>();
    private readonly IReportRenderer _renderer = Substitute.For<IReportRenderer>();
    private readonly IBienNhanThuView _view = Substitute.For<IBienNhanThuView>();

    private static DoiTuong DoiTuongMau() => new()
    {
        Id = 7,
        MaSo = "DT-0001",
        HoTen = "Nguyễn Văn A",
        LoaiDoiTuong = LoaiDoiTuong.TamGiuTamGiam,
        NgayVao = new DateOnly(2026, 9, 1),
    };

    public BienNhanThuPresenterTests()
    {
        _db.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(new GiaoDichGia());
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _doiTuongStore.TimTheoIdAsync(7, Arg.Any<CancellationToken>()).Returns(DoiTuongMau());
        _doiTuongStore.LayDangQuanLyAsync(Arg.Any<CancellationToken>()).Returns([DoiTuongMau()]);
        _numbering.CapSoAsync("BNT", 2026, Arg.Any<CancellationToken>()).Returns("BNT-2026-00001");
        _soDuWriter.CongAsync(7, 500_000, Arg.Any<CancellationToken>()).Returns((0m, 500_000m));

        _view.DoiTuongId.Returns(7);
        _view.NgayChungTu.Returns(new DateOnly(2026, 10, 1));
        _view.NghiepVu.Returns(NghiepVu.NguoiThanGui);
        _view.HinhThuc.Returns(HinhThuc.TienMat);
        _view.NguoiGuiHoTen.Returns("Trần Thị B");
        _view.QuanHe.Returns("Mẹ");
        _view.SoTien.Returns(500_000m);
    }

    private BienNhanThuPresenter TaoPresenter()
    {
        var clock = new FakeClock(new DateTime(2026, 10, 1, 8, 0, 0));
        var ghiSo = new GhiSoLuuKyService(_doiTuongStore, _chungTuStore, _db, _numbering, _soDuWriter, clock);
        var layDeIn = new LayBienNhanThuDeInQuery(_chungTuStore, _donViStore);
        var danhSach = new LayDoiTuongDangQuanLyQuery(_doiTuongStore);
        return new BienNhanThuPresenter(_view, ScopeFactoryGia.Tao(ghiSo, layDeIn, danhSach, _renderer));
    }

    [Fact]
    public async Task Loading_FillsTheDetaineeSelector()
    {
        await TaoPresenter().TaiAsync();

        _view.Received(1).DanhSachDoiTuong = Arg.Is<IReadOnlyList<DoiTuongChon>>(list => list.Count == 1 && list[0].HoTen == "Nguyễn Văn A");
    }

    [Fact]
    public void TheAmountPreview_ReadsTheAmountInWords()
    {
        TaoPresenter().CapNhatBangChu();

        _view.Received(1).SoTienBangChu = "Năm trăm nghìn đồng";
    }

    [Fact]
    public async Task Posting_Succeeds_ShowsTheNumberAndEnablesPrint()
    {
        await TaoPresenter().GhiSoAsync();

        _view.Received(1).DaGhiSo("BNT-2026-00001", 500_000m);
        _view.DidNotReceive().HienLoi(Arg.Any<string>());
    }

    [Fact]
    public async Task Posting_Fails_ShowsTheBusinessMessage()
    {
        _doiTuongStore.TimTheoIdAsync(7, Arg.Any<CancellationToken>()).Returns(new DoiTuong
        {
            Id = 7,
            MaSo = "DT-0001",
            HoTen = "Nguyễn Văn A",
            NgayVao = new DateOnly(2026, 9, 1),
            TrangThai = TrangThaiDoiTuong.DaChuyenTrai,
            NgayRa = new DateOnly(2026, 9, 30),
        });

        await TaoPresenter().GhiSoAsync();

        _view.Received(1).HienLoi(Arg.Any<string>());
        _view.DidNotReceive().DaGhiSo(Arg.Any<string>(), Arg.Any<decimal>());
    }

    [Fact]
    public async Task AMissingCounter_ThrowsInsteadOfPostingSilently()
    {
        _numbering.CapSoAsync("BNT", 2026, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(new InvalidOperationException("Không có bộ đếm.")));

        await Should.ThrowAsync<InvalidOperationException>(() => TaoPresenter().GhiSoAsync());

        _view.DidNotReceive().DaGhiSo(Arg.Any<string>(), Arg.Any<decimal>());
    }

    [Fact]
    public async Task Printing_RendersTheSnapshotModelAndShowsThePreview()
    {
        var chungTu = ChungTuLuuKy.TaoBienNhanThuDaGhiSo(
            "BNT-2026-00001",
            new DateOnly(2026, 10, 1),
            DoiTuongMau(),
            LoaiPhieu.Thu,
            NghiepVu.NguoiThanGui,
            HinhThuc.TienMat,
            "Trần Thị B",
            "Mẹ",
            null,
            null,
            null,
            "Tiền gửi lưu ký",
            500_000,
            "Năm trăm nghìn đồng",
            0,
            500_000);
        _chungTuStore.TimTheoIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(chungTu);
        _donViStore.LayAsync(Arg.Any<CancellationToken>()).Returns(new ThongTinDonVi { Id = 1, TenDonVi = "Trại …", DiaChi = "…" });
        var pdf = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _renderer.Render(Arg.Any<BienNhanThuModel>()).Returns(pdf);

        var presenter = TaoPresenter();
        await presenter.GhiSoAsync();
        await presenter.InAsync();

        _view.Received(1).HienThiBanIn(pdf, "BienNhanThu-BNT-2026-00001");
    }

    private sealed class GiaoDichGia : IAppTransaction
    {
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
