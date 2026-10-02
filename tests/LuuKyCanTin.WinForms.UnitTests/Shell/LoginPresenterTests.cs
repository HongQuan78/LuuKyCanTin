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
    private NguoiDung? _nguoiDung;

    private LoginPresenter TaoPresenter()
    {
        var clock = new FakeClock(new DateTime(2026, 10, 2, 9, 0, 0));
        var ghiNhanSai = new GhiNhanDangNhapSaiService(_store, clock, new DangNhapOptions(), _ghiNhatKy);
        var service = new DangNhapService(_store, _hasher, _phien, _ghiNhatKy, clock, ghiNhanSai);
        return new LoginPresenter(_view, ScopeFactoryGia.Tao(service));
    }

    private void SeedTaiKhoan(bool phaiDoiMatKhau = false)
    {
        _nguoiDung = new NguoiDung
        {
            Id = 1,
            TenDangNhap = "admin",
            MatKhauHash = "hash",
            DangHoatDong = true,
            PhaiDoiMatKhau = phaiDoiMatKhau,
        };
        _store.TimTheoDangNhapAsync("admin", Arg.Any<CancellationToken>()).Returns(_nguoiDung);
        _store.LuuAsync(_nguoiDung, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _view.TenDangNhap.Returns("admin");
        _view.MatKhau.Returns("LuuKy@2026");
    }

    [Fact]
    public async Task Success_ClosesTheViewAsOk()
    {
        SeedTaiKhoan();
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);

        var presenter = TaoPresenter();
        await presenter.DangNhapAsync();

        presenter.PhaiDoiMatKhau.ShouldBeFalse();
        _view.Received(1).DongVoiKetQua(true);
        _view.DidNotReceive().HienLoi(Arg.Any<string>());
    }

    [Fact]
    public async Task ForcedChange_ClosesTheViewAsOkAndFlagsTheChange()
    {
        SeedTaiKhoan(phaiDoiMatKhau: true);
        _hasher.Verify("LuuKy@2026", "hash").Returns(true);

        var presenter = TaoPresenter();
        await presenter.DangNhapAsync();

        presenter.PhaiDoiMatKhau.ShouldBeTrue();
        _view.Received(1).DongVoiKetQua(true);
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

    [Fact]
    public async Task ALockedAccount_ShowsTheLockedMessageAndClearsThePasswordBox()
    {
        SeedTaiKhoan();
        _nguoiDung!.KhoaDen = new DateTime(2026, 10, 2, 9, 5, 0);

        await TaoPresenter().DangNhapAsync();

        _view.Received(1).HienLoi(DangNhapService.LoiTaiKhoanBiKhoa);
        _view.Received(1).XoaMatKhau();
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }
}
