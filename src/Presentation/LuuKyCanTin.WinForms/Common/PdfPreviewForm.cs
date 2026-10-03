using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// The shared PDF preview/print/save window (UX-DR6, spike SP-01). The bytes are written under
/// %LOCALAPPDATA%\LuuKyCanTin\preview and shown by Chromium's built-in viewer; everything works offline
/// once the WebView2 Runtime is installed.
/// </summary>
internal sealed class PdfPreviewForm : Form
{
    private static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LuuKyCanTin");

    private static readonly string PreviewDir = Path.Combine(AppDataDir, "preview");

    private static readonly string WebView2UserDataDir = Path.Combine(AppDataDir, "WebView2");

    private readonly WebView2 _webView = new();
    private readonly ToolStripButton _printButton = new("In ra máy in…");
    private readonly ToolStripButton _saveButton = new("Lưu thành tệp PDF…");
    private readonly ToolStripLabel _statusLabel = new("Đang mở bản xem trước…");

    private readonly byte[] _pdf;
    private readonly string _fileName;
    private string _previewPath = "";

    public PdfPreviewForm(byte[] pdf, string fileName)
    {
        _pdf = pdf;
        _fileName = fileName;

        Text = "Xem trước bản in";
        Width = 1100;
        Height = 780;
        StartPosition = FormStartPosition.CenterParent;
        ShowIcon = false;
        MinimizeBox = false;

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
                    _statusLabel.Text = "Sẵn sàng. In 100 % kích thước thực tế.";
                }
                else
                {
                    _statusLabel.Text = $"Không hiển thị được bản xem trước (mã lỗi: {e.WebErrorStatus}).";
                }
            };

            Directory.CreateDirectory(PreviewDir);
            _previewPath = Path.Combine(PreviewDir, $"{_fileName}-{Guid.NewGuid():N}.pdf");
            await File.WriteAllBytesAsync(_previewPath, _pdf);

            _webView.CoreWebView2.Navigate(new Uri(_previewPath).AbsoluteUri);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Lỗi: " + ex.Message;
            MessageBox.Show(
                this,
                $"Không thể mở bản xem trước PDF.\n\n{ex.Message}\n\n"
                + "Yêu cầu: WebView2 Runtime (Evergreen hoặc bộ cài ngoại tuyến "
                + "MicrosoftEdgeWebView2RuntimeInstallerX64.exe).",
                "Lỗi in",
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
            MessageBox.Show(this, ex.Message, "Lỗi in", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnSave(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = $"{_fileName}.pdf",
            Title = "Lưu bản in PDF",
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            File.WriteAllBytes(dialog.FileName, _pdf);
            _statusLabel.Text = $"Đã lưu: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi lưu tệp", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void CleanupPreviewFiles()
    {
        try
        {
            if (!Directory.Exists(PreviewDir))
                return;

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
