using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace SP01QuestPdf;

/// <summary>
/// Interactive mode: WebView2 previews the generated PDF (bytes written to
/// %LOCALAPPDATA%\LuuKyCanTin\preview\, navigated to file:///), with a Print button that calls
/// CoreWebView2.PrintAsync at 100 % actual size, and a Save As… button. The WebView2 user-data
/// folder lives at %LOCALAPPDATA%\LuuKyCanTin\WebView2 (never next to the exe — Program Files is
/// read-only). Stale preview files are cleaned at startup and on close. Everything runs offline;
/// only the WebView2 Runtime must be installed on the machine (Evergreen or offline installer).
/// </summary>
internal sealed class PreviewForm : Form
{
    private static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LuuKyCanTin");

    private static readonly string PreviewDir = Path.Combine(AppDataDir, "preview");

    private static readonly string WebView2UserDataDir = Path.Combine(AppDataDir, "WebView2");

    private readonly WebView2 _webView = new();
    private readonly ToolStripButton _printButton = new("In ra máy in… (100 % kích thước)");
    private readonly ToolStripButton _saveButton = new("Lưu thành tệp PDF…");
    private readonly ToolStripLabel _statusLabel = new("Đang khởi tạo WebView2…");

    private byte[] _pdfBytes = [];
    private string _previewPath = "";

    public PreviewForm()
    {
        Text = "SP-01 · Xem trước PDF — QuestPDF + WebView2 (ngoại tuyến)";
        Width = 1100;
        Height = 780;
        StartPosition = FormStartPosition.CenterScreen;

        CleanupPreviewFiles();
        BuildUi();

        Load += async (_, _) => await InitializeAsync();
        FormClosing += (_, _) =>
        {
            CleanupPreviewFiles();
            _webView.Dispose();
        };
    }

    private void BuildUi()
    {
        var toolbar = new ToolStrip();
        _printButton.Enabled = false;
        _saveButton.Enabled = false;
        _printButton.Click += OnPrint;
        _saveButton.Click += OnSave;
        toolbar.Items.Add(_printButton);
        toolbar.Items.Add(_saveButton);
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(_statusLabel);

        _webView.Dock = DockStyle.Fill;
        Controls.Add(_webView);
        Controls.Add(toolbar);
        toolbar.Dock = DockStyle.Top;
    }

    private async Task InitializeAsync()
    {
        try
        {
            var environment = await CoreWebView2Environment.CreateAsync(null, WebView2UserDataDir);
            await _webView.EnsureCoreWebView2Async(environment);

            _webView.CoreWebView2.NavigationCompleted += (_, e) =>
            {
                if (e.IsSuccess)
                {
                    _printButton.Enabled = true;
                    _saveButton.Enabled = true;
                    _statusLabel.Text = "WebView2 sẵn sàng (ngoại tuyến). Xem trước: A4 dọc.";
                }
                else
                {
                    _statusLabel.Text = $"Không thể hiển thị bản xem trước (mã lỗi: {e.WebErrorStatus}).";
                }
            };

            SampleReportGenerator.RegisterFonts();
            _pdfBytes = SampleReportGenerator.Generate(MauInKichThuoc.A4Portrait);

            Directory.CreateDirectory(PreviewDir);
            _previewPath = Path.Combine(PreviewDir, "mau-in-a4-portrait.pdf");
            await File.WriteAllBytesAsync(_previewPath, _pdfBytes);

            _webView.CoreWebView2.Navigate(new Uri(_previewPath).AbsoluteUri);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Lỗi: " + ex.Message;
            MessageBox.Show(
                this,
                $"Không thể khởi tạo WebView2 hoặc tạo PDF.\n\n{ex.Message}\n\n"
                + "Yêu cầu: WebView2 Runtime (Evergreen hoặc bộ cài ngoại tuyến "
                + "MicrosoftEdgeWebView2RuntimeInstallerX64.exe).",
                "SP-01",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void OnPrint(object? sender, EventArgs e)
    {
        try
        {
            var settings = _webView.CoreWebView2.Environment.CreatePrintSettings();
            settings.ScaleFactor = 1.0; // 100 % actual size, not "fit to page"
            settings.ShouldPrintHeaderAndFooter = false;
            settings.ShouldPrintBackgrounds = true;

            var status = await _webView.CoreWebView2.PrintAsync(settings);
            _statusLabel.Text = status == CoreWebView2PrintStatus.Succeeded
                ? "Đã gửi lệnh in tới máy in mặc định."
                : $"In thất bại: {status}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "SP-01", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnSave(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = "BienNhanThu-20261002.pdf",
            Title = "Lưu bản in PDF",
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            File.WriteAllBytes(dialog.FileName, _pdfBytes);
            _statusLabel.Text = $"Đã lưu: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "SP-01", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void CleanupPreviewFiles()
    {
        try
        {
            if (!Directory.Exists(PreviewDir))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(PreviewDir, "*.pdf"))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // A file held open by the PDF viewer can wait until the next startup.
                }
            }
        }
        catch
        {
            // Preview cleanup must never break the app.
        }
    }
}