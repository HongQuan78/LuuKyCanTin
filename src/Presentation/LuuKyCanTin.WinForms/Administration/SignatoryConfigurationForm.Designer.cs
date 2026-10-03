using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

partial class SignatoryConfigurationForm
{
    private const int TemplateListWidth = 260;
    private const int PreviewHeight = 96;

    private System.ComponentModel.IContainer components = null!;
    private CardPanel crdTemplates = null!;
    private DataGridView grdTemplates = null!;
    private DataGridViewTextBoxColumn colTemplateName = null!;
    private Panel pnlColumnGap = null!;
    private CardPanel crdSignatories = null!;
    private Banner bnrMessage = null!;
    private Panel pnlBannerGap = null!;
    private DataGridView grdLines = null!;
    private DataGridViewTextBoxColumn colOrdinal = null!;
    private DataGridViewTextBoxColumn colTitle = null!;
    private DataGridViewComboBoxColumn colOfficer = null!;
    private FlowLayoutPanel pnlTools = null!;
    private Button btnAdd = null!;
    private Button btnRemove = null!;
    private Button btnUp = null!;
    private Button btnDown = null!;
    private Panel pnlPreview = null!;
    private Label lblPreviewCaption = null!;
    private FlowLayoutPanel flpSignature = null!;
    private FlowLayoutPanel pnlFooter = null!;
    private Button btnSave = null!;
    private Button btnDiscard = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        crdTemplates = new CardPanel();
        grdTemplates = new DataGridView();
        colTemplateName = new DataGridViewTextBoxColumn();
        pnlColumnGap = new Panel();
        crdSignatories = new CardPanel();
        bnrMessage = new Banner();
        pnlBannerGap = new Panel();
        grdLines = new DataGridView();
        colOrdinal = new DataGridViewTextBoxColumn();
        colTitle = new DataGridViewTextBoxColumn();
        colOfficer = new DataGridViewComboBoxColumn();
        pnlTools = new FlowLayoutPanel();
        btnAdd = new Button();
        btnRemove = new Button();
        btnUp = new Button();
        btnDown = new Button();
        pnlPreview = new Panel();
        lblPreviewCaption = new Label();
        flpSignature = new FlowLayoutPanel();
        pnlFooter = new FlowLayoutPanel();
        btnSave = new Button();
        btnDiscard = new Button();
        ((System.ComponentModel.ISupportInitialize)grdTemplates).BeginInit();
        ((System.ComponentModel.ISupportInitialize)grdLines).BeginInit();
        crdTemplates.SuspendLayout();
        crdSignatories.SuspendLayout();
        pnlTools.SuspendLayout();
        pnlPreview.SuspendLayout();
        pnlFooter.SuspendLayout();
        SuspendLayout();

        // ---- Left: the template list, 260px, display names only ----------------------------------------------------
        crdTemplates.Dock = DockStyle.Left;
        crdTemplates.HeaderText = "Mẫu in";
        crdTemplates.Name = "crdTemplates";
        crdTemplates.Padding = new Padding(1, AppTheme.CardHeaderHeight, 1, 1);
        crdTemplates.TabIndex = 0;
        crdTemplates.Width = TemplateListWidth;
        crdTemplates.Controls.Add(grdTemplates);

        grdTemplates.AllowUserToAddRows = false;
        grdTemplates.AllowUserToDeleteRows = false;
        grdTemplates.AllowUserToResizeRows = false;
        grdTemplates.AutoGenerateColumns = false;
        grdTemplates.Columns.Add(colTemplateName);
        grdTemplates.Dock = DockStyle.Fill;
        grdTemplates.MultiSelect = false;
        grdTemplates.Name = "grdTemplates";
        grdTemplates.ReadOnly = true;
        grdTemplates.StandardTab = true;
        grdTemplates.TabIndex = 0;
        AppTheme.StyleGrid(grdTemplates);
        grdTemplates.ColumnHeadersVisible = false;

        colTemplateName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colTemplateName.Name = "colTemplateName";

