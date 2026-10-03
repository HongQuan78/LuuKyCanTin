using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

public class RolePresenterTests
{
    private static readonly byte[] RowVer = [1, 2, 3];

    private readonly IRoleView _view = Substitute.For<IRoleView>();
    private readonly IRoleService _service = Substitute.For<IRoleService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private RolePresenter CreatePresenter() => new(_view, FakeScopeFactory.Create(_service), _currentUser);

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

    private void SeedOneRole(params string[] permissionCode)
    {
        _service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([new RoleDto(1, RoleCodes.Administrator, "Quản trị hệ thống", RowVer)]);
        _view.SelectedRoleId.Returns(1);
        _service.GetPermissionsAsync(1, Arg.Any<CancellationToken>()).Returns(permissionCode);
    }

    private async Task WaitForLoadAsync()
    {
        var loaded = new TaskCompletionSource();
        _view.When(v => v.ShowPermissions(Arg.Any<IReadOnlyList<string>>())).Do(_ => loaded.TrySetResult());
        _view.Loaded += Raise.Event();
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Loaded_ShowsTheRoleListAndItsPermissions()
    {
        SeedOneRole(PermissionCodes.Administration.View, PermissionCodes.MasterData.Create);

        CreatePresenter();
        await WaitForLoadAsync();

        _view.Received(1).ShowRoles(Arg.Any<IReadOnlyList<RoleDto>>());
        _view.Received(1).ShowPermissions(Arg.Is<IReadOnlyList<string>>(
            q => q.Count == 2 && q.Contains(PermissionCodes.MasterData.Create)));
    }

    [Fact]
    public async Task Save_SendsTheTickedCodesAndConfirms()
    {
        SeedOneRole(PermissionCodes.Administration.View);
        _view.SelectedPermissions.Returns([PermissionCodes.CustodyReporting.View, PermissionCodes.CustodyReporting.Print]);
        var saved = new TaskCompletionSource();
        _view.When(v => v.ShowMessage(Arg.Is<string>(s => s.Contains("Đã lưu")))).Do(_ => saved.TrySetResult());

        CreatePresenter();
        await WaitForLoadAsync();
        _view.SaveClicked += Raise.Event();
        await saved.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).UpdatePermissionsAsync(
            1,
            Arg.Is<IReadOnlyCollection<string>>(c => c.Count == 2 && c.Contains(PermissionCodes.CustodyReporting.Print)),
            RowVer,
            Arg.Any<CancellationToken>());
        _view.Received(1).ShowMessage(Arg.Is<string>(s => s.Contains("Đã lưu")));
    }

    [Fact]
    public async Task Save_WithoutPermission_ShowsTheError()
    {
        SeedOneRole(PermissionCodes.Administration.View);
        _service.UpdatePermissionsAsync(1, Arg.Any<IReadOnlyCollection<string>>(), RowVer, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PermissionDeniedException(PermissionCodes.Administration.Update)));
        var errorShown = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => errorShown.TrySetResult());

        CreatePresenter();
        await WaitForLoadAsync();
        _view.SaveClicked += Raise.Event();
        await errorShown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowError("Bạn không có quyền thực hiện thao tác này");
    }

    [Fact]
    public async Task Save_OnAStaleRowVersion_ShowsTheConflict()
    {
        SeedOneRole(PermissionCodes.Administration.View);
        _service.UpdatePermissionsAsync(1, Arg.Any<IReadOnlyCollection<string>>(), RowVer, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new ConcurrencyConflictException(new Exception("row version"))));
        var errorShown = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => errorShown.TrySetResult());

        CreatePresenter();
        await WaitForLoadAsync();
        _view.SaveClicked += Raise.Event();
        await errorShown.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowError(ConcurrencyConflictException.ConflictMessage);
    }
}
