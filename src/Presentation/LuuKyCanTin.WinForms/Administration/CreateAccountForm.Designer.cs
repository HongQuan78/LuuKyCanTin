using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

partial class CreateAccountForm
{
    private const int DialogWidth = 480;
    private const int RolesListHeight = 132;

    private System.ComponentModel.IContainer components = null!;
    private EditDialogLayout layout = null!;
    private Label lblHeading = null!;
    private Label lblSubtitle = null!;
    private Banner bnrError = null!;
    private Label lblUserName = null!;
    private InputFrame frmUserName = null!;
    private TextBox txtUserName = null!;
    private FieldError errUserName = null!;
    private Label lblOfficer = null!;
    private InputFrame frmOfficer = null!;
    private ComboBox cboOfficer = null!;
    private FieldError errOfficer = null!;
    private Label lblRoles = null!;
    private CheckedListBox lstRoles = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
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
        lblUserName = new Label();
        frmUserName = new InputFrame();
        txtUserName = new TextBox();
        errUserName = new FieldError();
        lblOfficer = new Label();
        frmOfficer = new InputFrame();
        cboOfficer = new ComboBox();
        errOfficer = new FieldError();
        lblRoles = new Label();
        lstRoles = new CheckedListBox();
        btnSave = new Button();
        btnCancel = new Button();
        SuspendLayout();

        layout.SetHeading(lblHeading, lblSubtitle);
        lblHeading.Text = "Tạo tài khoản";
        lblSubtitle.Text = "Chọn cán bộ chưa có tài khoản và gán ít nhất một vai trò.";

        bnrError.Name = "bnrError";
        layout.AddBanner(bnrError, row: 0);

        txtUserName.MaxLength = 50;
        txtUserName.Name = "txtUserName";
        frmUserName.Name = "frmUserName";
        layout.AddField(lblUserName, "&Tên đăng nhập *", frmUserName, txtUserName, errUserName, column: 0, row: 1, isWide: true);

        cboOfficer.DropDownStyle = ComboBoxStyle.DropDownList;
        cboOfficer.Name = "cboOfficer";
        frmOfficer.Name = "frmOfficer";
        layout.AddField(lblOfficer, "&Cán bộ *", frmOfficer, cboOfficer, errOfficer, column: 0, row: 2, isWide: true);

        lstRoles.CheckOnClick = true;
        lstRoles.Height = RolesListHeight;
        lstRoles.IntegralHeight = false;
        lstRoles.Name = "lstRoles";
        layout.AddListField(lblRoles, "&Vai trò *", lstRoles, column: 0, row: 3, isWide: true);

        btnSave.Name = "btnSave";
        btnSave.Text = "&Tạo tài khoản";
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Name = "btnCancel";
        btnCancel.Text = "&Huỷ";
        layout.AddButtons(btnSave, btnCancel);
        AppTheme.SetGlyph(btnSave, Glyphs.Save);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
        Name = "CreateAccountForm";
        layout.ApplyTo("Tạo tài khoản");
        ResumeLayout(false);
        PerformLayout();
    }
}
