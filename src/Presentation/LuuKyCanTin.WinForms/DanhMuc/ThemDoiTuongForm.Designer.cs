namespace LuuKyCanTin.WinForms.DanhMuc;

partial class ThemDoiTuongForm
{
    private System.ComponentModel.IContainer components = null!;
    private TextBox txtMaSo = null!;
    private TextBox txtHoTen = null!;
    private NumericUpDown numNamSinh = null!;
    private ComboBox cmbLoaiDoiTuong = null!;
    private DateTimePicker dtpNgayVao = null!;
    private TextBox txtBuongGiam = null!;
    private Button btnLuu = null!;
    private Button btnHuy = null!;
    private Label lblMaSo = null!;
    private Label lblHoTen = null!;
    private Label lblNamSinh = null!;
    private Label lblLoaiDoiTuong = null!;
    private Label lblNgayVao = null!;
    private Label lblBuongGiam = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        txtMaSo = new TextBox();
        txtHoTen = new TextBox();
        numNamSinh = new NumericUpDown();
        cmbLoaiDoiTuong = new ComboBox();
        dtpNgayVao = new DateTimePicker();
        txtBuongGiam = new TextBox();
        btnLuu = new Button();
        btnHuy = new Button();
        lblMaSo = new Label();
        lblHoTen = new Label();
        lblNamSinh = new Label();
        lblLoaiDoiTuong = new Label();
        lblNgayVao = new Label();
        lblBuongGiam = new Label();
        ((System.ComponentModel.ISupportInitialize)numNamSinh).BeginInit();
        SuspendLayout();

        lblMaSo.AutoSize = true;
        lblMaSo.Location = new Point(20, 20);
        lblMaSo.Text = "Mã số (*)";
        txtMaSo.Location = new Point(140, 17);
        txtMaSo.Size = new Size(240, 27);
        txtMaSo.Name = "txtMaSo";

        lblHoTen.AutoSize = true;
        lblHoTen.Location = new Point(20, 56);
        lblHoTen.Text = "Họ tên (*)";
        txtHoTen.Location = new Point(140, 53);
        txtHoTen.Size = new Size(240, 27);
        txtHoTen.Name = "txtHoTen";

        lblNamSinh.AutoSize = true;
        lblNamSinh.Location = new Point(20, 92);
        lblNamSinh.Text = "Năm sinh";
        numNamSinh.Location = new Point(140, 89);
        numNamSinh.Maximum = 2100;
        numNamSinh.Size = new Size(100, 27);
        numNamSinh.Name = "numNamSinh";

        lblLoaiDoiTuong.AutoSize = true;
        lblLoaiDoiTuong.Location = new Point(20, 128);
        lblLoaiDoiTuong.Text = "Loại đối tượng (*)";
        cmbLoaiDoiTuong.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLoaiDoiTuong.Location = new Point(140, 125);
        cmbLoaiDoiTuong.Size = new Size(240, 27);
        cmbLoaiDoiTuong.Name = "cmbLoaiDoiTuong";

        lblNgayVao.AutoSize = true;
        lblNgayVao.Location = new Point(20, 164);
        lblNgayVao.Text = "Ngày vào (*)";
        dtpNgayVao.Format = DateTimePickerFormat.Short;
        dtpNgayVao.Location = new Point(140, 161);
        dtpNgayVao.Size = new Size(140, 27);
        dtpNgayVao.Name = "dtpNgayVao";

        lblBuongGiam.AutoSize = true;
        lblBuongGiam.Location = new Point(20, 200);
        lblBuongGiam.Text = "Buồng giam";
        txtBuongGiam.Location = new Point(140, 197);
        txtBuongGiam.Size = new Size(240, 27);
        txtBuongGiam.Name = "txtBuongGiam";

        btnLuu.Location = new Point(140, 245);
        btnLuu.Size = new Size(115, 32);
        btnLuu.Text = "Lưu";
        btnLuu.Click += OnLuuBam;

        btnHuy.DialogResult = DialogResult.Cancel;
        btnHuy.Location = new Point(265, 245);
        btnHuy.Size = new Size(115, 32);
        btnHuy.Text = "Hủy";

        AcceptButton = btnLuu;
        CancelButton = btnHuy;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(410, 300);
        Controls.Add(lblMaSo);
        Controls.Add(txtMaSo);
        Controls.Add(lblHoTen);
        Controls.Add(txtHoTen);
        Controls.Add(lblNamSinh);
        Controls.Add(numNamSinh);
        Controls.Add(lblLoaiDoiTuong);
        Controls.Add(cmbLoaiDoiTuong);
        Controls.Add(lblNgayVao);
        Controls.Add(dtpNgayVao);
        Controls.Add(lblBuongGiam);
        Controls.Add(txtBuongGiam);
        Controls.Add(btnLuu);
        Controls.Add(btnHuy);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thêm đối tượng";
        ((System.ComponentModel.ISupportInitialize)numNamSinh).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
