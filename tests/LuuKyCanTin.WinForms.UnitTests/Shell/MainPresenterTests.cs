using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

// Presenters are tested against a fake View: no Form is ever created.
public class MainPresenterTests
{
    private readonly IMainView _view = Substitute.For<IMainView>();
    private readonly IDieuHuong _dieuHuong = Substitute.For<IDieuHuong>();
    private readonly INguoiDungStore _store = Substitute.For<INguoiDungStore>();
    private readonly IMatKhauHasher _hasher = Substitute.For<IMatKhauHasher>();
    private readonly ICurrentUserSession _phien = Substitute.For<ICurrentUserSession>();
    private readonly IGhiNhatKy _ghiNhatKy = Substitute.For<IGhiNhatKy>();

    private MainPresenter NewPresenter(string tieuDe = "X")
    {
        var clock = new FakeClock(new DateTime(2026, 10, 2, 9, 0, 0));
        var ghiNhanSai = new GhiNhanDangNhapSaiService(_store, clock, new DangNhapOptions(), _ghiNhatKy);
        var dangNhap = new DangNhapService(_store, _hasher, _phien, _ghiNhatKy, clock, ghiNhanSai);
        return new MainPresenter(
            _view, Options.Create(new AppOptions { TieuDe = tieuDe }), _dieuHuong, ScopeFactoryGia.Tao(dangNhap));
    }

    [Fact]
    public void OnLoaded_ViewLoaded_SetsTieuDeFromOptions()
    {
        NewPresenter("Tiêu đề từ cấu hình");

        _view.Loaded += Raise.Event();

        _view.Received(1).TieuDe = "Tiêu đề từ cấu hình";
    }

    [Fact]
    public void Constructor_BeforeLoaded_DoesNotTouchView()
    {
        NewPresenter();

        _view.DidNotReceive().TieuDe = Arg.Any<string>();
    }

    [Fact]
    public void StaffMenu_OpensTheStaffRegister()
    {
        NewPresenter();

        _view.DanhMucCanBoClicked += Raise.Event();

        _dieuHuong.Received(1).MoDanhMucCanBo();
    }

    [Fact]
    public void RoleMenu_OpensTheRoleScreen()
    {
        NewPresenter();

        _view.VaiTroClicked += Raise.Event();

        _dieuHuong.Received(1).MoVaiTro();
    }

    [Fact]
    public void ChangePasswordMenu_OpensTheDialog()
    {
        NewPresenter();

        _view.DoiMatKhauClicked += Raise.Event();

        _dieuHuong.Received(1).MoDoiMatKhau();
    }

    [Fact]
    public async Task SignOutMenu_ClearsTheSessionAndClosesTheShell()
    {
        _phien.NguoiDungId.Returns(3);
        _phien.TenDangNhap.Returns("admin");
        var daDong = new TaskCompletionSource();
        _view.When(v => v.Dong()).Do(_ => daDong.TrySetResult());

        var presenter = NewPresenter();
        _view.DangXuatClicked += Raise.Event();

        await daDong.Task.WaitAsync(TimeSpan.FromSeconds(5));
        presenter.DaDangXuat.ShouldBeTrue();
        _phien.Received(1).DangXuat();
        await _ghiNhatKy.Received(1).GhiAsync(
            HanhDong.DangNhap, "NguoiDung", 3, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructing_OpensNothing()
    {
        NewPresenter();

        _dieuHuong.ReceivedCalls().ShouldBeEmpty();
    }
}
