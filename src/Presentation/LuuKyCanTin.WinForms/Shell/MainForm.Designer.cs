namespace LuuKyCanTin.WinForms.Shell;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null!;
    private MenuStrip menuStrip = null!;
    private ToolStripMenuItem mnuMasterData = null!;
    private ToolStripMenuItem mnuOfficers = null!;
    private ToolStripMenuItem mnuAddInmate = null!;
    private ToolStripMenuItem mnuCustody = null!;
    private ToolStripMenuItem mnuDepositReceipt = null!;
    private ToolStripMenuItem mnuAdministration = null!;
    private ToolStripMenuItem mnuRoles = null!;
    private ToolStripMenuItem mnuChangePassword = null!;
    private ToolStripMenuItem mnuSignOut = null!;

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
        mnuMasterData = new ToolStripMenuItem();
        mnuOfficers = new ToolStripMenuItem();
        mnuAddInmate = new ToolStripMenuItem();
        mnuCustody = new ToolStripMenuItem();
        mnuDepositReceipt = new ToolStripMenuItem();
        mnuAdministration = new ToolStripMenuItem();
        mnuRoles = new ToolStripMenuItem();
        mnuChangePassword = new ToolStripMenuItem();
        mnuSignOut = new ToolStripMenuItem();
        menuStrip.SuspendLayout();
        SuspendLayout();

        mnuOfficers.Name = "mnuOfficers";
        mnuOfficers.Text = "Cán bộ…";
        mnuAddInmate.Name = "mnuAddInmate";
        mnuAddInmate.Text = "Thêm đối tượng…";
        mnuAddInmate.Click += OnAddInmate;
        mnuMasterData.DropDownItems.AddRange([mnuOfficers, mnuAddInmate]);
        mnuMasterData.Name = "mnuMasterData";
        mnuMasterData.Text = "Danh mục";

        mnuDepositReceipt.Name = "mnuDepositReceipt";
        mnuDepositReceipt.Text = "Lập biên nhận thu…";
        mnuDepositReceipt.Click += OnCreateDepositReceipt;
        mnuCustody.DropDownItems.AddRange([mnuDepositReceipt]);
        mnuCustody.Name = "mnuCustody";
        mnuCustody.Text = "Lưu ký";

        mnuRoles.Name = "mnuRoles";
        mnuRoles.Text = "Vai trò…";
        mnuChangePassword.Name = "mnuChangePassword";
        mnuChangePassword.Text = "Đổi mật khẩu…";
        mnuSignOut.Name = "mnuSignOut";
        mnuSignOut.Text = "Đăng xuất";
        mnuAdministration.DropDownItems.AddRange([mnuRoles, mnuChangePassword, mnuSignOut]);
        mnuAdministration.Name = "mnuAdministration";
        mnuAdministration.Text = "Hệ thống";

        menuStrip.Items.AddRange([mnuMasterData, mnuCustody, mnuAdministration]);
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
