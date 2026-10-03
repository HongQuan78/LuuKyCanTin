using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

partial class FacilityInfoForm
{
    private const int PreviewHeight = 96;
    private const int RowGap = 14;

    private System.ComponentModel.IContainer components = null!;
    private Panel pnlMain = null!;
    private FlowLayoutPanel pnlFooter = null!;
    private Button btnSave = null!;
    private Button btnReset = null!;
    private CardPanel crdInfo = null!;
    private Banner bnrMessage = null!;
    private Panel pnlBannerGap = null!;
    private TableLayoutPanel tblBody = null!;
    private Label lblParentAgency = null!;
    private InputFrame frmParentAgency = null!;
    private TextBox txtParentAgency = null!;
    private FieldError errParentAgency = null!;
    private Label lblFacilityName = null!;
    private InputFrame frmFacilityName = null!;
    private TextBox txtFacilityName = null!;
    private FieldError errFacilityName = null!;
    private Label lblAddress = null!;
    private InputFrame frmAddress = null!;
    private TextBox txtAddress = null!;
    private FieldError errAddress = null!;
    private Label lblPreviewCaption = null!;
    private Panel pnlPreview = null!;
    private Label lblPreview = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        pnlMain = new Panel();
        pnlFooter = new FlowLayoutPanel();
        btnSave = new Button();
        btnReset = new Button();
        crdInfo = new CardPanel();
        bnrMessage = new Banner();
        pnlBannerGap = new Panel();
        tblBody = new TableLayoutPanel();
        lblParentAgency = new Label();
        frmParentAgency = new InputFrame();
        txtParentAgency = new TextBox();
        errParentAgency = new FieldError();
        lblFacilityName = new Label();
        frmFacilityName = new InputFrame();
        txtFacilityName = new TextBox();
        errFacilityName = new FieldError();
        lblAddress = new Label();
        frmAddress = new InputFrame();
        txtAddress = new TextBox();
        errAddress = new FieldError();
        lblPreviewCaption = new Label();
        pnlPreview = new Panel();
        lblPreview = new Label();
        pnlMain.SuspendLayout();
        pnlFooter.SuspendLayout();
        crdInfo.SuspendLayout();
        tblBody.SuspendLayout();
        pnlPreview.SuspendLayout();
        SuspendLayout();

        // ---- Footer: the one primary button on the right, the secondary next to it ---------------------------------
        pnlFooter.AutoSize = true;
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.FlowDirection = FlowDirection.RightToLeft;
        pnlFooter.Name = "pnlFooter";
        pnlFooter.Padding = new Padding(0, AppTheme.Gap, 0, 0);
        pnlFooter.TabIndex = 1;
        pnlFooter.WrapContents = false;
        pnlFooter.Controls.Add(btnSave);
        pnlFooter.Controls.Add(btnReset);

        btnSave.AutoSize = true;
        btnSave.Margin = Padding.Empty;
        btnSave.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnSave.Name = "btnSave";
        btnSave.TabIndex = 0;
        btnSave.Text = "&Lưu";
        AppTheme.StylePrimary(btnSave);
        AppTheme.SetGlyph(btnSave, Glyphs.Save);

        btnReset.AutoSize = true;
        btnReset.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        btnReset.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnReset.Name = "btnReset";
        btnReset.TabIndex = 1;
        btnReset.Text = "Làm &mới";
        AppTheme.StyleSecondary(btnReset);
        AppTheme.SetGlyph(btnReset, Glyphs.Refresh);

        // ---- The card: the banner, then the three fields and the print preview -------------------------------------
        crdInfo.Dock = DockStyle.Fill;
        crdInfo.HeaderText = "Thông tin đơn vị";
        crdInfo.Name = "crdInfo";
        crdInfo.TabIndex = 0;
        // Dock order is reverse add order: the banner and gap sit at the top, the field table under them.
        crdInfo.Controls.Add(tblBody);
        crdInfo.Controls.Add(pnlBannerGap);
        crdInfo.Controls.Add(bnrMessage);

        bnrMessage.Dock = DockStyle.Top;
        bnrMessage.Name = "bnrMessage";
        bnrMessage.VisibleChanged += (_, _) => pnlBannerGap.Visible = bnrMessage.Visible;

        pnlBannerGap.Dock = DockStyle.Top;
        pnlBannerGap.Height = 12;
        pnlBannerGap.Name = "pnlBannerGap";
        pnlBannerGap.TabStop = false;
        pnlBannerGap.Visible = false;

        tblBody.AutoSize = true;
        tblBody.ColumnCount = 2;
        tblBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tblBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tblBody.Dock = DockStyle.Top;
        tblBody.Margin = Padding.Empty;
        tblBody.Name = "tblBody";
        tblBody.TabIndex = 0;

