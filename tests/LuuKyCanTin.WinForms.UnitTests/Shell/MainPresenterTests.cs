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

    [Fact]
    public void Loaded_SetsTitleFromOptions()
    {
        _ = new MainPresenter(_view, Options.Create(new AppOptions { TieuDe = "Lưu ký – Căn tin" }));

        _view.Loaded += Raise.Event();

        _view.Received(1).TieuDe = "Lưu ký – Căn tin";
    }

    [Fact]
    public void BeforeLoaded_DoesNotTouchView()
    {
        _ = new MainPresenter(_view, Options.Create(new AppOptions { TieuDe = "X" }));

        _view.DidNotReceive().TieuDe = Arg.Any<string>();
    }
}
