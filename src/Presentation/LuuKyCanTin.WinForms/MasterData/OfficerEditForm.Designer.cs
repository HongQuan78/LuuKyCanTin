using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.MasterData;

partial class OfficerEditForm
{
    private const int DialogWidth = 480;

    private System.ComponentModel.IContainer components = null!;
    private EditDialogLayout layout = null!;
    private Label lblHeading = null!;
    private Label lblSubtitle = null!;
    private Banner bnrError = null!;
    private Label lblOfficerCode = null!;
    private InputFrame frmOfficerCode = null!;
    private TextBox txtOfficerCode = null!;
    private FieldError errOfficerCode = null!;
    private Label lblPosition = null!;
    private InputFrame frmPosition = null!;
    private TextBox txtPosition = null!;
    private FieldError errPosition = null!;
    private Label lblFullName = null!;
    private InputFrame frmFullName = null!;
    private TextBox txtFullName = null!;
    private FieldError errFullName = null!;
    private CheckBox chkIsSupervisingOfficer = null!;
    private CheckBox chkIsActive = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        layout = new EditDialogLayout(this, DialogWidth);
        lblHeading = new Label();
        lblSubtitle = new Label();
        bnrError = new Banner();
        lblOfficerCode = new Label();
        frmOfficerCode = new InputFrame();
        txtOfficerCode = new TextBox();
        errOfficerCode = new FieldError();
        lblPosition = new Label();
        frmPosition = new InputFrame();
        txtPosition = new TextBox();
        errPosition = new FieldError();
        lblFullName = new Label();
        frmFullName = new InputFrame();
        txtFullName = new TextBox();
        errFullName = new FieldError();
        chkIsSupervisingOfficer = new CheckBox();
        chkIsActive = new CheckBox();
        btnSave = new Button();
        btnCancel = new Button();
        SuspendLayout();

        layout.SetHeading(lblHeading, lblSubtitle);

        bnrError.Name = "bnrError";
        layout.AddBanner(bnrError, row: 0);

        txtOfficerCode.CharacterCasing = CharacterCasing.Upper;
        txtOfficerCode.MaxLength = 20;
        txtOfficerCode.Name = "txtOfficerCode";
        frmOfficerCode.Name = "frmOfficerCode";
        layout.AddField(lblOfficerCode, "&Mã cán bộ *", frmOfficerCode, txtOfficerCode, errOfficerCode, column: 0, row: 1);

        txtPosition.MaxLength = 100;
        txtPosition.Name = "txtPosition";
        frmPosition.Name = "frmPosition";
        layout.AddField(lblPosition, "&Chức vụ", frmPosition, txtPosition, errPosition, column: 1, row: 1);

        txtFullName.MaxLength = 100;
        txtFullName.Name = "txtFullName";
        frmFullName.Name = "frmFullName";
        layout.AddField(lblFullName, "Họ &tên *", frmFullName, txtFullName, errFullName, column: 0, row: 2, isWide: true);

        chkIsSupervisingOfficer.AutoSize = true;
        chkIsSupervisingOfficer.Name = "chkIsSupervisingOfficer";
        chkIsSupervisingOfficer.Text = "Là cán bộ &quản giáo";
        layout.AddPlain(chkIsSupervisingOfficer, column: 0, row: 3);

        chkIsActive.AutoSize = true;
        chkIsActive.Name = "chkIsActive";
        chkIsActive.Text = "Đ&ang công tác";
        layout.AddPlain(chkIsActive, column: 1, row: 3);

        btnSave.Name = "btnSave";
        btnSave.Text = "&Lưu";
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Name = "btnCancel";
        btnCancel.Text = "&Hủy";
        layout.AddButtons(btnSave, btnCancel);
        AppTheme.SetGlyph(btnSave, Glyphs.Save);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
        Name = "OfficerEditForm";
        layout.ApplyTo("");
        ResumeLayout(false);
        PerformLayout();
    }
}
