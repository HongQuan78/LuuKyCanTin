using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.MasterData;

partial class AddInmateForm
{
    private const int DialogWidth = 560;

    private System.ComponentModel.IContainer components = null!;
    private EditDialogLayout layout = null!;
    private Label lblHeading = null!;
    private Label lblSubtitle = null!;
    private Banner bnrError = null!;
    private Label lblInmateCode = null!;
    private InputFrame frmInmateCode = null!;
    private TextBox txtInmateCode = null!;
    private FieldError errInmateCode = null!;
    private Label lblBirthYear = null!;
    private InputFrame frmBirthYear = null!;
    private NumericUpDown numBirthYear = null!;
    private FieldError errBirthYear = null!;
    private Label lblFullName = null!;
    private InputFrame frmFullName = null!;
    private TextBox txtFullName = null!;
    private FieldError errFullName = null!;
    private Label lblInmateType = null!;
    private InputFrame frmInmateType = null!;
    private ComboBox cmbInmateType = null!;
    private FieldError errInmateType = null!;
    private Label lblAdmissionDate = null!;
    private InputFrame frmAdmissionDate = null!;
    private DateTimePicker dtpAdmissionDate = null!;
    private FieldError errAdmissionDate = null!;
    private Label lblCell = null!;
    private InputFrame frmCell = null!;
    private TextBox txtCell = null!;
    private FieldError errCell = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        layout = new EditDialogLayout(this, DialogWidth);
        lblHeading = new Label();
        lblSubtitle = new Label();
        bnrError = new Banner();
        lblInmateCode = new Label();
        frmInmateCode = new InputFrame();
        txtInmateCode = new TextBox();
        errInmateCode = new FieldError();
        lblBirthYear = new Label();
        frmBirthYear = new InputFrame();
        numBirthYear = new NumericUpDown();
        errBirthYear = new FieldError();
        lblFullName = new Label();
        frmFullName = new InputFrame();
        txtFullName = new TextBox();
        errFullName = new FieldError();
        lblInmateType = new Label();
        frmInmateType = new InputFrame();
        cmbInmateType = new ComboBox();
        errInmateType = new FieldError();
        lblAdmissionDate = new Label();
        frmAdmissionDate = new InputFrame();
        dtpAdmissionDate = new DateTimePicker();
        errAdmissionDate = new FieldError();
        lblCell = new Label();
        frmCell = new InputFrame();
        txtCell = new TextBox();
        errCell = new FieldError();
        btnSave = new Button();
        btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)numBirthYear).BeginInit();
        SuspendLayout();

        lblHeading.Text = "Thêm đối tượng";
        lblSubtitle.Text = "Nhập thông tin đối tượng mới tiếp nhận.";
        layout.SetHeading(lblHeading, lblSubtitle);

        bnrError.Name = "bnrError";
        layout.AddBanner(bnrError, row: 0);

        txtInmateCode.Name = "txtInmateCode";
        frmInmateCode.Name = "frmInmateCode";
        layout.AddField(lblInmateCode, "&Mã số *", frmInmateCode, txtInmateCode, errInmateCode, column: 0, row: 1);

        numBirthYear.Maximum = 2100;
        numBirthYear.Name = "numBirthYear";
        frmBirthYear.Name = "frmBirthYear";
        layout.AddField(lblBirthYear, "Năm &sinh", frmBirthYear, numBirthYear, errBirthYear, column: 1, row: 1);

        txtFullName.Name = "txtFullName";
        frmFullName.Name = "frmFullName";
        layout.AddField(lblFullName, "Họ &tên *", frmFullName, txtFullName, errFullName, column: 0, row: 2, isWide: true);

        cmbInmateType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbInmateType.Name = "cmbInmateType";
        frmInmateType.Name = "frmInmateType";
        layout.AddField(lblInmateType, "L&oại đối tượng *", frmInmateType, cmbInmateType, errInmateType, column: 0, row: 3);

        dtpAdmissionDate.CustomFormat = "dd/MM/yyyy";
        dtpAdmissionDate.Format = DateTimePickerFormat.Custom;
        dtpAdmissionDate.Name = "dtpAdmissionDate";
        frmAdmissionDate.Name = "frmAdmissionDate";
        layout.AddField(lblAdmissionDate, "Ngày &vào *", frmAdmissionDate, dtpAdmissionDate, errAdmissionDate, column: 1, row: 3);

        txtCell.Name = "txtCell";
        frmCell.Name = "frmCell";
        layout.AddField(lblCell, "&Buồng giam", frmCell, txtCell, errCell, column: 0, row: 4);

        btnSave.Name = "btnSave";
        btnSave.Text = "&Lưu";
        btnSave.Click += OnSaveClicked;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Name = "btnCancel";
        btnCancel.Text = "&Hủy";
        layout.AddButtons(btnSave, btnCancel);
        AppTheme.SetGlyph(btnSave, Glyphs.Save);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
        Name = "AddInmateForm";
        layout.ApplyTo("Thêm đối tượng");
        ((System.ComponentModel.ISupportInitialize)numBirthYear).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
