namespace LuuKyCanTin.WinForms.MasterData;

partial class AddInmateForm
{
    private System.ComponentModel.IContainer components = null!;
    private TextBox txtInmateCode = null!;
    private TextBox txtFullName = null!;
    private NumericUpDown numBirthYear = null!;
    private ComboBox cmbInmateType = null!;
    private DateTimePicker dtpAdmissionDate = null!;
    private TextBox txtCell = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private Label lblInmateCode = null!;
    private Label lblFullName = null!;
    private Label lblBirthYear = null!;
    private Label lblInmateType = null!;
    private Label lblAdmissionDate = null!;
    private Label lblCell = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        txtInmateCode = new TextBox();
        txtFullName = new TextBox();
        numBirthYear = new NumericUpDown();
        cmbInmateType = new ComboBox();
        dtpAdmissionDate = new DateTimePicker();
        txtCell = new TextBox();
        btnSave = new Button();
        btnCancel = new Button();
        lblInmateCode = new Label();
        lblFullName = new Label();
        lblBirthYear = new Label();
        lblInmateType = new Label();
        lblAdmissionDate = new Label();
        lblCell = new Label();
        ((System.ComponentModel.ISupportInitialize)numBirthYear).BeginInit();
        SuspendLayout();

        lblInmateCode.AutoSize = true;
        lblInmateCode.Location = new Point(20, 20);
        lblInmateCode.Text = "Mã số (*)";
        txtInmateCode.Location = new Point(140, 17);
        txtInmateCode.Size = new Size(240, 27);
        txtInmateCode.Name = "txtInmateCode";

        lblFullName.AutoSize = true;
        lblFullName.Location = new Point(20, 56);
        lblFullName.Text = "Họ tên (*)";
        txtFullName.Location = new Point(140, 53);
        txtFullName.Size = new Size(240, 27);
        txtFullName.Name = "txtFullName";

        lblBirthYear.AutoSize = true;
        lblBirthYear.Location = new Point(20, 92);
        lblBirthYear.Text = "Năm sinh";
        numBirthYear.Location = new Point(140, 89);
        numBirthYear.Maximum = 2100;
        numBirthYear.Size = new Size(100, 27);
        numBirthYear.Name = "numBirthYear";

        lblInmateType.AutoSize = true;
        lblInmateType.Location = new Point(20, 128);
        lblInmateType.Text = "Loại đối tượng (*)";
        cmbInmateType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbInmateType.Location = new Point(140, 125);
        cmbInmateType.Size = new Size(240, 27);
        cmbInmateType.Name = "cmbInmateType";

        lblAdmissionDate.AutoSize = true;
        lblAdmissionDate.Location = new Point(20, 164);
        lblAdmissionDate.Text = "Ngày vào (*)";
        dtpAdmissionDate.Format = DateTimePickerFormat.Short;
        dtpAdmissionDate.Location = new Point(140, 161);
        dtpAdmissionDate.Size = new Size(140, 27);
        dtpAdmissionDate.Name = "dtpAdmissionDate";

        lblCell.AutoSize = true;
        lblCell.Location = new Point(20, 200);
        lblCell.Text = "Buồng giam";
        txtCell.Location = new Point(140, 197);
        txtCell.Size = new Size(240, 27);
        txtCell.Name = "txtCell";

        btnSave.Location = new Point(140, 245);
        btnSave.Size = new Size(115, 32);
        btnSave.Text = "Lưu";
        btnSave.Click += OnSaveClicked;

        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(265, 245);
        btnCancel.Size = new Size(115, 32);
        btnCancel.Text = "Hủy";

        AcceptButton = btnSave;
        CancelButton = btnCancel;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(410, 300);
        Controls.Add(lblInmateCode);
        Controls.Add(txtInmateCode);
        Controls.Add(lblFullName);
        Controls.Add(txtFullName);
        Controls.Add(lblBirthYear);
        Controls.Add(numBirthYear);
        Controls.Add(lblInmateType);
        Controls.Add(cmbInmateType);
        Controls.Add(lblAdmissionDate);
        Controls.Add(dtpAdmissionDate);
        Controls.Add(lblCell);
        Controls.Add(txtCell);
        Controls.Add(btnSave);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thêm đối tượng";
        ((System.ComponentModel.ISupportInitialize)numBirthYear).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
