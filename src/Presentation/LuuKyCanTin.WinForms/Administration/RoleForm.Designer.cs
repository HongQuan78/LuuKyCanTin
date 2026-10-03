namespace LuuKyCanTin.WinForms.Administration;

partial class RoleForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblVaiTro = null!;
    private ListBox lstRoles = null!;
    private DataGridView grdPermissions = null!;
    private Label lblSpecialPermissions = null!;
    private CheckedListBox lstSpecialPermissions = null!;
    private Label lblMessage = null!;
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
        lblVaiTro = new Label();
        lstRoles = new ListBox();
        grdPermissions = new DataGridView();
        lblSpecialPermissions = new Label();
        lstSpecialPermissions = new CheckedListBox();
        lblMessage = new Label();
        btnSave = new Button();
        btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)grdPermissions).BeginInit();
        SuspendLayout();

        lblVaiTro.AutoSize = true;
        lblVaiTro.Location = new Point(12, 12);
        lblVaiTro.Text = "Vai trò";

        lstRoles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        lstRoles.IntegralHeight = false;
        lstRoles.Location = new Point(12, 34);
        lstRoles.Name = "lstRoles";
        lstRoles.Size = new Size(230, 470);
        lstRoles.TabIndex = 0;

        grdPermissions.AllowUserToAddRows = false;
        grdPermissions.AllowUserToDeleteRows = false;
        grdPermissions.AllowUserToResizeRows = false;
        grdPermissions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grdPermissions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grdPermissions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        grdPermissions.Location = new Point(260, 34);
        grdPermissions.Name = "grdPermissions";
        grdPermissions.RowHeadersVisible = false;
        grdPermissions.Size = new Size(660, 330);
        grdPermissions.TabIndex = 1;

        lblSpecialPermissions.AutoSize = true;
        lblSpecialPermissions.Location = new Point(260, 375);
        lblSpecialPermissions.Text = "Quyền đặc biệt";

        lstSpecialPermissions.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstSpecialPermissions.CheckOnClick = true;
        lstSpecialPermissions.FormattingEnabled = true;
        lstSpecialPermissions.Location = new Point(260, 398);
        lstSpecialPermissions.Name = "lstSpecialPermissions";
        lstSpecialPermissions.Size = new Size(660, 58);
        lstSpecialPermissions.TabIndex = 2;

        lblMessage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblMessage.Location = new Point(260, 468);
        lblMessage.Size = new Size(450, 30);
        lblMessage.Text = "";

        btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSave.Location = new Point(725, 465);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(90, 30);
        btnSave.TabIndex = 3;
        btnSave.Text = "Lưu";

        btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCancel.Location = new Point(830, 465);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(90, 30);
        btnCancel.TabIndex = 4;
        btnCancel.Text = "Huỷ thay đổi";

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(940, 516);
        Controls.Add(lblVaiTro);
        Controls.Add(lstRoles);
        Controls.Add(grdPermissions);
        Controls.Add(lblSpecialPermissions);
        Controls.Add(lstSpecialPermissions);
        Controls.Add(lblMessage);
        Controls.Add(btnSave);
        Controls.Add(btnCancel);
        MinimumSize = new Size(760, 420);
        Name = "RoleForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Vai trò và phân quyền";
        ((System.ComponentModel.ISupportInitialize)grdPermissions).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
