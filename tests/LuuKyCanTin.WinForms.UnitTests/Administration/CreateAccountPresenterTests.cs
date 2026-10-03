using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

public class CreateAccountPresenterTests
{
    private readonly ICreateAccountView _view = Substitute.For<ICreateAccountView>();
    private readonly IAccountService _accountService = Substitute.For<IAccountService>();
    private readonly IRoleService _roleService = Substitute.For<IRoleService>();
    private readonly List<string> _temporaryPasswords = [];

    private CreateAccountPresenter CreatePresenter() =>
        new(_view, FakeScopeFactory.Create(_accountService, _roleService), _temporaryPasswords.Add);

    [Fact]
    public async Task Loaded_ShowsTheStaffAndTheRoles()
    {
        var loaded = new TaskCompletionSource();
        _view.When(v => v.ShowRoles(Arg.Any<IReadOnlyList<RoleDto>>())).Do(_ => loaded.TrySetResult());

        CreatePresenter();
        _view.Loaded += Raise.Event();
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowOfficers(Arg.Any<IReadOnlyList<OfficerDto>>());
        _view.Received(1).ShowRoles(Arg.Any<IReadOnlyList<RoleDto>>());
    }

    [Fact]
    public async Task Create_Success_ShowsTheTemporaryPasswordAndCloses()
    {
        _view.UserName.Returns("thuquy");
        _view.OfficerId.Returns(5);
        _view.SelectedRoleIds.Returns([2]);
        _accountService.CreateAsync(Arg.Any<CreateAccountRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreateAccountResult(9, "Abc234Def567"));
        var done = new TaskCompletionSource();
        _view.When(v => v.CloseAsSaved()).Do(_ => done.TrySetResult());

        CreatePresenter();
        _view.CreateClicked += Raise.Event();
        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _accountService.Received(1).CreateAsync(
            Arg.Is<CreateAccountRequest>(r => r.UserName == "thuquy" && r.OfficerId == 5 && r.RoleIds.Count == 1),
            Arg.Any<CancellationToken>());
        _temporaryPasswords.ShouldBe(["Abc234Def567"]);
    }

    [Fact]
    public async Task Create_BusinessError_ShowsTheMessageAndStaysOpen()
    {
        _view.UserName.Returns("thuquy");
        _view.OfficerId.Returns(5);
        _view.SelectedRoleIds.Returns([2]);
        _accountService.CreateAsync(Arg.Any<CreateAccountRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CreateAccountResult>(
                new BusinessRuleException(AccountService.OfficerAlreadyHasActiveAccountMessage)));
        var done = new TaskCompletionSource();
        _view.When(v => v.ShowError(Arg.Any<string>())).Do(_ => done.TrySetResult());

        CreatePresenter();
        _view.CreateClicked += Raise.Event();
        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _view.Received(1).ShowError(AccountService.OfficerAlreadyHasActiveAccountMessage);
        _view.DidNotReceive().CloseAsSaved();
        _temporaryPasswords.ShouldBeEmpty();
    }
}
