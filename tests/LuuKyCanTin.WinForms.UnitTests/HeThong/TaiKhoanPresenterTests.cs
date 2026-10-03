using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.WinForms.HeThong;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.HeThong;

public class TaiKhoanPresenterTests
{
    private readonly ITaiKhoanView _view = Substitute.For<ITaiKhoanView>();
    private readonly ITaiKhoanService _service = Substitute.For<ITaiKhoanService>();
    private readonly List<string> _matKhauTam = [];

    private static TaiKhoanDto TaiKhoan(bool dangHoatDong) =>
        new(1, "thuquy", "Nguyễn Văn A", "Cán bộ theo dõi tiền lưu ký", [2], dangHoatDong, false, true);

    private TaiKhoanPresenter TaoPresenter() =>
        new(
            _view,
            ScopeFactoryGia.Tao(_service),
            () => Substitute.For<ITaoTaiKhoanView>(),
            () => Substitute.For<ITaiKhoanVaiTroView>(),
            _matKhauTam.Add);

    private async Task TaiXongAsync()
    {
        var daTai = new TaskCompletionSource();
        _view.When(v => v.HienDanhSach(Arg.Any<IReadOnlyList<TaiKhoanDto>>())).Do(_ => daTai.TrySetResult());
        _view.Loaded += Raise.Event();
        await daTai.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Loaded_ShowsTheAccountList()
    {
        _service.LayDanhSachAsync(Arg.Any<CancellationToken>()).Returns([TaiKhoan(dangHoatDong: true)]);

        TaoPresenter();
        await TaiXongAsync();

        _view.Received(1).HienDanhSach(Arg.Is<IReadOnlyList<TaiKhoanDto>>(d => d.Count == 1));
    }

    [Fact]
    public async Task ResetPassword_Confirmed_ShowsTheTemporaryPasswordOnce()
    {
        _service.LayDanhSachAsync(Arg.Any<CancellationToken>()).Returns([TaiKhoan(dangHoatDong: true)]);
        _service.DatLaiMatKhauAsync(1, Arg.Any<CancellationToken>()).Returns("Abc234Def567");
        _view.TaiKhoanDangChon.Returns(TaiKhoan(dangHoatDong: true));
        _view.CoDongY(Arg.Any<string>()).Returns(true);
        var daXong = new TaskCompletionSource();
        _view.When(v => v.HienThongBao(Arg.Is<string>(s => s.Contains("Đã đặt lại")))).Do(_ => daXong.TrySetResult());

        TaoPresenter();
        await TaiXongAsync();
        _view.DatLaiMatKhauClicked += Raise.Event();
        await daXong.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).DatLaiMatKhauAsync(1, Arg.Any<CancellationToken>());
        _matKhauTam.ShouldBe(["Abc234Def567"]);
    }

    [Fact]
    public async Task ResetPassword_Declined_WritesNothing()
    {
        _service.LayDanhSachAsync(Arg.Any<CancellationToken>()).Returns([TaiKhoan(dangHoatDong: true)]);
        _view.TaiKhoanDangChon.Returns(TaiKhoan(dangHoatDong: true));
        _view.CoDongY(Arg.Any<string>()).Returns(false);

        TaoPresenter();
        await TaiXongAsync();
        _view.DatLaiMatKhauClicked += Raise.Event();

        await _service.DidNotReceive().DatLaiMatKhauAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        _matKhauTam.ShouldBeEmpty();
    }

    [Fact]
    public async Task Deactivate_Confirmed_CallsTheService()
    {
        _service.LayDanhSachAsync(Arg.Any<CancellationToken>()).Returns([TaiKhoan(dangHoatDong: true)]);
        _view.TaiKhoanDangChon.Returns(TaiKhoan(dangHoatDong: true));
        _view.CoDongY(Arg.Any<string>()).Returns(true);
        var daXong = new TaskCompletionSource();
        _view.When(v => v.HienThongBao(Arg.Is<string>(s => s.Contains("Đã ngừng")))).Do(_ => daXong.TrySetResult());

        TaoPresenter();
        await TaiXongAsync();
        _view.NgungKichHoatClicked += Raise.Event();
        await daXong.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).NgungHoatDongAsync(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reactivate_OnAnInactiveAccount_CallsTheService()
    {
        _service.LayDanhSachAsync(Arg.Any<CancellationToken>()).Returns([TaiKhoan(dangHoatDong: false)]);
        _view.TaiKhoanDangChon.Returns(TaiKhoan(dangHoatDong: false));
        var daXong = new TaskCompletionSource();
        _view.When(v => v.HienThongBao(Arg.Is<string>(s => s.Contains("Đã kích hoạt")))).Do(_ => daXong.TrySetResult());

        TaoPresenter();
        await TaiXongAsync();
        _view.NgungKichHoatClicked += Raise.Event();
        await daXong.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).KichHoatLaiAsync(1, Arg.Any<CancellationToken>());
        await _service.DidNotReceive().NgungHoatDongAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unlock_DoesNotAskForConfirmation()
    {
        _service.LayDanhSachAsync(Arg.Any<CancellationToken>()).Returns([TaiKhoan(dangHoatDong: true)]);
        _view.TaiKhoanDangChon.Returns(TaiKhoan(dangHoatDong: true));
        var daXong = new TaskCompletionSource();
        _view.When(v => v.HienThongBao(Arg.Is<string>(s => s.Contains("Đã mở khoá")))).Do(_ => daXong.TrySetResult());

        TaoPresenter();
        await TaiXongAsync();
        _view.MoKhoaClicked += Raise.Event();
        await daXong.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).MoKhoaAsync(1, Arg.Any<CancellationToken>());
        _view.DidNotReceive().CoDongY(Arg.Any<string>());
    }
}
