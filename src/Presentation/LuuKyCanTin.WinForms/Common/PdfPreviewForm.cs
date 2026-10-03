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

    private const int ToolbarHeight = 52;

    private readonly WebView2 _webView = new();
    private readonly Button _printButton = new() { Name = "btnPrint", Text = "&In ra máy in…" };
    private readonly Button _saveButton = new() { Name = "btnSave", Text = "&Lưu thành tệp PDF…" };
    private readonly Label _statusLabel = new() { Name = "lblStatus", Text = "Đang mở bản xem trước…" };

    private readonly byte[] _pdf;
    private readonly string _fileName;
    private string _previewPath = "";

    public PdfPreviewForm(byte[] pdf, string fileName)
    {
        _pdf = pdf;
        _fileName = fileName;

        Text = "Xem trước bản in";
        // Fits a 1366×768 screen with the taskbar.
        Width = 1100;
        Height = 720;
        StartPosition = FormStartPosition.CenterParent;
        ShowIcon = false;
        MinimizeBox = false;
        Font = AppTheme.BodyFont;
        BackColor = AppTheme.Surface;

        CleanupPreviewFiles();
        BuildUi();

        Load += async (_, _) => await InitializeAsync();
        FormClosing += (_, _) =>
        {
            CleanupPreviewFiles();
            _webView.Dispose();
        };
    }

    // A white toolbar with a bottom border (secondary button, then "In" as the one primary), the viewer, and a status
    // line styled like the shell's status bar.
    private void BuildUi()
    {
        var toolbar = new FlowLayoutPanel
        {
            BackColor = AppTheme.Card,
            Dock = DockStyle.Top,
            Height = ToolbarHeight,
            Name = "pnlToolbar",
            Padding = new Padding(AppTheme.PagePadding - 3, (ToolbarHeight - AppTheme.ButtonHeight) / 2, 0, 0),
            WrapContents = false,
        };
        toolbar.Paint += (_, e) => PaintLine(e.Graphics, toolbar.Height - 1, toolbar.Width);

        foreach (var button in new[] { _saveButton, _printButton })
        {
            button.AutoSize = true;
            button.Enabled = false;
            button.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
            toolbar.Controls.Add(button);
        }

        AppTheme.StyleSecondary(_saveButton);
        AppTheme.SetGlyph(_saveButton, Glyphs.SaveAs);
        AppTheme.StylePrimary(_printButton);
        AppTheme.SetGlyph(_printButton, Glyphs.Print);
        _printButton.Click += OnPrint;
        _saveButton.Click += OnSave;

        var statusBar = new Panel
        {
            BackColor = AppTheme.Card,
            Dock = DockStyle.Bottom,
            Height = AppTheme.StatusHeight,
            Name = "pnlStatus",
            Padding = new Padding(12, 0, 12, 0),
        };
        statusBar.Paint += (_, e) => PaintLine(e.Graphics, 0, statusBar.Width);
        _statusLabel.AutoEllipsis = true;
        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.Font = AppTheme.SmallFont;
        _statusLabel.ForeColor = AppTheme.Text2;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.UseMnemonic = false;
        statusBar.Controls.Add(_statusLabel);

        _webView.Dock = DockStyle.Fill;
        Controls.Add(_webView);
        Controls.Add(toolbar);
        Controls.Add(statusBar);
    }

    private static void PaintLine(Graphics graphics, int y, int width)
    {
        using var line = new SolidBrush(AppTheme.Border);
        graphics.FillRectangle(line, 0, y, width, 1);
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
