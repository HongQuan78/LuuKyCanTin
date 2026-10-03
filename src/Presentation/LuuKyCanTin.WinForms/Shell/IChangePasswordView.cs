using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The change-password dialog, used forced after a first sign-in and voluntary from the shell.</summary>
public interface IChangePasswordView : IDisposable
{
    event EventHandler SaveClicked;

    event EventHandler CancelClicked;

    /// <summary>Raised as the user types in any of the three password boxes.</summary>
    event EventHandler InputChanged;

    string Title { set; }

    /// <summary>In forced mode the secondary button reads "Đăng xuất" and signs out instead of returning to the shell.</summary>
    bool IsForced { set; }

    string CurrentPassword { get; }

    string NewPassword { get; }

    string Confirmation { get; }

    /// <summary>Enables the primary button; false until every rule passes and the confirmation matches.</summary>
    bool CanSave { set; }

    /// <summary>Ticks each policy rule green or grey in the live checklist.</summary>
    void ShowRuleResults(IReadOnlyList<PasswordRuleResult> results);

    /// <summary>Marks one field invalid, shows the message under it and focuses it.</summary>
    void ShowFieldError(PasswordField field, string message);

    /// <summary>Shows an error that concerns no single field in the banner above the fields.</summary>
    void ShowError(string message);

    /// <summary>Closes the dialog; true means the password was changed.</summary>
    void CloseWithResult(bool succeeded);

    /// <summary>Shows the dialog modally; true if it closed after a successful change.</summary>
    bool ShowModal();
}
