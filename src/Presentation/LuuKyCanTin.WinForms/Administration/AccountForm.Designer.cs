using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

partial class AccountForm
{
    private System.ComponentModel.IContainer components = null!;
    private Panel pnlToolbar = null!;
    private FlowLayoutPanel pnlActions = null!;
    private Button btnAdd = null!;
    private Button btnRoles = null!;
    private Button btnToggleActive = null!;
    private Button btnUnlock = null!;
    private Button btnResetPassword = null!;
    private Banner bnrMessage = null!;
    private CardPanel crdList = null!;
    private DataGridView grdAccounts = null!;
    private DataGridViewTextBoxColumn colUserName = null!;
    private DataGridViewTextBoxColumn colOfficer = null!;
    private DataGridViewTextBoxColumn colRoles = null!;
    private DataGridViewTextBoxColumn colStatus = null!;
    private DataGridViewTextBoxColumn colLocked = null!;
    private Panel pnlFooter = null!;
    private Label lblCount = null!;
    private Label lblHints = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        pnlToolbar = new Panel();
        pnlActions = new FlowLayoutPanel();
        btnAdd = new Button();
        btnRoles = new Button();
        btnToggleActive = new Button();
        btnUnlock = new Button();
        btnResetPassword = new Button();
        bnrMessage = new Banner();
        crdList = new CardPanel();
        grdAccounts = new DataGridView();
        colUserName = new DataGridViewTextBoxColumn();
        colOfficer = new DataGridViewTextBoxColumn();
        colRoles = new DataGridViewTextBoxColumn();
        colStatus = new DataGridViewTextBoxColumn();
        colLocked = new DataGridViewTextBoxColumn();
        pnlFooter = new Panel();
        lblCount = new Label();
        lblHints = new Label();
        ((System.ComponentModel.ISupportInitialize)grdAccounts).BeginInit();
        pnlToolbar.SuspendLayout();
        crdList.SuspendLayout();
        SuspendLayout();

        // Toolbar row: the actions sit on the right, with the one primary button last.
        pnlToolbar.Dock = DockStyle.Top;
        pnlToolbar.Height = AppTheme.ButtonHeight + AppTheme.Gap;
        pnlToolbar.Name = "pnlToolbar";
        pnlToolbar.TabIndex = 0;
        pnlToolbar.Controls.Add(pnlActions);

        // RightToLeft flow: the primary button is added first so it ends up last on the right.
        pnlActions.AutoSize = true;
        pnlActions.Dock = DockStyle.Right;
        pnlActions.FlowDirection = FlowDirection.RightToLeft;
        pnlActions.Margin = Padding.Empty;
        pnlActions.Name = "pnlActions";
        pnlActions.TabIndex = 0;
        pnlActions.WrapContents = false;
        pnlActions.Controls.Add(btnAdd);
        pnlActions.Controls.Add(btnResetPassword);
        pnlActions.Controls.Add(btnUnlock);
        pnlActions.Controls.Add(btnToggleActive);
        pnlActions.Controls.Add(btnRoles);

        btnAdd.AutoSize = true;
        btnAdd.Margin = Padding.Empty;
        btnAdd.Name = "btnAdd";
        btnAdd.TabIndex = 4;
        btnAdd.Text = "&Thêm tài khoản";
        AppTheme.StylePrimary(btnAdd);
        AppTheme.SetGlyph(btnAdd, Glyphs.Add);

        btnRoles.AutoSize = true;
        btnRoles.Margin = Padding.Empty;
        btnRoles.Name = "btnRoles";
        btnRoles.TabIndex = 0;
        btnRoles.Text = "Phân &vai trò";
        AppTheme.StyleSecondary(btnRoles);
        AppTheme.SetGlyph(btnRoles, Glyphs.Contact);

        btnToggleActive.AutoSize = true;
        btnToggleActive.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        btnToggleActive.Name = "btnToggleActive";
        btnToggleActive.TabIndex = 1;
        btnToggleActive.Text = "&Ngừng hoạt động";
        AppTheme.StyleSecondary(btnToggleActive);
        AppTheme.SetGlyph(btnToggleActive, Glyphs.Cancel);

        btnUnlock.AutoSize = true;
        btnUnlock.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        btnUnlock.Name = "btnUnlock";
        btnUnlock.TabIndex = 2;
        btnUnlock.Text = "&Mở khoá";
        AppTheme.StyleSecondary(btnUnlock);
        AppTheme.SetGlyph(btnUnlock, Glyphs.Lock);

