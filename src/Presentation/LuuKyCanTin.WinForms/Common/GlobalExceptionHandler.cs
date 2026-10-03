using Serilog;
using WinFormsApp = System.Windows.Forms.Application;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// Last-chance handler for exceptions nobody caught. The only place outside a Form allowed to call MessageBox.
/// </summary>
internal static class GlobalExceptionHandler
{
    private const string TieuDeLoi = "Lỗi";

    private const string LoiKhongMongMuon =
        "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại hoặc liên hệ quản trị viên. Chi tiết đã được ghi vào nhật ký.";

    /// <summary>Must be called before any window is created.</summary>
    public static void CaiDat()
    {
        WinFormsApp.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        WinFormsApp.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled exception on the UI thread");
        HienThongBaoLoi();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception on a background thread (terminating: {IsTerminating})", e.IsTerminating);
        Log.CloseAndFlush();
        HienThongBaoLoi();
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();

        // Raised on the finalizer thread: hand the message to the UI thread instead of blocking finalization.
        if (WinFormsApp.OpenForms.Count > 0 && WinFormsApp.OpenForms[0] is { IsHandleCreated: true } owner)
            owner.BeginInvoke(HienThongBaoLoi);
    }

    private static void HienThongBaoLoi()
    {
        MessageBox.Show(LoiKhongMongMuon, TieuDeLoi, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
