using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.WinForms.DanhMuc;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.DanhMuc;

public class ThemDoiTuongPresenterTests
{
    private readonly IDoiTuongStore _store = Substitute.For<IDoiTuongStore>();
    private readonly IThemDoiTuongView _view = Substitute.For<IThemDoiTuongView>();

    public ThemDoiTuongPresenterTests()
    {
        _store.ThemAsync(Arg.Any<DoiTuong>(), Arg.Any<CancellationToken>()).Returns(true);
        _view.MaSo.Returns("DT-0001");
        _view.HoTen.Returns("Nguyễn Văn A");
        _view.NamSinh.Returns((short)1990);
        _view.LoaiDoiTuong.Returns(LoaiDoiTuong.TamGiuTamGiam);
        _view.NgayVao.Returns(new DateOnly(2026, 9, 30));
    }

    private ThemDoiTuongPresenter TaoPresenter()
    {
        var clock = new FakeClock(new DateTime(2026, 10, 1, 8, 0, 0));
        var service = new ThemDoiTuongService(_store, clock);
        return new ThemDoiTuongPresenter(_view, ScopeFactoryGia.Tao(service));
    }

    [Fact]
    public async Task Success_ClosesTheViewAsOk()
    {
        await TaoPresenter().LuuAsync();

        _view.Received(1).DongVoiKetQua(true);
        await _store.Received(1).ThemAsync(Arg.Any<DoiTuong>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ADuplicateCode_ShowsTheFriendlyMessageAndKeepsTheFormOpen()
    {
        _store.MaSoDaTonTaiAsync("DT-0001", Arg.Any<CancellationToken>()).Returns(true);

        await TaoPresenter().LuuAsync();

        _view.Received(1).HienLoi(ThemDoiTuongService.MaSoDaTonTai);
        _view.DidNotReceive().DongVoiKetQua(Arg.Any<bool>());
    }
}
