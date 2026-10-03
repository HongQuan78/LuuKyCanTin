namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The full-window lock overlay (key-01 D), shown over the shell while a session is locked.</summary>
public interface ILockScreenView : IDisposable
{
    event EventHandler UnlockClicked;

    event EventHandler SignOutClicked;

    /// <summary>Raised from <c>OnFormClosed</c>, so the presenter completes the unlock or sign-out there.</summary>
    event EventHandler Closed;

    string Password { get; }

    /// <summary>The app title shown at the top of the overlay.</summary>
    string Title { set; }

    /// <summary>The initials tile and the name of the user who locked the session.</summary>
    void ShowUser(string initials, string displayName);

    /// <summary>Shows the wrong-password message in the banner and returns focus to the password box.</summary>
    void ShowError(string message);

    /// <summary>Shows the locked-account message modally before the forced sign-out.</summary>
    void ShowLockedMessage(string message);

    /// <summary>Asks "Mọi dữ liệu chưa lưu sẽ bị mất. Bạn có chắc muốn đăng xuất?"; No is the default.</summary>
    bool ConfirmSignOut();

    /// <summary>Closes the overlay and re-enables the forms it disabled.</summary>
    void CloseLock();
}
