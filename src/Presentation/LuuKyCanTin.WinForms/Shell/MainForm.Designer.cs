namespace LuuKyCanTin.WinForms.Shell;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null!;
    private MenuStrip menuStrip = null!;
    private ToolStripMenuItem mnuDanhMuc = null!;
    private ToolStripMenuItem mnuCanBo = null!;
    private ToolStripMenuItem mnuThemDoiTuong = null!;
    private ToolStripMenuItem mnuLuuKy = null!;
    private ToolStripMenuItem mnuLapBienNhanThu = null!;

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
        mnuThemDoiTuong = new ToolStripMenuItem();
        mnuLuuKy = new ToolStripMenuItem();
        mnuLapBienNhanThu = new ToolStripMenuItem();
        menuStrip.SuspendLayout();
        SuspendLayout();

        mnuCanBo.Name = "mnuCanBo";
        mnuCanBo.Text = "Cán bộ…";
        mnuThemDoiTuong.Name = "mnuThemDoiTuong";
        mnuThemDoiTuong.Text = "Thêm đối tượng…";
        mnuThemDoiTuong.Click += OnThemDoiTuong;
        mnuDanhMuc.DropDownItems.AddRange([mnuCanBo, mnuThemDoiTuong]);
        mnuDanhMuc.Name = "mnuDanhMuc";
        mnuDanhMuc.Text = "Danh mục";

        mnuLapBienNhanThu.Name = "mnuLapBienNhanThu";
        mnuLapBienNhanThu.Text = "Lập biên nhận thu…";
        mnuLapBienNhanThu.Click += OnLapBienNhanThu;
        mnuLuuKy.DropDownItems.AddRange([mnuLapBienNhanThu]);
        mnuLuuKy.Name = "mnuLuuKy";
        mnuLuuKy.Text = "Lưu ký";

        menuStrip.Items.AddRange([mnuDanhMuc, mnuLuuKy]);
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1024, 28);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1024, 640);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Lưu ký – Căn tin";
        WindowState = FormWindowState.Maximized;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}
