using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

partial class AccountRolesForm
{
    private const int DialogWidth = 480;
    private const int RolesListHeight = 168;

    private System.ComponentModel.IContainer components = null!;
    private EditDialogLayout layout = null!;
    private Label lblHeading = null!;
    private Label lblSubtitle = null!;
    private Banner bnrError = null!;
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
        lblRoles = new Label();
        lstRoles = new CheckedListBox();
        btnSave = new Button();
        btnCancel = new Button();
        SuspendLayout();

        layout.SetHeading(lblHeading, lblSubtitle);
        lblHeading.Text = "Phân vai trò";
        lblSubtitle.Text = "Chọn vai trò cấp cho tài khoản; bỏ chọn để thu quyền.";

        bnrError.Name = "bnrError";
        layout.AddBanner(bnrError, row: 0);

        lstRoles.CheckOnClick = true;
        lstRoles.Height = RolesListHeight;
        lstRoles.IntegralHeight = false;
        lstRoles.Name = "lstRoles";
        layout.AddListField(lblRoles, "&Vai trò *", lstRoles, column: 0, row: 1, isWide: true);

        btnSave.Name = "btnSave";
        btnSave.Text = "&Lưu";
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Name = "btnCancel";
        btnCancel.Text = "&Huỷ";
        layout.AddButtons(btnSave, btnCancel);
        AppTheme.SetGlyph(btnSave, Glyphs.Save);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
        Name = "AccountRolesForm";
        layout.ApplyTo("Phân vai trò");
        ResumeLayout(false);
        PerformLayout();
    }
}
