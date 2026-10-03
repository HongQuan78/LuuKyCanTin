using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

public class SignatoryConfigurationPresenterTests
{
    private readonly ISignatoryConfigurationView _view = Substitute.For<ISignatoryConfigurationView>();
    private readonly ISignatoryConfigurationService _service = Substitute.For<ISignatoryConfigurationService>();
    private readonly IOfficerService _officerService = Substitute.For<IOfficerService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private SignatoryConfigurationPresenter CreatePresenter() =>
        new(_view, FakeScopeFactory.Create(_service, _officerService), _currentUser);

    private void SeedLines(params SignatoryLine[] lines) => _view.Lines.Returns(lines.ToList());

    private static bool Matches(IReadOnlyList<SignatoryLine> actual, SignatoryLine[] expected) =>
        actual.SequenceEqual(expected);

    [Fact]
    public void Constructor_WithoutAdministrationUpdate_OpensReadOnly()
    {
        _currentUser.HasPermission(Arg.Any<string>()).Returns(false);

        CreatePresenter();

        _view.Received(1).SetEditingEnabled(false);
    }

    [Fact]
    public void Constructor_WithAdministrationUpdate_EnablesEditing()
    {
        _currentUser.HasPermission(PermissionCodes.Administration.Update).Returns(true);

        CreatePresenter();

        _view.Received(1).SetEditingEnabled(true);
    }

    [Fact]
    public void Constructor_DisablesSaveUntilTheRowsHaveLoaded()
    {
        _currentUser.HasPermission(PermissionCodes.Administration.Update).Returns(true);

        CreatePresenter();

        _view.Received(1).SetSaveEnabled(false);
        _view.DidNotReceive().SetSaveEnabled(true);
    }

    [Fact]
    public async Task Loaded_ShowsTheTemplatesAndLoadsTheFirstOne()
    {
        SignatoryRowDto[] rows = [new(1, "Người gửi", null, null, IsOfficerActive: false)];
        OfficerDto[] officers = [new(1, "CB001", "Nguyễn Văn A", "Cán bộ", IsSupervisingOfficer: false, IsActive: true, [1])];
        _view.SelectedTemplateCode.Returns(TemplateCodes.DepositReceipt);
        _service.GetByTemplateAsync(TemplateCodes.DepositReceipt, Arg.Any<CancellationToken>()).Returns(rows);
        _officerService.GetActiveOfficersAsync(false, Arg.Any<CancellationToken>()).Returns(officers);
        var loaded = new TaskCompletionSource();
        _view.When(v => v.ShowSignatories(Arg.Any<IReadOnlyList<SignatoryRowDto>>(), Arg.Any<IReadOnlyList<OfficerDto>>()))
            .Do(_ => loaded.TrySetResult());

        CreatePresenter();
        _view.Loaded += Raise.Event();
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowTemplates(TemplateCodes.All);
        _view.Received(1).ShowSignatories(
            Arg.Is<IReadOnlyList<SignatoryRowDto>>(items => items.Count == 1 && items[0].Title == "Người gửi"),
            Arg.Is<IReadOnlyList<OfficerDto>>(items => items.Count == 1 && items[0].FullName == "Nguyễn Văn A"));
        _view.Received(1).SetSaveEnabled(true);
    }