        txtParentAgency.Name = "txtParentAgency";
        frmParentAgency.Name = "frmParentAgency";
        AddField(tblBody, lblParentAgency, "&Cơ quan chủ quản", frmParentAgency, txtParentAgency, errParentAgency, 0, 0, isWide: true);

        txtFacilityName.Name = "txtFacilityName";
        frmFacilityName.Name = "frmFacilityName";
        AddField(tblBody, lblFacilityName, "Tên đơ&n vị *", frmFacilityName, txtFacilityName, errFacilityName, 0, 1);

        txtAddress.Name = "txtAddress";
        frmAddress.Name = "frmAddress";
        AddField(tblBody, lblAddress, "Địa c&hỉ *", frmAddress, txtAddress, errAddress, 1, 1);

        // Two field rows already fill grid rows 0-5 (three rows per field); the preview follows at rows 6 and 7.
        StyleLabel(lblPreviewCaption, "Xem trước tiêu đề in", isFirst: false);
        lblPreviewCaption.Name = "lblPreviewCaption";
        lblPreviewCaption.TabIndex = 8;
        tblBody.RowCount = Math.Max(tblBody.RowCount, 8);
        tblBody.Controls.Add(lblPreviewCaption, 0, 6);
        tblBody.SetColumnSpan(lblPreviewCaption, 2);

        pnlPreview.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        pnlPreview.BackColor = AppTheme.Subtle;
        pnlPreview.Height = PreviewHeight;
        pnlPreview.Margin = Padding.Empty;
        pnlPreview.Name = "pnlPreview";
        pnlPreview.Padding = new Padding(12);
        pnlPreview.TabIndex = 9;
        pnlPreview.Controls.Add(lblPreview);
        tblBody.Controls.Add(pnlPreview, 0, 7);
        tblBody.SetColumnSpan(pnlPreview, 2);

        lblPreview.AutoSize = false;
        lblPreview.Dock = DockStyle.Fill;
        lblPreview.ForeColor = AppTheme.Text2;
        lblPreview.Name = "lblPreview";
        lblPreview.TabStop = false;
        lblPreview.UseMnemonic = false;

        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Name = "pnlMain";
        pnlMain.TabIndex = 0;
        pnlMain.Controls.Add(crdInfo);
        pnlMain.Controls.Add(pnlFooter);

        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Surface;
        Controls.Add(pnlMain);
        Font = AppTheme.BodyFont;
        Name = "FacilityInfoForm";
        Size = new Size(1106, 615);
        pnlMain.ResumeLayout(false);
        pnlFooter.ResumeLayout(false);
        pnlFooter.PerformLayout();
        crdInfo.ResumeLayout(false);
        tblBody.ResumeLayout(false);
        tblBody.PerformLayout();
        pnlPreview.ResumeLayout(false);
        ResumeLayout(false);
    }

    private static void StyleLabel(Label label, string caption, bool isFirst)
    {
        label.AutoSize = true;
        label.Font = AppTheme.LabelFont;
        label.ForeColor = AppTheme.Label;
        label.Margin = new Padding(0, isFirst ? 0 : RowGap, 0, AppTheme.LabelGap);
        label.Text = caption;
    }

    /// <summary>
    /// One field in a 2-column card: label, framed input and error line on three table rows, so the input follows
    /// the column's width. Columns are 16px apart; field rows are 14px apart.
    /// </summary>
    private static void AddField(TableLayoutPanel table, Label label, string caption, InputFrame frame, Control inner, FieldError error,
        int column, int row, bool isWide = false)
    {
        StyleLabel(label, caption, isFirst: row == 0);
        var gap = isWide ? Padding.Empty : column == 0 ? new Padding(0, 0, AppTheme.Gap / 2, 0) : new Padding(AppTheme.Gap / 2, 0, 0, 0);
        label.Margin = new Padding(gap.Left, label.Margin.Top, 0, label.Margin.Bottom);

        frame.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        frame.Margin = gap;
        frame.Inner = inner;
        error.Margin = new Padding(gap.Left, 4, gap.Right, 0);

        var tabIndex = (row * 2 + column) * 2;
        label.TabIndex = tabIndex;
        frame.TabIndex = tabIndex + 1;

        table.RowCount = Math.Max(table.RowCount, row * 3 + 3);
        table.Controls.Add(label, column, row * 3);
        table.Controls.Add(frame, column, row * 3 + 1);
        table.Controls.Add(error, column, row * 3 + 2);
        if (isWide)
        {
            table.SetColumnSpan(label, 2);
            table.SetColumnSpan(frame, 2);
            table.SetColumnSpan(error, 2);
        }
    }
}
