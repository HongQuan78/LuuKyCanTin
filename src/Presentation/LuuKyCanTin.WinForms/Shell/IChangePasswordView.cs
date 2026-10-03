namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The change-password dialog, used forced after a first sign-in and voluntary from the shell menu.</summary>
public interface IChangePasswordView : IDisposable
{
    event EventHandler SaveClicked;

    event EventHandler CancelClicked;

    string Title { set; }

    /// <summary>In forced mode Cancel signs out instead of returning to the shell.</summary>
    bool IsForced { set; }

    string CurrentPassword { get; }

    string NewPassword { get; }

    string Confirmation { get; }

    void ShowError(string message);

    /// <summary>Closes the dialog; true means the password was changed.</summary>
    void CloseWithResult(bool succeeded);

    /// <summary>Shows the dialog modally; true if it closed after a successful change.</summary>
    bool DisplayText();
}
