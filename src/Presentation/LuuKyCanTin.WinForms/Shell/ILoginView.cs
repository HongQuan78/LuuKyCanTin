namespace LuuKyCanTin.WinForms.Shell;

public interface ILoginView
{
    event EventHandler? SignInClicked;

    string UserName { get; }

    string Password { get; }

    /// <summary>While true the sign-in button reads "Đang đăng nhập…", is disabled, and the cursor waits.</summary>
    bool IsBusy { set; }

    /// <summary>Shows the error banner above the fields, then clears and focuses the password box.</summary>
    void ShowError(string message);

    /// <summary>Clears the password box, for a locked account: retyping the same password can't help.</summary>
    void ClearPassword();

    /// <summary>Closes the form; true sets <see cref="DialogResult.OK"/> so the shell may open.</summary>
    void CloseWithResult(bool succeeded);
}