        pnlColumnGap.Dock = DockStyle.Left;
        pnlColumnGap.Name = "pnlColumnGap";
        pnlColumnGap.TabStop = false;
        pnlColumnGap.Width = AppTheme.Gap;

        // ---- Right: the signer grid, tools, preview and footer ------------------------------------------------------
        crdSignatories.Dock = DockStyle.Fill;
        crdSignatories.HeaderText = "Người ký";
        crdSignatories.Name = "crdSignatories";
        crdSignatories.TabIndex = 1;
        // Dock order is reverse add order: the grid fills the middle; from the top come banner and gap, and from the
        // bottom footer, preview and tools, so the printed order (top to bottom) is grid, tools, preview, footer.
        crdSignatories.Controls.Add(grdLines);
        crdSignatories.Controls.Add(pnlTools);
        crdSignatories.Controls.Add(pnlPreview);
        crdSignatories.Controls.Add(pnlFooter);
        crdSignatories.Controls.Add(pnlBannerGap);
        crdSignatories.Controls.Add(bnrMessage);

        bnrMessage.Dock = DockStyle.Top;
        bnrMessage.Name = "bnrMessage";
        bnrMessage.VisibleChanged += (_, _) => pnlBannerGap.Visible = bnrMessage.Visible;

        pnlBannerGap.Dock = DockStyle.Top;
        pnlBannerGap.Height = 12;
        pnlBannerGap.Name = "pnlBannerGap";
        pnlBannerGap.TabStop = false;
        pnlBannerGap.Visible = false;

        grdLines.AllowUserToAddRows = false;
        grdLines.AllowUserToDeleteRows = false;
        grdLines.AllowUserToResizeRows = false;
        grdLines.AutoGenerateColumns = false;
        grdLines.Columns.AddRange(colOrdinal, colTitle, colOfficer);
        grdLines.Dock = DockStyle.Fill;
        grdLines.MultiSelect = false;
        grdLines.Name = "grdLines";
        grdLines.StandardTab = true;
        grdLines.TabIndex = 0;
        AppTheme.StyleGrid(grdLines);

        colOrdinal.HeaderText = "Thứ tự";
        colOrdinal.Name = "colOrdinal";
        colOrdinal.ReadOnly = true;
        colOrdinal.Width = 70;

        colTitle.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colTitle.HeaderText = "Chức danh";
        colTitle.Name = "colTitle";

        colOfficer.FlatStyle = FlatStyle.Flat;
        colOfficer.HeaderText = "Người ký mặc định";
        colOfficer.Name = "colOfficer";
        colOfficer.Width = 240;

        pnlTools.AutoSize = true;
        pnlTools.Dock = DockStyle.Bottom;
        pnlTools.FlowDirection = FlowDirection.LeftToRight;
        pnlTools.Name = "pnlTools";
        pnlTools.Padding = new Padding(0, AppTheme.Gap, 0, 0);
        pnlTools.TabIndex = 1;
        pnlTools.WrapContents = false;
        pnlTools.Controls.Add(btnAdd);
        pnlTools.Controls.Add(btnRemove);
        pnlTools.Controls.Add(btnUp);
        pnlTools.Controls.Add(btnDown);

        btnAdd.AutoSize = true;
        btnAdd.Margin = Padding.Empty;
        btnAdd.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnAdd.Name = "btnAdd";
        btnAdd.TabIndex = 0;
        btnAdd.Text = "Thêm &dòng";
        AppTheme.StyleSecondary(btnAdd);
        AppTheme.SetGlyph(btnAdd, Glyphs.Add);

        btnRemove.AutoSize = true;
        btnRemove.Margin = new Padding(AppTheme.GapSmall, 0, 0, 0);
        btnRemove.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnRemove.Name = "btnRemove";
        btnRemove.TabIndex = 1;
        btnRemove.Text = "&Xoá dòng";
        AppTheme.StyleSecondary(btnRemove);

