using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
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

    [Fact]
    public void Adding_StartsWithAnEmptyActiveStaffMember()
    {
        _ = new OfficerEditPresenter(_view, _scopes, officer: null);

        _view.Received().Title = "Thêm cán bộ";
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
    public void SaveError_IsShown_AndTheDialogStaysOpen()
    {
        Fill();
        _service.AddAsync(Arg.Any<SaveOfficerRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new BusinessRuleException("Mã cán bộ đã tồn tại"));
        _ = new OfficerEditPresenter(_view, _scopes, officer: null);

        _view.SaveClicked += Raise.Event();

        _view.Received(1).ShowError("Mã cán bộ đã tồn tại");
        _view.DidNotReceive().CloseAsSaved();
    }

    [Fact]
    public void ConcurrencyConflict_IsShownToo()
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
        _service.AddAsync(Arg.Any<SaveOfficerRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new BusinessRuleException("x"));
        Fill();
        _ = new OfficerEditPresenter(_view, scopes, officer: null);

        _view.SaveClicked += Raise.Event();
        _view.SaveClicked += Raise.Event();

        scopes.Received(2).CreateScope();
        scope.Received(2).Dispose();
    }
}
