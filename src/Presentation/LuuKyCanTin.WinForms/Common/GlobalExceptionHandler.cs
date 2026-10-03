using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using Serilog;
using WinFormsApp = System.Windows.Forms.Application;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// Last-chance handler for exceptions nobody caught. The only place outside a Form allowed to call MessageBox.
/// </summary>
internal static class GlobalExceptionHandler
{
    private const string ErrorTitle = "Lỗi";
    private const string WarningTitle = "Cảnh báo";

    private const string UnexpectedErrorMessage =
        "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại hoặc liên hệ quản trị viên. Chi tiết đã được ghi vào nhật ký.";

    /// <summary>Set once the host is built, so a warning logs who hit the rule.</summary>
    public static ICurrentUser? CurrentUser { get; set; }

    /// <summary>The warning dialog; replaceable in tests. Defaults to the real MessageBox.</summary>
    internal static Action<string, string> ShowWarning { get; set; } =
        (message, title) => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    /// <summary>Must be called before any window is created.</summary>
    public static void Install()
    {
        WinFormsApp.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        WinFormsApp.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>
    /// A rule the user broke or a refused permission: shown as a warning with its own message, logged at Warning,
    /// never as the generic "unexpected error" crash. An <see cref="AggregateException"/> is unwrapped first.
    /// </summary>
    internal static bool IsBusinessError(Exception? exception) => BusinessError(exception) is not null;

    /// <summary>
    /// Shows the warning dialog for a business error and returns true; an unrelated exception returns false
    /// without touching the dialog.
    /// </summary>
    internal static bool TryShowBusinessWarning(Exception? exception)
    {
        var business = BusinessError(exception);
        if (business is null)
            return false;

        ShowWarning(business.Message, WarningTitle);
        return true;
    }

    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        if (LogBusinessWarning(e.Exception, "Unhandled exception on the UI thread"))
        {
            TryShowBusinessWarning(e.Exception);
            return;
        }

        Log.Error(e.Exception, "Unhandled exception on the UI thread");
        ShowErrorMessage();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        if (LogBusinessWarning(exception, "Unhandled exception on a background thread"))
        {
            Log.CloseAndFlush();
            TryShowBusinessWarning(exception);
            return;
        }

        Log.Fatal(exception, "Unhandled exception on a background thread (terminating: {IsTerminating})", e.IsTerminating);
        Log.CloseAndFlush();
        ShowErrorMessage();
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();

        // Logged unconditionally: with no form there is nobody to show the message to, but the log must record it.
        if (!LogBusinessWarning(e.Exception, "Unobserved task exception"))
            Log.Error(e.Exception, "Unobserved task exception");

        // Raised on the finalizer thread: hand the message to the UI thread instead of blocking finalization.
        if (WinFormsApp.OpenForms.Count > 0 && WinFormsApp.OpenForms[0] is { IsHandleCreated: true } owner)
            owner.BeginInvoke(() => ShowUnobserved(e.Exception));
    }

    private static void ShowUnobserved(Exception exception)
    {
        if (TryShowBusinessWarning(exception))
            return;

        ShowErrorMessage();
    }

    /// <summary>Logs a business error at Warning with its context; false for anything else.</summary>
    private static bool LogBusinessWarning(Exception? exception, string context)
    {
        var business = BusinessError(exception);
        if (business is null)
            return false;

        Log.Warning(
            business,
            "{Context}: {Message} (user {UserId}, permission {PermissionCode})",
            context,
            business.Message,
            CurrentUser?.UserId,
            (business as PermissionDeniedException)?.PermissionCode);
        return true;
    }

    // An AggregateException (an unobserved task) is unwrapped until a business error is found.
    private static Exception? BusinessError(Exception? exception) => exception switch
    {
        BusinessRuleException or PermissionDeniedException => exception,
        AggregateException aggregate => aggregate.InnerExceptions.Select(BusinessError).FirstOrDefault(e => e is not null),
        _ => null,
    };

    private static void ShowErrorMessage() =>
        MessageBox.Show(UnexpectedErrorMessage, ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
}
