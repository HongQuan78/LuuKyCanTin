using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

partial class RoleForm
{
    private const int RoleListWidth = 260;
    private const int SpecialPermissionsHeight = 96;

    private System.ComponentModel.IContainer components = null!;
    private CardPanel crdRoles = null!;
    private DataGridView grdRoles = null!;
    private DataGridViewTextBoxColumn colRoleName = null!;
    private Panel pnlColumnGap = null!;
    private CardPanel crdPermissions = null!;
    private Banner bnrMessage = null!;
    private Panel pnlBannerGap = null!;
    private DataGridView grdPermissions = null!;
    private Panel pnlSpecialPermissions = null!;
    private Label lblSpecialPermissions = null!;
    private CheckedListBox lstSpecialPermissions = null!;
    private FlowLayoutPanel pnlFooter = null!;
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
        crdRoles = new CardPanel();
        grdRoles = new DataGridView();
        colRoleName = new DataGridViewTextBoxColumn();
        pnlColumnGap = new Panel();
        crdPermissions = new CardPanel();
        bnrMessage = new Banner();
        pnlBannerGap = new Panel();
        grdPermissions = new DataGridView();
        pnlSpecialPermissions = new Panel();
        lblSpecialPermissions = new Label();
        lstSpecialPermissions = new CheckedListBox();
        pnlFooter = new FlowLayoutPanel();
        btnSave = new Button();
        btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)grdRoles).BeginInit();
        ((System.ComponentModel.ISupportInitialize)grdPermissions).BeginInit();
        crdRoles.SuspendLayout();
        crdPermissions.SuspendLayout();
        SuspendLayout();

        // ---- Left: the role list, 260px; the list runs edge to edge under the card header ----------------------
        crdRoles.Dock = DockStyle.Left;
        crdRoles.HeaderText = "Vai trò";
        crdRoles.Name = "crdRoles";
        crdRoles.Padding = new Padding(1, AppTheme.CardHeaderHeight, 1, 1);
        crdRoles.TabIndex = 0;
        crdRoles.Width = RoleListWidth;
        crdRoles.Controls.Add(grdRoles);

        grdRoles.AllowUserToAddRows = false;
        grdRoles.AllowUserToDeleteRows = false;
        grdRoles.AllowUserToResizeRows = false;
        grdRoles.AutoGenerateColumns = false;
        grdRoles.Columns.Add(colRoleName);
        grdRoles.Dock = DockStyle.Fill;
        grdRoles.MultiSelect = false;
        grdRoles.Name = "grdRoles";
        grdRoles.ReadOnly = true;
        grdRoles.StandardTab = true;
        grdRoles.TabIndex = 0;
        AppTheme.StyleGrid(grdRoles);
        grdRoles.ColumnHeadersVisible = false;

        colRoleName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colRoleName.Name = "colRoleName";

        pnlColumnGap.Dock = DockStyle.Left;
        pnlColumnGap.Name = "pnlColumnGap";
        pnlColumnGap.TabStop = false;
        pnlColumnGap.Width = AppTheme.Gap;

        // ---- Right: the selected role's permissions, then the footer with the one primary button last -----------
        crdPermissions.Dock = DockStyle.Fill;
        crdPermissions.HeaderText = " ";
        crdPermissions.Name = "crdPermissions";
        crdPermissions.TabIndex = 1;
        // Dock order is reverse add order: the footer and special list sit at the bottom, the grid fills the middle.
        crdPermissions.Controls.Add(grdPermissions);
        crdPermissions.Controls.Add(pnlBannerGap);
        crdPermissions.Controls.Add(bnrMessage);
        crdPermissions.Controls.Add(pnlSpecialPermissions);
        crdPermissions.Controls.Add(pnlFooter);

        bnrMessage.Dock = DockStyle.Top;
        bnrMessage.Name = "bnrMessage";
        bnrMessage.VisibleChanged += (_, _) => pnlBannerGap.Visible = bnrMessage.Visible;

        pnlBannerGap.Dock = DockStyle.Top;
        pnlBannerGap.Height = 12;
        pnlBannerGap.Name = "pnlBannerGap";
        pnlBannerGap.TabStop = false;
        pnlBannerGap.Visible = false;

        grdPermissions.AllowUserToAddRows = false;
        grdPermissions.AllowUserToDeleteRows = false;
        grdPermissions.AllowUserToResizeRows = false;
        grdPermissions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grdPermissions.Dock = DockStyle.Fill;
        grdPermissions.MultiSelect = false;
        grdPermissions.Name = "grdPermissions";
        grdPermissions.StandardTab = true;
        grdPermissions.TabIndex = 0;
        AppTheme.StyleGrid(grdPermissions);

        pnlSpecialPermissions.Dock = DockStyle.Bottom;
        pnlSpecialPermissions.Height = SpecialPermissionsHeight;
        pnlSpecialPermissions.Name = "pnlSpecialPermissions";
        pnlSpecialPermissions.Padding = new Padding(0, AppTheme.Gap, 0, 0);
        pnlSpecialPermissions.TabIndex = 1;
        pnlSpecialPermissions.Controls.Add(lstSpecialPermissions);
        pnlSpecialPermissions.Controls.Add(lblSpecialPermissions);

        lblSpecialPermissions.AutoSize = true;
        lblSpecialPermissions.Dock = DockStyle.Top;
        lblSpecialPermissions.Font = AppTheme.LabelFont;
        lblSpecialPermissions.ForeColor = AppTheme.Label;
        lblSpecialPermissions.Name = "lblSpecialPermissions";
        lblSpecialPermissions.Padding = new Padding(0, 0, 0, AppTheme.LabelGap);
        lblSpecialPermissions.Text = "Quyền đặc biệt";
        lblSpecialPermissions.UseMnemonic = false;

        lstSpecialPermissions.BackColor = AppTheme.Card;
        lstSpecialPermissions.CheckOnClick = true;
        lstSpecialPermissions.Dock = DockStyle.Fill;
        lstSpecialPermissions.ForeColor = AppTheme.Text;
        lstSpecialPermissions.FormattingEnabled = true;
        lstSpecialPermissions.Name = "lstSpecialPermissions";
        lstSpecialPermissions.TabIndex = 0;

        pnlFooter.AutoSize = true;
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.FlowDirection = FlowDirection.RightToLeft;
        pnlFooter.Name = "pnlFooter";
        pnlFooter.Padding = new Padding(0, AppTheme.Gap, 0, 0);
        pnlFooter.TabIndex = 2;
        pnlFooter.WrapContents = false;
        pnlFooter.Controls.Add(btnSave);
        pnlFooter.Controls.Add(btnCancel);

        btnSave.AutoSize = true;
        btnSave.Margin = Padding.Empty;
        btnSave.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnSave.Name = "btnSave";
        btnSave.TabIndex = 1;
        btnSave.Text = "&Lưu";
        AppTheme.StylePrimary(btnSave);
        AppTheme.SetGlyph(btnSave, Glyphs.Save);

        btnCancel.AutoSize = true;
        btnCancel.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        btnCancel.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnCancel.Name = "btnCancel";
        btnCancel.TabIndex = 0;
        btnCancel.Text = "&Huỷ thay đổi";
        AppTheme.StyleSecondary(btnCancel);

        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Surface;
        Controls.Add(crdPermissions);
        Controls.Add(pnlColumnGap);
        Controls.Add(crdRoles);
        Font = AppTheme.BodyFont;
        Name = "RoleForm";
        Size = new Size(1106, 615);
        ((System.ComponentModel.ISupportInitialize)grdRoles).EndInit();
        ((System.ComponentModel.ISupportInitialize)grdPermissions).EndInit();
        crdRoles.ResumeLayout(false);
        crdPermissions.ResumeLayout(false);
        crdPermissions.PerformLayout();
        ResumeLayout(false);
    }
}
