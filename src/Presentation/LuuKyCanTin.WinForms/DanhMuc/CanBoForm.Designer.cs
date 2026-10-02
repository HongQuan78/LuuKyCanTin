namespace LuuKyCanTin.WinForms.DanhMuc;

partial class CanBoForm
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
        lblTuKhoa = new Label();
        txtTuKhoa = new TextBox();
        chkHienCaNguoiDaNghi = new CheckBox();
        btnThem = new Button();
        btnSua = new Button();
        gridCanBo = new DataGridView();
        colMaCanBo = new DataGridViewTextBoxColumn();
        colHoTen = new DataGridViewTextBoxColumn();
        colChucVu = new DataGridViewTextBoxColumn();
        colLaQuanGiao = new DataGridViewCheckBoxColumn();
        colDangCongTac = new DataGridViewCheckBoxColumn();
        timerTimKiem = new System.Windows.Forms.Timer(components);
        ((System.ComponentModel.ISupportInitialize)gridCanBo).BeginInit();
        SuspendLayout();
        //
        // lblTuKhoa
        //
        lblTuKhoa.AutoSize = true;
        lblTuKhoa.Location = new Point(12, 15);
        lblTuKhoa.Name = "lblTuKhoa";
        lblTuKhoa.Text = "Tìm (mã hoặc tên):";
        //
        // txtTuKhoa
        //
        txtTuKhoa.Location = new Point(130, 12);
        txtTuKhoa.Name = "txtTuKhoa";
        txtTuKhoa.Size = new Size(260, 23);
        txtTuKhoa.TabIndex = 0;
        //
        // chkHienCaNguoiDaNghi
        //
        chkHienCaNguoiDaNghi.AutoSize = true;
        chkHienCaNguoiDaNghi.Location = new Point(405, 14);
        chkHienCaNguoiDaNghi.Name = "chkHienCaNguoiDaNghi";
        chkHienCaNguoiDaNghi.TabIndex = 1;
        chkHienCaNguoiDaNghi.Text = "Hiện cả người đã nghỉ";
        //
        // btnThem
        //
        btnThem.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnThem.Location = new Point(616, 11);
        btnThem.Name = "btnThem";
        btnThem.Size = new Size(75, 25);
        btnThem.TabIndex = 2;
        btnThem.Text = "&Thêm";
        //
        // btnSua
        //
        btnSua.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSua.Location = new Point(697, 11);
        btnSua.Name = "btnSua";
        btnSua.Size = new Size(75, 25);
        btnSua.TabIndex = 3;
        btnSua.Text = "&Sửa";
        //
        // gridCanBo
        //
        gridCanBo.AllowUserToAddRows = false;
        gridCanBo.AllowUserToDeleteRows = false;
        gridCanBo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        gridCanBo.AutoGenerateColumns = false;
        gridCanBo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridCanBo.Columns.AddRange(new DataGridViewColumn[] { colMaCanBo, colHoTen, colChucVu, colLaQuanGiao, colDangCongTac });
        gridCanBo.Location = new Point(12, 45);
        gridCanBo.MultiSelect = false;
        gridCanBo.Name = "gridCanBo";
        gridCanBo.ReadOnly = true;
        gridCanBo.RowHeadersVisible = false;
        gridCanBo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridCanBo.Size = new Size(760, 404);
        gridCanBo.TabIndex = 4;
        //
        // colMaCanBo
        //
        colMaCanBo.FillWeight = 15F;
        colMaCanBo.HeaderText = "Mã";
        colMaCanBo.Name = "colMaCanBo";
        //
        // colHoTen
        //
        colHoTen.FillWeight = 35F;
        colHoTen.HeaderText = "Họ tên";
        colHoTen.Name = "colHoTen";
        //
        // colChucVu
        //
        colChucVu.FillWeight = 30F;
        colChucVu.HeaderText = "Chức vụ";
        colChucVu.Name = "colChucVu";
        //
        // colLaQuanGiao
        //
        colLaQuanGiao.FillWeight = 10F;
        colLaQuanGiao.HeaderText = "Quản giáo";
        colLaQuanGiao.Name = "colLaQuanGiao";
        //
        // colDangCongTac
        //
        colDangCongTac.FillWeight = 10F;
        colDangCongTac.HeaderText = "Đang công tác";
        colDangCongTac.Name = "colDangCongTac";
        //
        // timerTimKiem
        //
        timerTimKiem.Interval = 300;
        //
        // CanBoForm
        //
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 461);
        Controls.Add(gridCanBo);
        Controls.Add(btnSua);
        Controls.Add(btnThem);
        Controls.Add(chkHienCaNguoiDaNghi);
        Controls.Add(txtTuKhoa);
        Controls.Add(lblTuKhoa);
        MinimumSize = new Size(640, 360);
        Name = "CanBoForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Danh mục cán bộ";
        ((System.ComponentModel.ISupportInitialize)gridCanBo).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTuKhoa;
    private TextBox txtTuKhoa;
    private CheckBox chkHienCaNguoiDaNghi;
    private Button btnThem;
    private Button btnSua;
    private DataGridView gridCanBo;
    private DataGridViewTextBoxColumn colMaCanBo;
    private DataGridViewTextBoxColumn colHoTen;
    private DataGridViewTextBoxColumn colChucVu;
    private DataGridViewCheckBoxColumn colLaQuanGiao;
    private DataGridViewCheckBoxColumn colDangCongTac;
    private System.Windows.Forms.Timer timerTimKiem;
}
