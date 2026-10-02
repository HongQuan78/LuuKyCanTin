using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

public class LoginPresenterTests
{
    private readonly INguoiDungStore _store = Substitute.For<INguoiDungStore>();
    private readonly IMatKhauHasher _hasher = Substitute.For<IMatKhauHasher>();
    private readonly ICurrentUserSession _phien = Substitute.For<ICurrentUserSession>();
    private readonly IGhiNhatKy _ghiNhatKy = Substitute.For<IGhiNhatKy>();
    private readonly ILoginView _view = Substitute.For<ILoginView>();

    private LoginPresenter TaoPresenter()
    {
        var service = new DangNhapService(_store, _hasher, _phien, _ghiNhatKy);
        return new LoginPresenter(_view, ScopeFactoryGia.Tao(service));
    }

    [Fact]
    public async Task Success_ClosesTheViewAsOk()
    {
        _store.TimTheoDangNhapAsync("admin", Arg.Any<CancellationToken>()).Returns(new NguoiDung
        {
            Id = 1,
            TenDangNhap = "admin",
            MatKhauHash = "hash",
            DangHoatDong = true,
        });
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);
        _view.TenDangNhap.Returns("admin");
        _view.MatKhau.Returns("LuuKy@2026");

        await TaoPresenter().DangNhapAsync();

        _view.Received(1).DongVoiKetQua(true);
        _view.DidNotReceive().HienLoi(Arg.Any<string>());
    }

    [Fact]
    public async Task Failure_ShowsTheErrorAndKeepsTheFormOpen()
    {
        _store.TimTheoDangNhapAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((NguoiDung?)null);
        _view.TenDangNhap.Returns("admin");
        _view.MatKhau.Returns("sai");

        await TaoPresenter().DangNhapAsync();

        _view.Received(1).HienLoi(DangNhapService.SaiThongTin);
        _view.DidNotReceive().DongVoiKetQua(Arg.Any<bool>());
    }
}
