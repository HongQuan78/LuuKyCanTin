using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.MasterData;

partial class OfficerForm
{
    private const int StatusFilterWidth = 230;

    private System.ComponentModel.IContainer components = null!;
    private Panel pnlToolbar = null!;
    private FlowLayoutPanel pnlFilters = null!;
    private InputFrame frmKeyword = null!;
    private TextBox txtKeyword = null!;
    private InputFrame frmStatusFilter = null!;
    private ComboBox cboStatusFilter = null!;
    private FlowLayoutPanel pnlActions = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private CardPanel crdList = null!;
    private DataGridView grdOfficers = null!;
    private DataGridViewTextBoxColumn colOfficerCode = null!;
    private DataGridViewTextBoxColumn colFullName = null!;
    private DataGridViewTextBoxColumn colPosition = null!;
    private DataGridViewCheckBoxColumn colIsSupervisingOfficer = null!;
    private DataGridViewTextBoxColumn colStatus = null!;
    private Panel pnlFooter = null!;
    private Label lblCount = null!;
    private Label lblHints = null!;
    private System.Windows.Forms.Timer tmrSearch = null!;

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
        pnlToolbar = new Panel();
        pnlFilters = new FlowLayoutPanel();
        frmKeyword = new InputFrame();
        txtKeyword = new TextBox();
        frmStatusFilter = new InputFrame();
        cboStatusFilter = new ComboBox();
        pnlActions = new FlowLayoutPanel();
        btnAdd = new Button();
        btnEdit = new Button();
        crdList = new CardPanel();
        grdOfficers = new DataGridView();
        colOfficerCode = new DataGridViewTextBoxColumn();
        colFullName = new DataGridViewTextBoxColumn();
        colPosition = new DataGridViewTextBoxColumn();
        colIsSupervisingOfficer = new DataGridViewCheckBoxColumn();
        colStatus = new DataGridViewTextBoxColumn();
        pnlFooter = new Panel();
        lblCount = new Label();
        lblHints = new Label();
        tmrSearch = new System.Windows.Forms.Timer(components);
        ((System.ComponentModel.ISupportInitialize)grdOfficers).BeginInit();
        pnlToolbar.SuspendLayout();
        crdList.SuspendLayout();
        SuspendLayout();

        // Toolbar row: search, inline-label filter, spacer, secondary button, then the one primary button last.
        pnlToolbar.Dock = DockStyle.Top;
        pnlToolbar.Height = AppTheme.ButtonHeight + AppTheme.Gap;
        pnlToolbar.Name = "pnlToolbar";
        pnlToolbar.TabIndex = 0;
        pnlToolbar.Controls.Add(pnlFilters);
        pnlToolbar.Controls.Add(pnlActions);

        pnlFilters.Dock = DockStyle.Fill;
        pnlFilters.Margin = Padding.Empty;
        pnlFilters.Name = "pnlFilters";
        pnlFilters.TabIndex = 0;
        pnlFilters.WrapContents = false;
        pnlFilters.Controls.Add(frmKeyword);
        pnlFilters.Controls.Add(frmStatusFilter);

        txtKeyword.Name = "txtKeyword";
        frmKeyword.Glyph = Glyphs.Search;
        frmKeyword.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        frmKeyword.Name = "frmKeyword";
        frmKeyword.TabIndex = 0;
        frmKeyword.Width = AppTheme.SearchWidth;
        frmKeyword.Inner = txtKeyword;

        cboStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStatusFilter.Items.AddRange(["Đang công tác", "Tất cả"]);
        cboStatusFilter.Name = "cboStatusFilter";
        cboStatusFilter.SelectedIndex = 0;
        frmStatusFilter.InlineLabel = "Trạng thái:";
        frmStatusFilter.Margin = Padding.Empty;
        frmStatusFilter.Name = "frmStatusFilter";
        frmStatusFilter.TabIndex = 1;
        frmStatusFilter.Width = StatusFilterWidth;
        frmStatusFilter.Inner = cboStatusFilter;

