using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

public class AccountPresenterTests
{
    private readonly IAccountView _view = Substitute.For<IAccountView>();
    private readonly IAccountService _service = Substitute.For<IAccountService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly List<string> _temporaryPasswords = [];

    private static AccountDto Account(bool isActive) =>
        new(1, "thuquy", "Nguyễn Văn A", "Cán bộ theo dõi tiền lưu ký", [2], isActive, false, true);

    private AccountPresenter CreatePresenter() =>
        new(
            _view,
            FakeScopeFactory.Create(_service),
            () => Substitute.For<ICreateAccountView>(),
            () => Substitute.For<IAccountRolesView>(),
            _temporaryPasswords.Add,
            _currentUser);

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

    private async Task LoadAsync()
    {
        var loaded = new TaskCompletionSource();
        _view.When(v => v.ShowAccounts(Arg.Any<IReadOnlyList<AccountDto>>())).Do(_ => loaded.TrySetResult());
        _view.Loaded += Raise.Event();
        await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Loaded_ShowsTheAccountList()
    {
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns([Account(isActive: true)]);

        CreatePresenter();
        await LoadAsync();

        _view.Received(1).ShowAccounts(Arg.Is<IReadOnlyList<AccountDto>>(a => a.Count == 1));
    }

    [Fact]
    public async Task ResetPassword_Confirmed_ShowsTheTemporaryPasswordOnce()
    {
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns([Account(isActive: true)]);
        _service.ResetPasswordAsync(1, Arg.Any<CancellationToken>()).Returns("Abc234Def567");
        _view.SelectedAccount.Returns(Account(isActive: true));
        _view.Confirm(Arg.Any<string>()).Returns(true);
        var done = new TaskCompletionSource();
        _view.When(v => v.ShowMessage(Arg.Is<string>(s => s.Contains("Đã đặt lại")))).Do(_ => done.TrySetResult());

        CreatePresenter();
        await LoadAsync();
        _view.ResetPasswordClicked += Raise.Event();
        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).ResetPasswordAsync(1, Arg.Any<CancellationToken>());
        _temporaryPasswords.ShouldBe(["Abc234Def567"]);
    }

    [Fact]
    public async Task ResetPassword_Declined_WritesNothing()
    {
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns([Account(isActive: true)]);
        _view.SelectedAccount.Returns(Account(isActive: true));
        _view.Confirm(Arg.Any<string>()).Returns(false);

        CreatePresenter();
        await LoadAsync();
        _view.ResetPasswordClicked += Raise.Event();

        await _service.DidNotReceive().ResetPasswordAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        _temporaryPasswords.ShouldBeEmpty();
    }

    [Fact]
    public async Task Deactivate_Confirmed_CallsTheService()
    {
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns([Account(isActive: true)]);
        _view.SelectedAccount.Returns(Account(isActive: true));
        _view.Confirm(Arg.Any<string>()).Returns(true);
        var done = new TaskCompletionSource();
        _view.When(v => v.ShowMessage(Arg.Is<string>(s => s.Contains("Đã ngừng")))).Do(_ => done.TrySetResult());

        CreatePresenter();
        await LoadAsync();
        _view.ToggleActiveClicked += Raise.Event();
        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).DeactivateAsync(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reactivate_OnAnInactiveAccount_CallsTheService()
    {
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns([Account(isActive: false)]);
        _view.SelectedAccount.Returns(Account(isActive: false));
        var done = new TaskCompletionSource();
        _view.When(v => v.ShowMessage(Arg.Is<string>(s => s.Contains("Đã kích hoạt")))).Do(_ => done.TrySetResult());

        CreatePresenter();
        await LoadAsync();
        _view.ToggleActiveClicked += Raise.Event();
        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).ReactivateAsync(1, Arg.Any<CancellationToken>());
        await _service.DidNotReceive().DeactivateAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unlock_DoesNotAskForConfirmation()
    {
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns([Account(isActive: true)]);
        _view.SelectedAccount.Returns(Account(isActive: true));
        var done = new TaskCompletionSource();
        _view.When(v => v.ShowMessage(Arg.Is<string>(s => s.Contains("Đã mở khoá")))).Do(_ => done.TrySetResult());

        CreatePresenter();
        await LoadAsync();
        _view.UnlockClicked += Raise.Event();
        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _service.Received(1).UnlockAsync(1, Arg.Any<CancellationToken>());
        _view.DidNotReceive().Confirm(Arg.Any<string>());
    }
}
