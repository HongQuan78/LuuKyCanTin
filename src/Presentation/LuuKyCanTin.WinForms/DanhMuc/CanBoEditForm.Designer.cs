namespace LuuKyCanTin.WinForms.DanhMuc;

partial class CanBoEditForm
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
        lblMaCanBo = new Label();
        txtMaCanBo = new TextBox();
        lblHoTen = new Label();
        txtHoTen = new TextBox();
        lblChucVu = new Label();
        txtChucVu = new TextBox();
        chkLaQuanGiao = new CheckBox();
        chkDangCongTac = new CheckBox();
        btnLuu = new Button();
        btnHuy = new Button();
        SuspendLayout();
        //
        // lblMaCanBo
        //
        lblMaCanBo.AutoSize = true;
        lblMaCanBo.Location = new Point(12, 15);
        lblMaCanBo.Name = "lblMaCanBo";
        lblMaCanBo.Text = "Mã cán bộ:";
        //
        // txtMaCanBo
        //
        txtMaCanBo.CharacterCasing = CharacterCasing.Upper;
        txtMaCanBo.Location = new Point(100, 12);
        txtMaCanBo.MaxLength = 20;
        txtMaCanBo.Name = "txtMaCanBo";
        txtMaCanBo.Size = new Size(160, 23);
        txtMaCanBo.TabIndex = 0;
        //
        // lblHoTen
        //
        lblHoTen.AutoSize = true;
        lblHoTen.Location = new Point(12, 44);
        lblHoTen.Name = "lblHoTen";
        lblHoTen.Text = "Họ tên:";
        //
        // txtHoTen
        //
        txtHoTen.Location = new Point(100, 41);
        txtHoTen.MaxLength = 100;
        txtHoTen.Name = "txtHoTen";
        txtHoTen.Size = new Size(300, 23);
        txtHoTen.TabIndex = 1;
        //
        // lblChucVu
        //
        lblChucVu.AutoSize = true;
        lblChucVu.Location = new Point(12, 73);
        lblChucVu.Name = "lblChucVu";
        lblChucVu.Text = "Chức vụ:";
        //
        // txtChucVu
        //
        txtChucVu.Location = new Point(100, 70);
        txtChucVu.MaxLength = 100;
        txtChucVu.Name = "txtChucVu";
        txtChucVu.Size = new Size(300, 23);
        txtChucVu.TabIndex = 2;
        //
        // chkLaQuanGiao
        //
        chkLaQuanGiao.AutoSize = true;
        chkLaQuanGiao.Location = new Point(100, 101);
        chkLaQuanGiao.Name = "chkLaQuanGiao";
        chkLaQuanGiao.TabIndex = 3;
        chkLaQuanGiao.Text = "Là cán bộ quản giáo";
        //
        // chkDangCongTac
        //
        chkDangCongTac.AutoSize = true;
        chkDangCongTac.Location = new Point(100, 126);
        chkDangCongTac.Name = "chkDangCongTac";
        chkDangCongTac.TabIndex = 4;
        chkDangCongTac.Text = "Đang công tác";
        //
        // btnLuu
        //
        btnLuu.Location = new Point(244, 160);
        btnLuu.Name = "btnLuu";
        btnLuu.Size = new Size(75, 25);
        btnLuu.TabIndex = 5;
        btnLuu.Text = "&Lưu";
        //
        // btnHuy
        //
        btnHuy.DialogResult = DialogResult.Cancel;
        btnHuy.Location = new Point(325, 160);
        btnHuy.Name = "btnHuy";
        btnHuy.Size = new Size(75, 25);
        btnHuy.TabIndex = 6;
        btnHuy.Text = "&Hủy";
        //
        // CanBoEditForm
        //
        AcceptButton = btnLuu;
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnHuy;
        ClientSize = new Size(414, 197);
        Controls.Add(btnHuy);
        Controls.Add(btnLuu);
        Controls.Add(chkDangCongTac);
        Controls.Add(chkLaQuanGiao);
        Controls.Add(txtChucVu);
        Controls.Add(lblChucVu);
        Controls.Add(txtHoTen);
        Controls.Add(lblHoTen);
        Controls.Add(txtMaCanBo);
        Controls.Add(lblMaCanBo);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "CanBoEditForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblMaCanBo;
    private TextBox txtMaCanBo;
    private Label lblHoTen;
    private TextBox txtHoTen;
    private Label lblChucVu;
    private TextBox txtChucVu;
    private CheckBox chkLaQuanGiao;
    private CheckBox chkDangCongTac;
    private Button btnLuu;
    private Button btnHuy;
}
