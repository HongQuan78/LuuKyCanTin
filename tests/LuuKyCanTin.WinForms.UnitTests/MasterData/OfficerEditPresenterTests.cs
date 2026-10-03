using FluentValidation.Results;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.MasterData;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace LuuKyCanTin.WinForms.UnitTests.MasterData;

public class OfficerEditPresenterTests
{
    private static readonly OfficerDto Existing = new(5, "CB05", "Lê Thị Bình", "Kế toán", false, true, [1, 2, 3]);

    private readonly IOfficerEditView _view = Substitute.For<IOfficerEditView>();
    private readonly IOfficerService _service = Substitute.For<IOfficerService>();
    private readonly IServiceScopeFactory _scopes;

    public OfficerEditPresenterTests()
    {
        _scopes = new ServiceCollection().AddScoped(_ => _service).BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private void Fill(string code = "cb01", string fullName = "Nguyễn Văn An", string position = "Quản giáo", bool supervising = true, bool isActive = true)
    {
        _view.OfficerCode.Returns(code);
        _view.FullName.Returns(fullName);
        _view.Position.Returns(position);
        _view.IsSupervisingOfficer.Returns(supervising);
        _view.IsActive.Returns(isActive);
    }

    private void SaveFails(Exception error) =>
        _service.AddAsync(Arg.Any<SaveOfficerRequest>(), Arg.Any<CancellationToken>()).ThrowsAsync(error);

    private static bool IsOnly(IReadOnlyList<FieldMessage<OfficerField>> errors, OfficerField field, string message) =>
        errors.Count == 1 && errors[0] == new FieldMessage<OfficerField>(field, message);

    [Fact]
    public void Adding_StartsWithAnEmptyActiveStaffMember()
    {
        _ = new OfficerEditPresenter(_view, _scopes, officer: null);

        _view.Received().Title = "Thêm cán bộ";
        _view.Received().ShowHeading("Thêm cán bộ", "Nhập thông tin cán bộ mới.");
        _view.Received().IsActive = true;
        _view.DidNotReceive().OfficerCode = Arg.Any<string>();
    }

    [Fact]
    public void Editing_ShowsTheCurrentValues()
    {
        _ = new OfficerEditPresenter(_view, _scopes, Existing);

        _view.Received().Title = "Sửa cán bộ";
        _view.Received().OfficerCode = "CB05";
        _view.Received().FullName = "Lê Thị Bình";
        _view.Received().Position = "Kế toán";
        _view.Received().IsSupervisingOfficer = false;
        _view.Received().IsActive = true;
    }

    [Fact]
    public void Editing_HeadsTheDialogWithTheNameAndCodeAndPosition()
    {
        _ = new OfficerEditPresenter(_view, _scopes, Existing);

        _view.Received().ShowHeading("Lê Thị Bình", "CB05 · Kế toán");
    }

    [Fact]
    public void Editing_SomeoneWithoutAPosition_ShowsTheCodeOnly()
    {
        _ = new OfficerEditPresenter(_view, _scopes, Existing with { Position = null });

        _view.Received().ShowHeading("Lê Thị Bình", "CB05");
    }

    [Fact]
    public void Save_WhenAdding_AddsAndCloses()
    {
        Fill();
        _ = new OfficerEditPresenter(_view, _scopes, officer: null);

        _view.SaveClicked += Raise.Event();

        _service.Received(1).AddAsync(new SaveOfficerRequest("cb01", "Nguyễn Văn An", "Quản giáo", true, true), Arg.Any<CancellationToken>());
        _view.Received(1).CloseAsSaved();
    }

    [Fact]
    public void Save_WhenEditing_SendsTheLoadedRowVersion()
    {
        _ = new OfficerEditPresenter(_view, _scopes, Existing);
        // What the user typed over the loaded values.
        Fill(code: "CB05", isActive: false);

        _view.SaveClicked += Raise.Event();

        _service.Received(1).UpdateAsync(5, Arg.Is<SaveOfficerRequest>(r => r.OfficerCode == "CB05" && !r.IsActive && r.RowVer == Existing.RowVer), Arg.Any<CancellationToken>());
        _view.Received(1).CloseAsSaved();
    }

    [Fact]
    public void Save_DuplicateCode_IsShownUnderTheCodeAndTheDialogStaysOpen()
    {
        Fill();
        SaveFails(new BusinessRuleException(OfficerService.DuplicateCodeMessage));
        _ = new OfficerEditPresenter(_view, _scopes, officer: null);

        _view.SaveClicked += Raise.Event();

        _view.Received(1).ShowFieldErrors(Arg.Is<IReadOnlyList<FieldMessage<OfficerField>>>(
            e => IsOnly(e, OfficerField.OfficerCode, OfficerService.DuplicateCodeMessage)));
        _view.DidNotReceive().ShowError(Arg.Any<string>());
        _view.DidNotReceive().CloseAsSaved();
    }

    [Fact]
    public void Save_InvalidFields_AreEachShownUnderTheirField()
    {
        Fill();
        SaveFails(new RequestValidationException(
        [
            new ValidationFailure(nameof(SaveOfficerRequest.OfficerCode), "Mã cán bộ không được để trống."),
            new ValidationFailure(nameof(SaveOfficerRequest.FullName), "Họ tên không được để trống."),
            new ValidationFailure(nameof(SaveOfficerRequest.Position), "Chức vụ tối đa 100 ký tự."),
        ]));
        _ = new OfficerEditPresenter(_view, _scopes, officer: null);

        _view.SaveClicked += Raise.Event();

        _view.Received(1).ShowFieldErrors(Arg.Is<IReadOnlyList<FieldMessage<OfficerField>>>(e => e.SequenceEqual(new FieldMessage<OfficerField>[]
        {
            new(OfficerField.OfficerCode, "Mã cán bộ không được để trống."),
            new(OfficerField.FullName, "Họ tên không được để trống."),
            new(OfficerField.Position, "Chức vụ tối đa 100 ký tự."),
        })));
        _view.DidNotReceive().ShowError(Arg.Any<string>());
        _view.DidNotReceive().CloseAsSaved();
    }

    [Fact]
    public void Save_AFailureOfNoField_IsShownInTheBanner()
    {
        Fill();
        SaveFails(new RequestValidationException([new ValidationFailure("RowVer", "Thiếu phiên bản dữ liệu.")]));
        _ = new OfficerEditPresenter(_view, _scopes, officer: null);

        _view.SaveClicked += Raise.Event();

        _view.Received(1).ShowError("Thiếu phiên bản dữ liệu.");
        _view.DidNotReceive().ShowFieldErrors(Arg.Any<IReadOnlyList<FieldMessage<OfficerField>>>());
    }

    [Fact]
    public void ConcurrencyConflict_IsShownInTheBanner()
    {
        _service.UpdateAsync(Arg.Any<int>(), Arg.Any<SaveOfficerRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConcurrencyConflictException(new InvalidOperationException()));
        _ = new OfficerEditPresenter(_view, _scopes, Existing);
        Fill();

        _view.SaveClicked += Raise.Event();

        _view.Received(1).ShowError("Dữ liệu đã bị người khác thay đổi, vui lòng tải lại");
        _view.DidNotReceive().CloseAsSaved();
    }

    [Fact]
    public void EachSave_UsesAFreshScope()
    {
        var scopes = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.GetService(typeof(IOfficerService)).Returns(_service);
        scopes.CreateScope().Returns(scope);
        SaveFails(new BusinessRuleException("x"));
        Fill();
        _ = new OfficerEditPresenter(_view, scopes, officer: null);

        _view.SaveClicked += Raise.Event();
        _view.SaveClicked += Raise.Event();

        scopes.Received(2).CreateScope();
        scope.Received(2).Dispose();
    }
}