        // RightToLeft flow: the primary button is added first so it ends up last on the right.
        pnlActions.AutoSize = true;
        pnlActions.Dock = DockStyle.Right;
        pnlActions.FlowDirection = FlowDirection.RightToLeft;
        pnlActions.Margin = Padding.Empty;
        pnlActions.Name = "pnlActions";
        pnlActions.TabIndex = 1;
        pnlActions.WrapContents = false;
        pnlActions.Controls.Add(btnAdd);
        pnlActions.Controls.Add(btnEdit);

        btnAdd.AutoSize = true;
        btnAdd.Margin = Padding.Empty;
        btnAdd.Name = "btnAdd";
        btnAdd.TabIndex = 1;
        btnAdd.Text = "&Thêm cán bộ";
        AppTheme.StylePrimary(btnAdd);
        AppTheme.SetGlyph(btnAdd, Glyphs.Add);

        btnEdit.AutoSize = true;
        btnEdit.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        btnEdit.Name = "btnEdit";
        btnEdit.TabIndex = 0;
        btnEdit.Text = "&Sửa";
        AppTheme.StyleSecondary(btnEdit);
        AppTheme.SetGlyph(btnEdit, Glyphs.Edit);

        // The card holds the grid edge to edge (inside its 1px border) and the 44px footer.
        crdList.Dock = DockStyle.Fill;
        crdList.Name = "crdList";
        crdList.Padding = new Padding(1);
        crdList.TabIndex = 1;
        crdList.Controls.Add(grdOfficers);
        crdList.Controls.Add(pnlFooter);

        grdOfficers.AllowUserToAddRows = false;
        grdOfficers.AllowUserToDeleteRows = false;
        grdOfficers.AllowUserToResizeRows = false;
        grdOfficers.AutoGenerateColumns = false;
        grdOfficers.Columns.AddRange(new DataGridViewColumn[] { colOfficerCode, colFullName, colPosition, colIsSupervisingOfficer, colStatus });
        grdOfficers.Dock = DockStyle.Fill;
        grdOfficers.MultiSelect = false;
        grdOfficers.Name = "grdOfficers";
        grdOfficers.ReadOnly = true;
        grdOfficers.StandardTab = true;
        grdOfficers.TabIndex = 0;
        AppTheme.StyleGrid(grdOfficers);

        colOfficerCode.DefaultCellStyle.ForeColor = AppTheme.Text2;
        colOfficerCode.HeaderText = "Mã";
        colOfficerCode.Name = "colOfficerCode";
        colOfficerCode.Width = 110;

        // The name is the only Fill column, semibold as DESIGN.md asks for names in grids.
        colFullName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colFullName.DefaultCellStyle.Font = AppTheme.BodySemiboldFont;
        colFullName.HeaderText = "Họ tên";
        colFullName.Name = "colFullName";

        colPosition.HeaderText = "Chức vụ";
        colPosition.Name = "colPosition";
        colPosition.Width = 220;

        colIsSupervisingOfficer.HeaderText = "Quản giáo";
        colIsSupervisingOfficer.Name = "colIsSupervisingOfficer";
        colIsSupervisingOfficer.Width = 100;

        colStatus.HeaderText = "Trạng thái";
        colStatus.Name = "colStatus";
        colStatus.Width = 162;

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
        lblHints.Text = "Insert thêm · Enter sửa";
        lblHints.TextAlign = ContentAlignment.MiddleRight;
        lblHints.UseMnemonic = false;

        tmrSearch.Interval = 300;

        // Dock order is reverse add order: the toolbar takes the top, the card fills the rest.
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Surface;
        Controls.Add(crdList);
        Controls.Add(pnlToolbar);
        Font = AppTheme.BodyFont;
        Name = "OfficerForm";
        Size = new Size(1106, 615);
        ((System.ComponentModel.ISupportInitialize)grdOfficers).EndInit();
        pnlToolbar.ResumeLayout(false);
        pnlToolbar.PerformLayout();
        crdList.ResumeLayout(false);
        ResumeLayout(false);
    }
}
