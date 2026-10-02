using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

public class DoiMatKhauPresenterTests
{
    private readonly INguoiDungStore _store = Substitute.For<INguoiDungStore>();
    private readonly IMatKhauHasher _hasher = Substitute.For<IMatKhauHasher>();
    private readonly ICurrentUserSession _currentUser = Substitute.For<ICurrentUserSession>();
    private readonly IGhiNhatKy _ghiNhatKy = Substitute.For<IGhiNhatKy>();
    private readonly IDoiMatKhauView _view = Substitute.For<IDoiMatKhauView>();
    private NguoiDung _nguoiDung = null!;

    private DoiMatKhauPresenter TaoPresenter(bool batBuoc)
    {
        var clock = new FakeClock(new DateTime(2026, 10, 2, 9, 0, 0));
        var ghiNhanSai = new GhiNhanDangNhapSaiService(_store, clock, new DangNhapOptions(), _ghiNhatKy);
        var doiMatKhau = new DoiMatKhauService(_store, _hasher, _currentUser, _ghiNhatKy, ghiNhanSai);
        var dangNhap = new DangNhapService(_store, _hasher, _currentUser, _ghiNhatKy, clock, ghiNhanSai);
        return new DoiMatKhauPresenter(_view, ScopeFactoryGia.Tao(doiMatKhau, dangNhap), batBuoc);
    }

    private void SeedNguoiDung()
    {
        _currentUser.NguoiDungId.Returns(7);
        _nguoiDung = new NguoiDung { Id = 7, TenDangNhap = "luuky", MatKhauHash = "hash" };
        _store.TimTheoIdAsync(7, Arg.Any<CancellationToken>()).Returns(_nguoiDung);
        _store.LuuAsync(_nguoiDung, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _view.MatKhauHienTai.Returns("LuuKy@2026");
        _view.MatKhauMoi.Returns("Moi@2026a");
        _view.XacNhan.Returns("Moi@2026a");
    }

    [Fact]
    public async Task Save_WithAValidChange_ClosesWithOk()
    {
        SeedNguoiDung();
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);
        _hasher.Hash("Moi@2026a").Returns("hash-moi");
        var daDong = new TaskCompletionSource();
        _view.When(v => v.DongVoiKetQua(Arg.Any<bool>())).Do(_ => daDong.TrySetResult());

        TaoPresenter(batBuoc: false);
        _view.LuuClicked += Raise.Event();
        await daDong.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).DongVoiKetQua(true);
        _view.DidNotReceive().HienLoi(Arg.Any<string>());
    }

    [Fact]
    public async Task Save_WithAPolicyViolation_ShowsTheMessagesAndStaysOpen()
    {
        SeedNguoiDung();
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);
        _view.MatKhauMoi.Returns("abc");
        _view.XacNhan.Returns("abc");
        var daBao = new TaskCompletionSource();
        _view.When(v => v.HienLoi(Arg.Any<string>())).Do(_ => daBao.TrySetResult());

        TaoPresenter(batBuoc: false);
        _view.LuuClicked += Raise.Event();
        await daBao.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).HienLoi(Arg.Is<string>(s => s.Contains(ChinhSachMatKhau.LoiQuaNgan)));
        _view.DidNotReceive().DongVoiKetQua(Arg.Any<bool>());
        await _store.DidNotReceive().LuuAsync(Arg.Any<NguoiDung>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancel_InForcedMode_SignsOut()
    {
        SeedNguoiDung();
        _currentUser.TenDangNhap.Returns("luuky");
        var daDong = new TaskCompletionSource();
        _view.When(v => v.DongVoiKetQua(Arg.Any<bool>())).Do(_ => daDong.TrySetResult());

        TaoPresenter(batBuoc: true);
        _view.HuyClicked += Raise.Event();
        await daDong.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).DongVoiKetQua(false);
        _currentUser.Received(1).DangXuat();
    }

    [Fact]
    public async Task Cancel_InVoluntaryMode_KeepsTheSession()
    {
        SeedNguoiDung();
        var daDong = new TaskCompletionSource();
        _view.When(v => v.DongVoiKetQua(Arg.Any<bool>())).Do(_ => daDong.TrySetResult());

        TaoPresenter(batBuoc: false);
        _view.HuyClicked += Raise.Event();
        await daDong.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).DongVoiKetQua(false);
        _currentUser.DidNotReceive().DangXuat();
    }
}