    [Fact]
    public async Task TemplateChanged_ClearsTheMessageAndLoadsTheNewTemplate()
    {
        _view.SelectedTemplateCode.Returns(TemplateCodes.Payout);
        _service.GetByTemplateAsync(TemplateCodes.Payout, Arg.Any<CancellationToken>()).Returns([]);
        _officerService.GetActiveOfficersAsync(false, Arg.Any<CancellationToken>()).Returns([]);
        var loaded = new TaskCompletionSource();
        _view.When(v => v.ShowSignatories(Arg.Any<IReadOnlyList<SignatoryRowDto>>(), Arg.Any<IReadOnlyList<OfficerDto>>()))
            .Do(_ => loaded.TrySetResult());

        CreatePresenter();
        _view.TemplateChanged += Raise.Event();
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowMessage("");
        await _service.Received(1).GetByTemplateAsync(TemplateCodes.Payout, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Discard_ClearsTheMessageAndReloadsTheSelectedTemplate()
    {
        _view.SelectedTemplateCode.Returns(TemplateCodes.Payout);
        _service.GetByTemplateAsync(TemplateCodes.Payout, Arg.Any<CancellationToken>()).Returns([]);
        _officerService.GetActiveOfficersAsync(false, Arg.Any<CancellationToken>()).Returns([]);
        var loaded = new TaskCompletionSource();
        _view.When(v => v.ShowSignatories(Arg.Any<IReadOnlyList<SignatoryRowDto>>(), Arg.Any<IReadOnlyList<OfficerDto>>()))
            .Do(_ => loaded.TrySetResult());

        CreatePresenter();
        _view.DiscardClicked += Raise.Event();
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowMessage("");
        await _service.Received(1).GetByTemplateAsync(TemplateCodes.Payout, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Load_Failure_ClearsTheGridDisablesSaveAndShowsTheError()
    {
        _view.SelectedTemplateCode.Returns(TemplateCodes.Payout);
        _service.GetByTemplateAsync(TemplateCodes.Payout, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("db down"));
        var failed = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => failed.TrySetResult());

        CreatePresenter();
        _view.ClearReceivedCalls();
        _view.TemplateChanged += Raise.Event();
        await failed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowSignatories(
            Arg.Is<IReadOnlyList<SignatoryRowDto>>(items => items.Count == 0),
            Arg.Is<IReadOnlyList<OfficerDto>>(items => items.Count == 0));
        _view.Received(1).SetSaveEnabled(false);
        _view.Received(1).ShowError(Arg.Is<string>(s => s.Contains("Không tải được") && s.Contains("db down")));
    }

    [Fact]
    public async Task Save_WhenTheReloadFails_ShowsTheLoadErrorAndNotTheSuccessMessage()
    {
        _view.SelectedTemplateCode.Returns(TemplateCodes.Payout);
        SeedLines(new SignatoryLine("A"));
        _service.GetByTemplateAsync(TemplateCodes.Payout, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("db down"));
        var failed = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => failed.TrySetResult());

        CreatePresenter();
        _view.SaveClicked += Raise.Event();
        await failed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.DidNotReceive().ShowMessage(Arg.Is<string>(s => s.Contains("Đã lưu")));
        _view.Received(1).ShowError(Arg.Is<string>(s => s.Contains("Không tải được")));
    }

    [Fact]
    public async Task TemplateSwitch_DuringAnInFlightLoad_DoesNotBindTheStaleRows()
    {
        var firstLoad = new TaskCompletionSource<IReadOnlyList<SignatoryRowDto>>();
        var secondLoad = new TaskCompletionSource<IReadOnlyList<SignatoryRowDto>>();
        _view.SelectedTemplateCode.Returns(TemplateCodes.DepositReceipt);
        _service.GetByTemplateAsync(TemplateCodes.DepositReceipt, Arg.Any<CancellationToken>()).Returns(firstLoad.Task);
        _service.GetByTemplateAsync(TemplateCodes.Payout, Arg.Any<CancellationToken>()).Returns(secondLoad.Task);
        _officerService.GetActiveOfficersAsync(false, Arg.Any<CancellationToken>()).Returns([]);

        CreatePresenter();
        _view.Loaded += Raise.Event();

        // The user switches while the first load is still in flight.
        _view.SelectedTemplateCode.Returns(TemplateCodes.Payout);
        _view.TemplateChanged += Raise.Event();

        var bound = new TaskCompletionSource();
        _view.When(v => v.ShowSignatories(Arg.Any<IReadOnlyList<SignatoryRowDto>>(), Arg.Any<IReadOnlyList<OfficerDto>>()))
            .Do(_ => bound.TrySetResult());

        firstLoad.SetResult([new SignatoryRowDto(1, "Người gửi", null, null, IsOfficerActive: false)]);
        secondLoad.SetResult([new SignatoryRowDto(1, "Lãnh đạo đơn vị", null, null, IsOfficerActive: false)]);
        await bound.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowSignatories(
            Arg.Is<IReadOnlyList<SignatoryRowDto>>(items => items.Count == 1 && items[0].Title == "Lãnh đạo đơn vị"),
            Arg.Any<IReadOnlyList<OfficerDto>>());
        _view.DidNotReceive().ShowSignatories(
            Arg.Is<IReadOnlyList<SignatoryRowDto>>(items => items.Count == 1 && items[0].Title == "Người gửi"),
            Arg.Any<IReadOnlyList<OfficerDto>>());
    }

    [Fact]
    public void AddLine_AppendsABlankRowAndSelectsIt()
    {
        SeedLines(new SignatoryLine("Cán bộ căn tin"));

        CreatePresenter();
        _view.AddLineClicked += Raise.Event();

        SignatoryLine[] expected = [new("Cán bộ căn tin"), new("")];
        _view.Received(1).ShowLines(
            Arg.Is<IReadOnlyList<SignatoryLine>>(lines => Matches(lines, expected)),
            1);
    }

    [Fact]
    public void MoveUp_SelectedRow_SwapsWithTheRowAboveAndKeepsItSelected()
    {
        SeedLines(new SignatoryLine("A"), new SignatoryLine("B"), new SignatoryLine("C"));
        _view.SelectedLineIndex.Returns(1);

        CreatePresenter();
        _view.MoveUpClicked += Raise.Event();

        SignatoryLine[] expected = [new("B"), new("A"), new("C")];
        _view.Received(1).ShowLines(
            Arg.Is<IReadOnlyList<SignatoryLine>>(lines => Matches(lines, expected)),
            0);
    }

    [Fact]
    public void MoveUp_FirstRow_DoesNothing()
    {
        SeedLines(new SignatoryLine("A"), new SignatoryLine("B"));
        _view.SelectedLineIndex.Returns(0);

        CreatePresenter();
        _view.MoveUpClicked += Raise.Event();

        _view.DidNotReceive().ShowLines(Arg.Any<IReadOnlyList<SignatoryLine>>(), Arg.Any<int>());
    }

    [Fact]
    public void MoveUp_NoSelection_DoesNothing()
    {
        SeedLines(new SignatoryLine("A"), new SignatoryLine("B"));
        _view.SelectedLineIndex.Returns((int?)null);

        CreatePresenter();
        _view.MoveUpClicked += Raise.Event();

        _view.DidNotReceive().ShowLines(Arg.Any<IReadOnlyList<SignatoryLine>>(), Arg.Any<int>());
    }

    [Fact]
    public void MoveDown_SelectedRow_SwapsWithTheRowBelowAndKeepsItSelected()
    {
        SeedLines(new SignatoryLine("A"), new SignatoryLine("B"), new SignatoryLine("C"));
        _view.SelectedLineIndex.Returns(1);

        CreatePresenter();
        _view.MoveDownClicked += Raise.Event();

        SignatoryLine[] expected = [new("A"), new("C"), new("B")];
        _view.Received(1).ShowLines(
            Arg.Is<IReadOnlyList<SignatoryLine>>(lines => Matches(lines, expected)),
            2);
    }

    [Fact]
    public void MoveDown_LastRow_DoesNothing()
    {
        SeedLines(new SignatoryLine("A"), new SignatoryLine("B"));
        _view.SelectedLineIndex.Returns(1);

        CreatePresenter();
        _view.MoveDownClicked += Raise.Event();

        _view.DidNotReceive().ShowLines(Arg.Any<IReadOnlyList<SignatoryLine>>(), Arg.Any<int>());
    }

    [Fact]
    public async Task RemoveLine_LastRemainingRow_IsBlockedWithTheAcMessageAndWritesNothing()
    {
        SeedLines(new SignatoryLine("Người gửi"));
        _view.SelectedLineIndex.Returns(0);

        CreatePresenter();
        _view.RemoveLineClicked += Raise.Event();

        _view.Received(1).ShowError(SignatoryConfigurationService.AtLeastOneSignatoryMessage);
        _view.DidNotReceive().ShowLines(Arg.Any<IReadOnlyList<SignatoryLine>>(), Arg.Any<int>());
        await _service.DidNotReceive().SaveAsync(
            Arg.Any<string>(), Arg.Any<IReadOnlyList<SignatoryLine>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RemoveLine_WithAnotherRowLeft_RemovesTheSelectedRow()
    {
        SeedLines(new SignatoryLine("A"), new SignatoryLine("B"));
        _view.SelectedLineIndex.Returns(0);

        CreatePresenter();
        _view.RemoveLineClicked += Raise.Event();

        SignatoryLine[] expected = [new("B")];
        _view.Received(1).ShowLines(
            Arg.Is<IReadOnlyList<SignatoryLine>>(lines => Matches(lines, expected)),
            0);
    }

    [Fact]
    public async Task Save_SendsTheEditedLinesAndConfirms()
    {
        _view.SelectedTemplateCode.Returns(TemplateCodes.Payout);
        SeedLines(new SignatoryLine("A", 5), new SignatoryLine("B"));
        _service.GetByTemplateAsync(TemplateCodes.Payout, Arg.Any<CancellationToken>()).Returns([]);
        _officerService.GetActiveOfficersAsync(false, Arg.Any<CancellationToken>()).Returns([]);
        var confirmed = new TaskCompletionSource();
        _view.When(v => v.ShowMessage(Arg.Is<string>(s => s.Contains("Đã lưu")))).Do(_ => confirmed.TrySetResult());

        CreatePresenter();
        _view.SaveClicked += Raise.Event();
        await confirmed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        SignatoryLine[] expected = [new("A", 5), new("B")];
        await _service.Received(1).SaveAsync(
            TemplateCodes.Payout,
            Arg.Is<IReadOnlyList<SignatoryLine>>(lines => Matches(lines, expected)),
            Arg.Any<CancellationToken>());
        _view.Received(1).ShowMessage(Arg.Is<string>(s => s.Contains("Đã lưu")));
    }
}
