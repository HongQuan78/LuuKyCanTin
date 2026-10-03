using FluentValidation.Results;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

public class FacilityInfoPresenterTests
{
    private static readonly byte[] RowVer = [1, 2, 3];
    private static readonly FacilityInfoDto Stored = new("CÔNG AN TỈNH ABC", "TRẠI TẠM GIAM ABC", "Xã ABC", true, RowVer);

    private readonly IFacilityInfoView _view = Substitute.For<IFacilityInfoView>();
    private readonly IFacilityInfoService _service = Substitute.For<IFacilityInfoService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private FacilityInfoPresenter CreatePresenter() => new(_view, FakeScopeFactory.Create(_service), _currentUser);

    private void GrantUpdate() => _currentUser.HasPermission(PermissionCodes.Administration.Update).Returns(true);

    private void SeedStored() => _service.GetAsync(Arg.Any<CancellationToken>()).Returns(Stored);

    private async Task WaitForLoadAsync()
    {
        var loaded = new TaskCompletionSource();
        _view.When(v => v.ShowFacility(Arg.Any<FacilityInfoDto>())).Do(_ => loaded.TrySetResult());
        _view.Loaded += Raise.Event();
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

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
        GrantUpdate();

        CreatePresenter();

        _view.Received(1).SetEditingEnabled(true);
    }

    [Fact]
    public void Constructor_DisablesSaveUntilTheStoredRowHasLoaded()
    {
        GrantUpdate();

        CreatePresenter();

        _view.Received(1).SetSaveEnabled(false);
        _view.DidNotReceive().SetSaveEnabled(true);
    }

    [Fact]
    public async Task Loaded_ShowsTheStoredHeaderAndEnablesSave()
    {
        GrantUpdate();
        SeedStored();

        CreatePresenter();
        await WaitForLoadAsync();

        _view.Received(1).ShowFacility(Stored);
        _view.Received(1).SetSaveEnabled(true);
    }

    [Fact]
    public async Task Loaded_DatabaseFailure_ShowsTheErrorAndKeepsSaveDisabled()
    {
        GrantUpdate();
        _service.GetAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("db down"));
        var errorShown = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => errorShown.TrySetResult());

        CreatePresenter();
        _view.Loaded += Raise.Event();
        await errorShown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowError(Arg.Is<string>(s => s.Contains("Không tải được") && s.Contains("db down")));
        _view.DidNotReceive().SetSaveEnabled(true);
    }

    [Fact]
    public async Task Save_SendsTheLoadedRowVersionAndConfirms()
    {
        GrantUpdate();
        SeedStored();
        _view.ParentAgencyName.Returns("CÔNG AN TỈNH ABC");
        _view.FacilityName.Returns("TRẠI TẠM GIAM MỚI");
        _view.Address.Returns("Xã ABC");
        var saved = new TaskCompletionSource();
        _view.When(v => v.ShowMessage(Arg.Is<string>(s => s.Contains("Đã lưu")))).Do(_ => saved.TrySetResult());

        CreatePresenter();
        await WaitForLoadAsync();
        _view.SaveClicked += Raise.Event();
        await saved.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).SaveAsync(
            Arg.Is<SaveFacilityInfoRequest>(r =>
                r.FacilityName == "TRẠI TẠM GIAM MỚI" && r.Address == "Xã ABC" && r.RowVer == RowVer),
            Arg.Any<CancellationToken>());
        _view.Received(1).ShowMessage(Arg.Is<string>(s => s.Contains("Đã lưu")));
    }

    [Fact]
    public async Task Save_InvalidFields_FromTheRealValidator_AreShownUnderTheirField()
    {
        GrantUpdate();
        SeedStored();
        var validation = new SaveFacilityInfoRequestValidator().Validate(new SaveFacilityInfoRequest(null, "", "", RowVer));
        _service.SaveAsync(Arg.Any<SaveFacilityInfoRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestValidationException(validation.Errors));
        var shown = new TaskCompletionSource();
        _view.When(v => v.ShowFieldErrors(Arg.Any<IReadOnlyList<FieldMessage<FacilityInfoField>>>())).Do(_ => shown.TrySetResult());

        CreatePresenter();
        await WaitForLoadAsync();
        _view.SaveClicked += Raise.Event();
        await shown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowFieldErrors(Arg.Is<IReadOnlyList<FieldMessage<FacilityInfoField>>>(e => e.SequenceEqual(
            new FieldMessage<FacilityInfoField>[]
            {
                new(FacilityInfoField.FacilityName, SaveFacilityInfoRequestValidator.FacilityNameRequiredMessage),
                new(FacilityInfoField.Address, SaveFacilityInfoRequestValidator.AddressRequiredMessage),
            })));
    }

    [Fact]
    public async Task Save_OnAStaleRowVersion_ShowsTheConflict()
    {
        GrantUpdate();
        SeedStored();
        _service.SaveAsync(Arg.Any<SaveFacilityInfoRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConcurrencyConflictException(new InvalidOperationException("row version")));
        var errorShown = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => errorShown.TrySetResult());

        CreatePresenter();
        await WaitForLoadAsync();
        _view.SaveClicked += Raise.Event();
        await errorShown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowError(ConcurrencyConflictException.ConflictMessage);
    }

    [Fact]
    public async Task Save_UnexpectedFailure_IsShownInTheBanner()
    {
        GrantUpdate();
        SeedStored();
        _service.SaveAsync(Arg.Any<SaveFacilityInfoRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));
        var errorShown = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => errorShown.TrySetResult());

        CreatePresenter();
        await WaitForLoadAsync();
        _view.SaveClicked += Raise.Event();
        await errorShown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowError("Không lưu được: boom");
    }
}
