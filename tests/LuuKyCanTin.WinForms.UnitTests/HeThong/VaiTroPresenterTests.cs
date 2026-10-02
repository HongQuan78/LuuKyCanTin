using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.WinForms.HeThong;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.HeThong;

public class VaiTroPresenterTests
{
    private static readonly byte[] RowVer = [1, 2, 3];

    private readonly IVaiTroView _view = Substitute.For<IVaiTroView>();
    private readonly IVaiTroService _service = Substitute.For<IVaiTroService>();

    private VaiTroPresenter TaoPresenter() => new(_view, ScopeFactoryGia.Tao(_service));

    private void SeedMotVaiTro(params string[] maQuyen)
    {
        _service.LayDanhSachAsync(Arg.Any<CancellationToken>())
            .Returns([new VaiTroDto(1, MaVaiTro.QuanTri, "Quản trị hệ thống", RowVer)]);
        _view.VaiTroDangChon.Returns(1);
        _service.LayQuyenCuaVaiTroAsync(1, Arg.Any<CancellationToken>()).Returns(maQuyen);
    }

    private async Task TaiXongAsync()
    {
        var daTai = new TaskCompletionSource();
        _view.When(v => v.HienQuyen(Arg.Any<IReadOnlyList<string>>())).Do(_ => daTai.TrySetResult());
        _view.Loaded += Raise.Event();
        await daTai.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Loaded_ShowsTheRoleListAndItsPermissions()
    {
        SeedMotVaiTro(MaQuyen.HT.Xem, MaQuyen.DM.Them);

        TaoPresenter();
        await TaiXongAsync();

        _view.Received(1).HienDanhSachVaiTro(Arg.Any<IReadOnlyList<VaiTroDto>>());
        _view.Received(1).HienQuyen(Arg.Is<IReadOnlyList<string>>(
            q => q.Count == 2 && q.Contains(MaQuyen.DM.Them)));
    }

    [Fact]
    public async Task Save_SendsTheTickedCodesAndConfirms()
    {
        SeedMotVaiTro(MaQuyen.HT.Xem);
        _view.QuyenDaChon.Returns([MaQuyen.LKBC.Xem, MaQuyen.LKBC.In]);
        var daLuu = new TaskCompletionSource();
        _view.When(v => v.HienThongBao(Arg.Is<string>(s => s.Contains("Đã lưu")))).Do(_ => daLuu.TrySetResult());

        TaoPresenter();
        await TaiXongAsync();
        _view.LuuClicked += Raise.Event();
        await daLuu.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).CapNhatQuyenAsync(
            1,
            Arg.Is<IReadOnlyCollection<string>>(c => c.Count == 2 && c.Contains(MaQuyen.LKBC.In)),
            RowVer,
            Arg.Any<CancellationToken>());
        _view.Received(1).HienThongBao(Arg.Is<string>(s => s.Contains("Đã lưu")));
    }

    [Fact]
    public async Task Save_WithoutPermission_ShowsTheError()
    {
        SeedMotVaiTro(MaQuyen.HT.Xem);
        _service.CapNhatQuyenAsync(1, Arg.Any<IReadOnlyCollection<string>>(), RowVer, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new KhongCoQuyenException(MaQuyen.HT.Sua)));
        var daBao = new TaskCompletionSource();
        _view.When(v => v.HienLoi(Arg.Any<string>())).Do(_ => daBao.TrySetResult());

        TaoPresenter();
        await TaiXongAsync();
        _view.LuuClicked += Raise.Event();
        await daBao.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).HienLoi("Bạn không có quyền thực hiện thao tác này");
    }

    [Fact]
    public async Task Save_OnAStaleRowVersion_ShowsTheConflict()
    {
        SeedMotVaiTro(MaQuyen.HT.Xem);
        _service.CapNhatQuyenAsync(1, Arg.Any<IReadOnlyCollection<string>>(), RowVer, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new XungDotDuLieuException(new Exception("row version"))));
        var daBao = new TaskCompletionSource();
        _view.When(v => v.HienLoi(Arg.Any<string>())).Do(_ => daBao.TrySetResult());

        TaoPresenter();
        await TaiXongAsync();
        _view.LuuClicked += Raise.Event();
        await daBao.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).HienLoi(XungDotDuLieuException.ThongBao);
    }
}
