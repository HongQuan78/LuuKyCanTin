using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

public class AccountRolesPresenterTests
{
    private readonly IAccountRolesView _view = Substitute.For<IAccountRolesView>();
    private readonly IAccountService _accountService = Substitute.For<IAccountService>();
    private readonly IRoleService _roleService = Substitute.For<IRoleService>();

    private static AccountDto Account() =>
        new(1, "thuquy", "Nguyễn Văn A", "Lưu ký", [2], true, false, true);

    private AccountRolesPresenter CreatePresenter() =>
        new(_view, FakeScopeFactory.Create(_accountService, _roleService), Account());

    [Fact]
    public async Task Loaded_ShowsTheRolesAndTicksTheCurrentOnes()
    {
        var roles = new List<RoleDto>
        {
            new(1, RoleCodes.Administrator, "Quản trị hệ thống", [1, 2, 3]),
            new(2, RoleCodes.CustodyOfficer, "Cán bộ theo dõi tiền lưu ký", [4, 5, 6]),
        };
        _roleService.GetAllAsync(Arg.Any<CancellationToken>()).Returns(roles);
        var loaded = new TaskCompletionSource();
        _view.When(v => v.ShowSelectedRoles(Arg.Any<IReadOnlyList<int>>())).Do(_ => loaded.TrySetResult());

        CreatePresenter();
        _view.Loaded += Raise.Event();
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowRoles(roles);
        _view.Received(1).ShowSelectedRoles(Arg.Is<IReadOnlyList<int>>(ids => ids.Count == 1 && ids[0] == 2));
    }

    [Fact]
    public async Task Save_Success_ClosesAsSaved()
    {
        _view.SelectedRoleIds.Returns([1, 2]);
        var done = new TaskCompletionSource();
        _view.When(v => v.CloseAsSaved()).Do(_ => done.TrySetResult());

        CreatePresenter();
        _view.SaveClicked += Raise.Event();
        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _accountService.Received(1).UpdateRolesAsync(
            1, Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 2), Arg.Any<CancellationToken>());
    }
}