        btnResetPassword.AutoSize = true;
        btnResetPassword.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        btnResetPassword.Name = "btnResetPassword";
        btnResetPassword.TabIndex = 3;
        btnResetPassword.Text = "Đặt &lại mật khẩu";
        AppTheme.StyleSecondary(btnResetPassword);
        AppTheme.SetGlyph(btnResetPassword, Glyphs.Refresh);

        bnrMessage.Dock = DockStyle.Top;
        bnrMessage.Name = "bnrMessage";
        bnrMessage.TabIndex = 1;

        // The card holds the grid edge to edge (inside its 1px border) and the 44px footer.
        crdList.Dock = DockStyle.Fill;
        crdList.Name = "crdList";
        crdList.Padding = new Padding(1);
        crdList.TabIndex = 2;
        crdList.Controls.Add(grdAccounts);
        crdList.Controls.Add(pnlFooter);

        grdAccounts.AllowUserToAddRows = false;
        grdAccounts.AllowUserToDeleteRows = false;
        grdAccounts.AllowUserToResizeRows = false;
        grdAccounts.AutoGenerateColumns = false;
        grdAccounts.Columns.AddRange(new DataGridViewColumn[] { colUserName, colOfficer, colRoles, colStatus, colLocked });
        grdAccounts.Dock = DockStyle.Fill;
        grdAccounts.MultiSelect = false;
        grdAccounts.Name = "grdAccounts";
        grdAccounts.ReadOnly = true;
        grdAccounts.StandardTab = true;
        grdAccounts.TabIndex = 0;
        AppTheme.StyleGrid(grdAccounts);

        colUserName.DefaultCellStyle.Font = AppTheme.BodySemiboldFont;
        colUserName.HeaderText = "Tên đăng nhập";
        colUserName.Name = "colUserName";
        colUserName.Width = 170;

        colOfficer.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colOfficer.HeaderText = "Cán bộ";
        colOfficer.Name = "colOfficer";

        colRoles.HeaderText = "Vai trò";
        colRoles.Name = "colRoles";
        colRoles.Width = 300;

        colStatus.HeaderText = "Hoạt động";
        colStatus.Name = "colStatus";
        colStatus.Width = 162;

        colLocked.HeaderText = "Bị khoá";
        colLocked.Name = "colLocked";
        colLocked.Width = 120;

        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Height = AppTheme.CardFooterHeight;
        pnlFooter.Name = "pnlFooter";
        // The top padding keeps the 1px border line clear of the labels. Dock order is reverse add order: the hints
        // take the right first, the count fills what is left.
        pnlFooter.Padding = new Padding(AppTheme.CardPadding, 1, AppTheme.CardPadding, 0);
        pnlFooter.Controls.Add(lblCount);
        pnlFooter.Controls.Add(lblHints);
        pnlFooter.Paint += (_, e) =>
        {
            using var line = new SolidBrush(AppTheme.Border);
            e.Graphics.FillRectangle(line, 0, 0, pnlFooter.Width, 1);
        };

        lblCount.AutoSize = false;
        lblCount.Dock = DockStyle.Fill;
        lblCount.ForeColor = AppTheme.Muted;
        lblCount.Name = "lblCount";
        lblCount.TextAlign = ContentAlignment.MiddleLeft;
        lblCount.UseMnemonic = false;

        lblHints.AutoSize = false;
        lblHints.Dock = DockStyle.Right;
        lblHints.Width = 280;
        lblHints.ForeColor = AppTheme.Muted;
        lblHints.Name = "lblHints";
        lblHints.Text = "Insert thêm · Enter phân vai trò";
        lblHints.TextAlign = ContentAlignment.MiddleRight;
        lblHints.UseMnemonic = false;

        // Dock order is reverse add order: the toolbar takes the top, then the banner, then the card fills the rest.
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Surface;
        Controls.Add(crdList);
        Controls.Add(bnrMessage);
        Controls.Add(pnlToolbar);
        Font = AppTheme.BodyFont;
        Name = "AccountForm";
        Size = new Size(1106, 615);
        ((System.ComponentModel.ISupportInitialize)grdAccounts).EndInit();
        pnlToolbar.ResumeLayout(false);
        pnlToolbar.PerformLayout();
        crdList.ResumeLayout(false);
        ResumeLayout(false);
    }
}
