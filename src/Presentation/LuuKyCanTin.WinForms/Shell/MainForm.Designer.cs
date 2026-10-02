namespace LuuKyCanTin.WinForms.Shell;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null!;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        menuStrip = new MenuStrip();
        mnuDanhMuc = new ToolStripMenuItem();
        mnuCanBo = new ToolStripMenuItem();
        menuStrip.SuspendLayout();
        SuspendLayout();
        //
        // menuStrip
        //
        menuStrip.Items.AddRange(new ToolStripItem[] { mnuDanhMuc });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.TabIndex = 0;
        //
        // mnuDanhMuc
        //
        mnuDanhMuc.DropDownItems.AddRange(new ToolStripItem[] { mnuCanBo });
        mnuDanhMuc.Name = "mnuDanhMuc";
        mnuDanhMuc.Text = "&Danh mục";
        //
        // mnuCanBo
        //
        mnuCanBo.Name = "mnuCanBo";
        mnuCanBo.Text = "&Cán bộ";
        //
        // MainForm
        //
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1024, 640);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        Text = "Lưu ký – Căn tin";
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip;
    private ToolStripMenuItem mnuDanhMuc;
    private ToolStripMenuItem mnuCanBo;
}
