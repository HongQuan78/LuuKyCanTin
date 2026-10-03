using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private Sidebar sidebar = null!;
    private HeaderBar headerBar = null!;
    private StatusBar statusBar = null!;
    private Panel pnlContent = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        sidebar = new Sidebar();
        headerBar = new HeaderBar();
        statusBar = new StatusBar();
        pnlContent = new Panel();
        SuspendLayout();

        sidebar.Name = "sidebar";
        sidebar.TabIndex = 0;

        headerBar.Name = "headerBar";
        headerBar.TabStop = false;

        statusBar.Name = "statusBar";
        statusBar.TabStop = false;

        pnlContent.BackColor = AppTheme.Surface;
        pnlContent.Dock = DockStyle.Fill;
        pnlContent.Name = "pnlContent";
        pnlContent.Padding = new Padding(AppTheme.PagePadding);
        pnlContent.TabIndex = 1;

        // Dock order is reverse add order: the sidebar takes the full height, then header and status bar share the rest.
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Surface;
        ClientSize = new Size(1366, 737);
        Controls.Add(pnlContent);
        Controls.Add(headerBar);
        Controls.Add(statusBar);
        Controls.Add(sidebar);
        Font = AppTheme.BodyFont;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Lưu ký – Căn tin";
        WindowState = FormWindowState.Maximized;
        ResumeLayout(false);
    }
}
