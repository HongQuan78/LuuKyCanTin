using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.WinForms.HeThong;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.HeThong;

public class TaoTaiKhoanPresenterTests
{
    private readonly ITaoTaiKhoanView _view = Substitute.For<ITaoTaiKhoanView>();
    private readonly ITaiKhoanService _taiKhoanService = Substitute.For<ITaiKhoanService>();
    private readonly IVaiTroService _vaiTroService = Substitute.For<IVaiTroService>();
    private readonly List<string> _matKhauTam = [];

    private TaoTaiKhoanPresenter TaoPresenter() =>
        new(_view, ScopeFactoryGia.Tao(_taiKhoanService, _vaiTroService), _matKhauTam.Add);

    [Fact]
    public async Task Loaded_ShowsTheStaffAndTheRoles()
    {
        var daTai = new TaskCompletionSource();
        _view.When(v => v.HienDanhSachVaiTro(Arg.Any<IReadOnlyList<VaiTroDto>>())).Do(_ => daTai.TrySetResult());

        TaoPresenter();
        _view.Loaded += Raise.Event();
        await daTai.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).HienDanhSachCanBo(Arg.Any<IReadOnlyList<CanBoDto>>());
        _view.Received(1).HienDanhSachVaiTro(Arg.Any<IReadOnlyList<VaiTroDto>>());
    }

    [Fact]
    public async Task Tao_Success_ShowsTheTemporaryPasswordAndCloses()
    {
        _view.TenDangNhap.Returns("thuquy");
        _view.CanBoId.Returns(5);
        _view.VaiTroDaChon.Returns([2]);
        _taiKhoanService.TaoAsync(Arg.Any<TaoTaiKhoanRequest>(), Arg.Any<CancellationToken>())
            .Returns(new KetQuaTaoTaiKhoan(9, "Abc234Def567"));
        var daXong = new TaskCompletionSource();
        _view.When(v => v.DongDaLuu()).Do(_ => daXong.TrySetResult());

        TaoPresenter();
        _view.TaoClicked += Raise.Event();
        await daXong.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _taiKhoanService.Received(1).TaoAsync(
            Arg.Is<TaoTaiKhoanRequest>(r => r.TenDangNhap == "thuquy" && r.CanBoId == 5 && r.VaiTroIds.Count == 1),
            Arg.Any<CancellationToken>());
        _matKhauTam.ShouldBe(["Abc234Def567"]);
    }

    [Fact]
    public async Task Tao_BusinessError_ShowsTheMessageAndStaysOpen()
    {
        _view.TenDangNhap.Returns("thuquy");
        _view.CanBoId.Returns(5);
        _view.VaiTroDaChon.Returns([2]);
        _taiKhoanService.TaoAsync(Arg.Any<TaoTaiKhoanRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<KetQuaTaoTaiKhoan>(new LoiNghiepVuException(TaiKhoanService.LoiCanBoDaCoTaiKhoan)));
        var daBao = new TaskCompletionSource();
        _view.When(v => v.HienLoi(Arg.Any<string>())).Do(_ => daBao.TrySetResult());

        TaoPresenter();
        _view.TaoClicked += Raise.Event();
        await daBao.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).HienLoi(TaiKhoanService.LoiCanBoDaCoTaiKhoan);
        _view.DidNotReceive().DongDaLuu();
        _matKhauTam.ShouldBeEmpty();
    }
}
