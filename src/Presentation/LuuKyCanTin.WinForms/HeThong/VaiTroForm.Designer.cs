namespace LuuKyCanTin.WinForms.HeThong;

partial class VaiTroForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblVaiTro = null!;
    private ListBox lstVaiTro = null!;
    private DataGridView gridQuyen = null!;
    private Label lblQuyenDacBiet = null!;
    private CheckedListBox lstQuyenDacBiet = null!;
    private Label lblThongBao = null!;
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
        lstVaiTro = new ListBox();
        gridQuyen = new DataGridView();
        lblQuyenDacBiet = new Label();
        lstQuyenDacBiet = new CheckedListBox();
        lblThongBao = new Label();
        btnLuu = new Button();
        btnHuy = new Button();
        ((System.ComponentModel.ISupportInitialize)gridQuyen).BeginInit();
        SuspendLayout();

        lblVaiTro.AutoSize = true;
        lblVaiTro.Location = new Point(12, 12);
        lblVaiTro.Text = "Vai trò";

        lstVaiTro.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        lstVaiTro.IntegralHeight = false;
        lstVaiTro.Location = new Point(12, 34);
        lstVaiTro.Name = "lstVaiTro";
        lstVaiTro.Size = new Size(230, 470);
        lstVaiTro.TabIndex = 0;

        gridQuyen.AllowUserToAddRows = false;
        gridQuyen.AllowUserToDeleteRows = false;
        gridQuyen.AllowUserToResizeRows = false;
        gridQuyen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        gridQuyen.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridQuyen.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridQuyen.Location = new Point(260, 34);
        gridQuyen.Name = "gridQuyen";
        gridQuyen.RowHeadersVisible = false;
        gridQuyen.Size = new Size(660, 330);
        gridQuyen.TabIndex = 1;

        lblQuyenDacBiet.AutoSize = true;
        lblQuyenDacBiet.Location = new Point(260, 375);
        lblQuyenDacBiet.Text = "Quyền đặc biệt";

        lstQuyenDacBiet.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstQuyenDacBiet.CheckOnClick = true;
        lstQuyenDacBiet.FormattingEnabled = true;
        lstQuyenDacBiet.Location = new Point(260, 398);
        lstQuyenDacBiet.Name = "lstQuyenDacBiet";
        lstQuyenDacBiet.Size = new Size(660, 58);
        lstQuyenDacBiet.TabIndex = 2;

        lblThongBao.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblThongBao.Location = new Point(260, 468);
        lblThongBao.Size = new Size(450, 30);
        lblThongBao.Text = "";

        btnLuu.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnLuu.Location = new Point(725, 465);
        btnLuu.Name = "btnLuu";
        btnLuu.Size = new Size(90, 30);
        btnLuu.TabIndex = 3;
        btnLuu.Text = "Lưu";

        btnHuy.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnHuy.Location = new Point(830, 465);
        btnHuy.Name = "btnHuy";
        btnHuy.Size = new Size(90, 30);
        btnHuy.TabIndex = 4;
        btnHuy.Text = "Huỷ thay đổi";

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(940, 516);
        Controls.Add(lblVaiTro);
        Controls.Add(lstVaiTro);
        Controls.Add(gridQuyen);
        Controls.Add(lblQuyenDacBiet);
        Controls.Add(lstQuyenDacBiet);
        Controls.Add(lblThongBao);
        Controls.Add(btnLuu);
        Controls.Add(btnHuy);
        MinimumSize = new Size(760, 420);
        Name = "VaiTroForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Vai trò và phân quyền";
        ((System.ComponentModel.ISupportInitialize)gridQuyen).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
