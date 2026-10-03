namespace LuuKyCanTin.WinForms.HeThong;

partial class TaiKhoanVaiTroForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblVaiTro = null!;
    private CheckedListBox lstVaiTro = null!;
    private Label lblLoi = null!;
    private Button btnLuu = null!;
    private Button btnHuy = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblVaiTro = new Label();
        lstVaiTro = new CheckedListBox();
        lblLoi = new Label();
        btnLuu = new Button();
        btnHuy = new Button();
        SuspendLayout();

        lblVaiTro.AutoSize = true;
        lblVaiTro.Location = new Point(14, 14);
        lblVaiTro.Text = "Vai trò của tài khoản:";

        lstVaiTro.CheckOnClick = true;
        lstVaiTro.FormattingEnabled = true;
        lstVaiTro.Location = new Point(14, 38);
        lstVaiTro.Name = "lstVaiTro";
        lstVaiTro.Size = new Size(360, 112);
        lstVaiTro.TabIndex = 0;

        lblLoi.ForeColor = Color.Firebrick;
        lblLoi.Location = new Point(14, 160);
        lblLoi.Size = new Size(360, 40);
        lblLoi.Text = "";

        btnLuu.Location = new Point(218, 206);
        btnLuu.Name = "btnLuu";
        btnLuu.Size = new Size(75, 25);
        btnLuu.TabIndex = 1;
        btnLuu.Text = "&Lưu";

        btnHuy.DialogResult = DialogResult.Cancel;
        btnHuy.Location = new Point(299, 206);
        btnHuy.Name = "btnHuy";
        btnHuy.Size = new Size(75, 25);
        btnHuy.TabIndex = 2;
        btnHuy.Text = "&Hủy";

        AcceptButton = btnLuu;
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnHuy;
        ClientSize = new Size(390, 244);
        Controls.Add(btnHuy);
        Controls.Add(btnLuu);
        Controls.Add(lblLoi);
        Controls.Add(lstVaiTro);
        Controls.Add(lblVaiTro);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "TaiKhoanVaiTroForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Phân vai trò";
        ResumeLayout(false);
        PerformLayout();
    }
}
