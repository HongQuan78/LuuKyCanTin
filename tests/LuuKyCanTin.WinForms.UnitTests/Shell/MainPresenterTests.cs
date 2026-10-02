using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.Shell;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

// Presenters are tested against a fake View: no Form is ever created.
public class MainPresenterTests
{
    private readonly IMainView _view = Substitute.For<IMainView>();
    private readonly IDieuHuong _dieuHuong = Substitute.For<IDieuHuong>();

    private MainPresenter NewPresenter(string tieuDe = "X") =>
        new(_view, Options.Create(new AppOptions { TieuDe = tieuDe }), _dieuHuong);

    [Fact]
    public void Loaded_SetsTitleFromOptions()
    {
        NewPresenter("Lưu ký – Căn tin");

        _view.Loaded += Raise.Event();

        _view.Received(1).TieuDe = "Lưu ký – Căn tin";
    }

    [Fact]
    public void BeforeLoaded_DoesNotTouchView()
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
    public void Constructing_OpensNothing()
    {
        NewPresenter();

        _dieuHuong.ReceivedCalls().ShouldBeEmpty();
    }
}
