using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.WinForms.MasterData;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.MasterData;

public class OfficerPresenterTests
{
    private static readonly OfficerDto An = new(1, "CB01", "Nguyễn Văn An", null, true, true, [1]);

    private readonly IOfficerView _view = Substitute.For<IOfficerView>();
    private readonly IOfficerEditView _dialog = Substitute.For<IOfficerEditView>();
    private readonly IOfficerService _service = Substitute.For<IOfficerService>();
    private readonly IServiceScopeFactory _scopes;
    private int _dialogsOpened;

    public OfficerPresenterTests()
    {
        _scopes = new ServiceCollection().AddScoped(_ => _service).BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        _service.SearchAsync(Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([An]);
    }

    private OfficerPresenter NewPresenter() => new(_view, _scopes, () =>
    {
        _dialogsOpened++;
        return _dialog;
    });

    [Fact]
    public void Loaded_ShowsActiveStaffMatchingTheKeyword()
    {
        _view.Keyword.Returns("nguyen");
        _view.ShowInactive.Returns(false);
        NewPresenter();

        _view.Loaded += Raise.Event();

        _service.Received(1).SearchAsync("nguyen", false, Arg.Any<CancellationToken>());
        _view.Received(1).ShowList(Arg.Is<IReadOnlyList<OfficerDto>>(l => l.Single() == An));
    }

    [Fact]
    public void ChangingTheFilter_SearchesAgain_IncludingStaffWhoLeft()
    {
        NewPresenter();
        _view.Keyword.Returns("an");
        _view.ShowInactive.Returns(true);

        _view.SearchChanged += Raise.Event();

        _service.Received(1).SearchAsync("an", true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Add_OpensAnEmptyDialog_AndReloadsAfterASave()
    {
        _dialog.DisplayText().Returns(true);
        NewPresenter();

        _view.AddClicked += Raise.Event();

        _dialogsOpened.ShouldBe(1);
        _dialog.Received().Title = "Thêm cán bộ";
        _dialog.Received(1).Dispose();
        _view.Received(1).ShowList(Arg.Any<IReadOnlyList<OfficerDto>>());
    }

    [Fact]
    public void CancelledDialog_DoesNotReload()
    {
        _dialog.DisplayText().Returns(false);
        NewPresenter();

        _view.AddClicked += Raise.Event();

        _view.DidNotReceive().ShowList(Arg.Any<IReadOnlyList<OfficerDto>>());
    }

    [Fact]
    public void Edit_OpensTheSelectedStaffMember()
    {
        _view.SelectedOfficer.Returns(An);
        NewPresenter();

        _view.EditClicked += Raise.Event();

        _dialog.Received().Title = "Sửa cán bộ";
        _dialog.Received().OfficerCode = "CB01";
    }

    [Fact]
    public void Edit_WithNothingSelected_DoesNothing()
    {
        _view.SelectedOfficer.Returns((OfficerDto?)null);
        NewPresenter();

        _view.EditClicked += Raise.Event();

        _dialogsOpened.ShouldBe(0);
    }

    [Fact]
    public async Task OnlyTheLatestSearch_IsShown()
    {
        var slow = new TaskCompletionSource<IReadOnlyList<OfficerDto>>();
        var newValues = new OfficerDto(2, "CB02", "Trần Thị Mới", null, false, true, [2]);
        _service.SearchAsync("ch", Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(slow.Task);
        _service.SearchAsync("cho", Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([newValues]);
        NewPresenter();

        _view.Keyword.Returns("ch");
        _view.SearchChanged += Raise.Event();
        _view.Keyword.Returns("cho");
        _view.SearchChanged += Raise.Event();
        slow.SetResult([An]);
        await Task.Yield();

        _view.Received(1).ShowList(Arg.Any<IReadOnlyList<OfficerDto>>());
        _view.Received(1).ShowList(Arg.Is<IReadOnlyList<OfficerDto>>(l => l.Single() == newValues));
    }
}
