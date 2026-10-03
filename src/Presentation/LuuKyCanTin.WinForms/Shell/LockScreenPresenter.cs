using LuuKyCanTin.Application.Administration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// The lock overlay's use case: re-verify the signed-in user (story 2.7), or sign out with a confirmation. There is
/// no user-name field, so a lock can only resume the session of the person who locked it.
/// </summary>
public sealed class LockScreenPresenter
{
    public const string UnlockFailedMessage = "Không mở khoá được, vui lòng thử lại.";
    public const string SignOutFailedMessage = "Không đăng xuất được, vui lòng thử lại.";

    private readonly ILockScreenView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly Action _onUnlocked;
    private readonly Action _onSignedOut;
    private bool _isBusy;
    private LockExit _pendingExit;

    public LockScreenPresenter(
        ILockScreenView view,
        IServiceScopeFactory scopes,
        string title,
        string initials,
        string displayName,
        Action onUnlocked,
        Action onSignedOut)
    {
        _view = view;
        _scopes = scopes;
        _onUnlocked = onUnlocked;
        _onSignedOut = onSignedOut;
        _view.Title = title;
        _view.ShowUser(initials, displayName);
        _view.UnlockClicked += OnUnlockClicked;
        _view.SignOutClicked += OnSignOutClicked;
        _view.Closed += OnClosed;
    }

    private enum LockExit
    {
        None,
        Unlocked,
        SignedOut,
    }

    private async void OnUnlockClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
            return;
        _isBusy = true;
        try
        {
            using var scope = _scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<SignInService>()
                .ReauthenticateAsync(_view.Password);

            if (result.MustChangePassword)
            {
                // Same as sign-in: a forced change blocks the session, so the user signs in again.
                _view.ShowLockedMessage(SignInService.PasswordChangeRequiredMessage);
                await SignOutAsync(confirm: false);
                return;
            }

            if (result.Succeeded)
            {
                _pendingExit = LockExit.Unlocked;
                _view.CloseLock();
                return;
            }

            if (result.Status is SignInStatus.AccountLocked or SignInStatus.AccountInactive)
            {
                // The account cannot unlock again; show why, then force the sign-out path.
                _view.ShowLockedMessage(result.Message ?? SignInService.AccountLockedMessage);
                await SignOutAsync(confirm: false);
                return;
            }

            _view.ShowError(result.Message ?? SignInService.InvalidCredentialsMessage);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unlocking the session failed.");
            _view.ShowError(UnlockFailedMessage);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void OnSignOutClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
            return;
        _isBusy = true;
        try
        {
            await SignOutAsync(confirm: true);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task SignOutAsync(bool confirm)
    {
        if (confirm && !_view.ConfirmSignOut())
            return;

        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<SignInService>().SignOutAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Signing out from the lock screen failed.");
            _view.ShowError(SignOutFailedMessage);
            return;
        }

        _pendingExit = LockExit.SignedOut;
        _view.CloseLock();
    }

    // The overlay tells the presenter when it has really closed, so the session only resumes or ends there.
    private void OnClosed(object? sender, EventArgs e)
    {
        var exit = _pendingExit;
        _pendingExit = LockExit.None;
        if (exit == LockExit.Unlocked)
            _onUnlocked();
        else if (exit == LockExit.SignedOut)
            _onSignedOut();
    }
}