        btnUp.AutoSize = true;
        btnUp.Margin = new Padding(AppTheme.GapSmall, 0, 0, 0);
        btnUp.MinimumSize = new Size(72, AppTheme.ButtonHeight);
        btnUp.Name = "btnUp";
        btnUp.TabIndex = 2;
        btnUp.Text = "Lê&n";
        AppTheme.StyleSecondary(btnUp);

        btnDown.AutoSize = true;
        btnDown.Margin = new Padding(AppTheme.GapSmall, 0, 0, 0);
        btnDown.MinimumSize = new Size(72, AppTheme.ButtonHeight);
        btnDown.Name = "btnDown";
        btnDown.TabIndex = 3;
        btnDown.Text = "X&uống";
        AppTheme.StyleSecondary(btnDown);

        pnlPreview.BackColor = AppTheme.Subtle;
        pnlPreview.Dock = DockStyle.Bottom;
        pnlPreview.Height = PreviewHeight;
        pnlPreview.Margin = Padding.Empty;
        pnlPreview.Name = "pnlPreview";
        pnlPreview.Padding = new Padding(12);
        pnlPreview.TabIndex = 2;
        // Added first, so it fills under the caption label that docks to the top.
        pnlPreview.Controls.Add(flpSignature);
        pnlPreview.Controls.Add(lblPreviewCaption);

        lblPreviewCaption.AutoSize = true;
        lblPreviewCaption.Dock = DockStyle.Top;
        lblPreviewCaption.Font = AppTheme.LabelFont;
        lblPreviewCaption.ForeColor = AppTheme.Label;
        lblPreviewCaption.Name = "lblPreviewCaption";
        lblPreviewCaption.Padding = new Padding(0, 0, 0, AppTheme.LabelGap);
        lblPreviewCaption.Text = "Xem trước khối chữ ký";
        lblPreviewCaption.UseMnemonic = false;

        flpSignature.AutoScroll = true;
        flpSignature.BackColor = AppTheme.Subtle;
        flpSignature.Dock = DockStyle.Fill;
        flpSignature.FlowDirection = FlowDirection.LeftToRight;
        flpSignature.Name = "flpSignature";
        flpSignature.TabIndex = 0;
        flpSignature.WrapContents = false;

        pnlFooter.AutoSize = true;
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.FlowDirection = FlowDirection.RightToLeft;
        pnlFooter.Name = "pnlFooter";
        pnlFooter.Padding = new Padding(0, AppTheme.Gap, 0, 0);
        pnlFooter.TabIndex = 3;
        pnlFooter.WrapContents = false;
        pnlFooter.Controls.Add(btnSave);
        pnlFooter.Controls.Add(btnDiscard);

        btnSave.AutoSize = true;
        btnSave.Margin = Padding.Empty;
        btnSave.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnSave.Name = "btnSave";
        btnSave.TabIndex = 0;
        btnSave.Text = "&Lưu";
        AppTheme.StylePrimary(btnSave);
        AppTheme.SetGlyph(btnSave, Glyphs.Save);

        btnDiscard.AutoSize = true;
        btnDiscard.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        btnDiscard.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnDiscard.Name = "btnDiscard";
        btnDiscard.TabIndex = 1;
        btnDiscard.Text = "&Huỷ thay đổi";
        AppTheme.StyleSecondary(btnDiscard);

        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Surface;
        Controls.Add(crdSignatories);
        Controls.Add(pnlColumnGap);
        Controls.Add(crdTemplates);
        Font = AppTheme.BodyFont;
        Name = "SignatoryConfigurationForm";
        Size = new Size(1106, 615);
        ((System.ComponentModel.ISupportInitialize)grdTemplates).EndInit();
        ((System.ComponentModel.ISupportInitialize)grdLines).EndInit();
        crdTemplates.ResumeLayout(false);
        crdSignatories.ResumeLayout(false);
        pnlTools.ResumeLayout(false);
        pnlTools.PerformLayout();
        pnlPreview.ResumeLayout(false);
        pnlPreview.PerformLayout();
        pnlFooter.ResumeLayout(false);
        pnlFooter.PerformLayout();
        ResumeLayout(false);
    }
}
