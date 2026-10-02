using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.WinForms.DanhMuc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.DanhMuc;

public class CanBoPresenterTests
{
    private static readonly CanBoDto An = new(1, "CB01", "Nguyễn Văn An", null, true, true, [1]);

    private readonly ICanBoView _view = Substitute.For<ICanBoView>();
    private readonly ICanBoEditView _dialog = Substitute.For<ICanBoEditView>();
    private readonly ICanBoService _service = Substitute.For<ICanBoService>();
    private readonly IServiceScopeFactory _scopes;
    private int _dialogsOpened;

    public CanBoPresenterTests()
    {
        _scopes = new ServiceCollection().AddScoped(_ => _service).BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        _service.TimAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([An]);
    }

    private CanBoPresenter NewPresenter() => new(_view, _scopes, () =>
    {
        _dialogsOpened++;
        return _dialog;
    });

    [Fact]
    public void Loaded_ShowsActiveStaffMatchingTheKeyword()
    {
        _view.TuKhoa.Returns("nguyen");
        _view.HienCaNguoiDaNghi.Returns(false);
        NewPresenter();

        _view.Loaded += Raise.Event();

        _service.Received(1).TimAsync("nguyen", false, Arg.Any<CancellationToken>());
        _view.Received(1).HienDanhSach(Arg.Is<IReadOnlyList<CanBoDto>>(l => l.Single() == An));
    }

    [Fact]
    public void ChangingTheFilter_SearchesAgain_IncludingStaffWhoLeft()
    {
        NewPresenter();
        _view.TuKhoa.Returns("an");
        _view.HienCaNguoiDaNghi.Returns(true);

        _view.TimKiemThayDoi += Raise.Event();

        _service.Received(1).TimAsync("an", true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Them_OpensAnEmptyDialog_AndReloadsAfterASave()
    {
        _dialog.HienThi().Returns(true);
        NewPresenter();

        _view.ThemClicked += Raise.Event();

        _dialogsOpened.ShouldBe(1);
        _dialog.Received().TieuDe = "Thêm cán bộ";
        _dialog.Received(1).Dispose();
        _view.Received(1).HienDanhSach(Arg.Any<IReadOnlyList<CanBoDto>>());
    }

    [Fact]
    public void CancelledDialog_DoesNotReload()
    {
        _dialog.HienThi().Returns(false);
        NewPresenter();

        _view.ThemClicked += Raise.Event();

        _view.DidNotReceive().HienDanhSach(Arg.Any<IReadOnlyList<CanBoDto>>());
    }

    [Fact]
    public void Sua_OpensTheSelectedStaffMember()
    {
        _view.CanBoDangChon.Returns(An);
        NewPresenter();

        _view.SuaClicked += Raise.Event();

        _dialog.Received().TieuDe = "Sửa cán bộ";
        _dialog.Received().MaCanBo = "CB01";
    }

    [Fact]
    public void Sua_WithNothingSelected_DoesNothing()
    {
        _view.CanBoDangChon.Returns((CanBoDto?)null);
        NewPresenter();

        _view.SuaClicked += Raise.Event();

        _dialogsOpened.ShouldBe(0);
    }

    [Fact]
    public async Task OnlyTheLatestSearch_IsShown()
    {
        var cham = new TaskCompletionSource<IReadOnlyList<CanBoDto>>();
        var moi = new CanBoDto(2, "CB02", "Trần Thị Mới", null, false, true, [2]);
        _service.TimAsync("ch", Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(cham.Task);
        _service.TimAsync("cho", Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([moi]);
        NewPresenter();

        _view.TuKhoa.Returns("ch");
        _view.TimKiemThayDoi += Raise.Event();
        _view.TuKhoa.Returns("cho");
        _view.TimKiemThayDoi += Raise.Event();
        cham.SetResult([An]);
        await Task.Yield();

        _view.Received(1).HienDanhSach(Arg.Any<IReadOnlyList<CanBoDto>>());
        _view.Received(1).HienDanhSach(Arg.Is<IReadOnlyList<CanBoDto>>(l => l.Single() == moi));
    }
}
