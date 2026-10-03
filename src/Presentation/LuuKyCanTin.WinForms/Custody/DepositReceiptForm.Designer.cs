using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Custody;

partial class DepositReceiptForm
{
    private const int DescriptionHeight = 64;
    private const int RowGap = 14;

    private System.ComponentModel.IContainer components = null!;
    private Panel pnlMain = null!;
    private CardPanel crdInmate = null!;
    private TableLayoutPanel tblInmate = null!;
    private Panel pnlCardGap = null!;
    private CardPanel crdSender = null!;
    private TableLayoutPanel tblSender = null!;
    private Panel pnlColumnGap = null!;
    private CardPanel crdAmount = null!;
    private TableLayoutPanel tblAmount = null!;
    private Label lblInmate = null!;
    private InputFrame frmInmate = null!;
    private ComboBox cmbInmate = null!;
    private FieldError errInmate = null!;
    private Label lblTransactionType = null!;
    private InputFrame frmTransactionType = null!;
    private ComboBox cmbTransactionType = null!;
    private FieldError errTransactionType = null!;
    private Label lblSender = null!;
    private InputFrame frmSenderFullName = null!;
    private TextBox txtSenderFullName = null!;
    private FieldError errSenderFullName = null!;
    private Label lblRelationship = null!;
    private InputFrame frmRelationship = null!;
    private TextBox txtRelationship = null!;
    private FieldError errRelationship = null!;
    private Label lblPaymentMethod = null!;
    private InputFrame frmPaymentMethod = null!;
    private ComboBox cmbPaymentMethod = null!;
    private FieldError errPaymentMethod = null!;
    private Label lblAccountNumber = null!;
    private InputFrame frmAccountNumber = null!;
    private TextBox txtAccountNumber = null!;
    private FieldError errAccountNumber = null!;
    private Label lblVoucherDate = null!;
    private InputFrame frmVoucherDate = null!;
    private DateTimePicker dtpVoucherDate = null!;
    private FieldError errVoucherDate = null!;
    private Label lblDescription = null!;
    private InputFrame frmDescription = null!;
    private TextBox txtDescription = null!;
    private FieldError errDescription = null!;
    private Label lblAmount = null!;
    private InputFrame frmAmount = null!;
    private TextBox txtAmount = null!;
    private FieldError errAmount = null!;
    private TableLayoutPanel pnlTotal = null!;
    private Label lblTotalCaption = null!;
    private Label lblTotal = null!;
    private Label lblAmountInWords = null!;
    private Label lblStatus = null!;
    private Button btnPost = null!;
    private TableLayoutPanel pnlSecondary = null!;
    private Button btnIn = null!;
    private Button btnReset = null!;

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
        crdInmate = new CardPanel();
        tblInmate = new TableLayoutPanel();
        pnlCardGap = new Panel();
        crdSender = new CardPanel();
        tblSender = new TableLayoutPanel();
        pnlColumnGap = new Panel();
        crdAmount = new CardPanel();
        tblAmount = new TableLayoutPanel();
        lblInmate = new Label();
        frmInmate = new InputFrame();
        cmbInmate = new ComboBox();
        errInmate = new FieldError();
        lblTransactionType = new Label();
        frmTransactionType = new InputFrame();
        cmbTransactionType = new ComboBox();
        errTransactionType = new FieldError();
        lblSender = new Label();
        frmSenderFullName = new InputFrame();
        txtSenderFullName = new TextBox();
        errSenderFullName = new FieldError();
        lblRelationship = new Label();
        frmRelationship = new InputFrame();
        txtRelationship = new TextBox();
        errRelationship = new FieldError();
        lblPaymentMethod = new Label();
        frmPaymentMethod = new InputFrame();
        cmbPaymentMethod = new ComboBox();
        errPaymentMethod = new FieldError();
        lblAccountNumber = new Label();
        frmAccountNumber = new InputFrame();
        txtAccountNumber = new TextBox();
        errAccountNumber = new FieldError();
        lblVoucherDate = new Label();
        frmVoucherDate = new InputFrame();
        dtpVoucherDate = new DateTimePicker();
        errVoucherDate = new FieldError();
        lblDescription = new Label();
        frmDescription = new InputFrame();
        txtDescription = new TextBox();
        errDescription = new FieldError();
        lblAmount = new Label();
        frmAmount = new InputFrame();
        txtAmount = new TextBox();
        errAmount = new FieldError();
        pnlTotal = new TableLayoutPanel();
        lblTotalCaption = new Label();
        lblTotal = new Label();
        lblAmountInWords = new Label();
        lblStatus = new Label();
        btnPost = new Button();
        pnlSecondary = new TableLayoutPanel();
        btnIn = new Button();
        btnReset = new Button();
        SuspendLayout();

        // ---- Left column: "Đối tượng" on top, "Thông tin nộp tiền" filling the rest -------------------------------
        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Name = "pnlMain";
        pnlMain.TabIndex = 0;
        pnlMain.Controls.Add(crdSender);
        pnlMain.Controls.Add(pnlCardGap);
        pnlMain.Controls.Add(crdInmate);

        crdInmate.AutoSize = true;
        crdInmate.Dock = DockStyle.Top;
        crdInmate.HeaderText = "Đối tượng";
        crdInmate.Name = "crdInmate";
        crdInmate.TabIndex = 0;
        crdInmate.Controls.Add(tblInmate);
        SetUpFieldTable(tblInmate, "tblInmate");

        cmbInmate.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbInmate.Name = "cmbInmate";
        frmInmate.Name = "frmInmate";
        AddField(tblInmate, lblInmate, "Đối &tượng *", frmInmate, cmbInmate, errInmate, 0, 0);

        cmbTransactionType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTransactionType.Name = "cmbTransactionType";
        frmTransactionType.Name = "frmTransactionType";
        AddField(tblInmate, lblTransactionType, "Nghiệ&p vụ *", frmTransactionType, cmbTransactionType, errTransactionType, 1, 0);

        pnlCardGap.Dock = DockStyle.Top;
        pnlCardGap.Height = AppTheme.Gap;
        pnlCardGap.Name = "pnlCardGap";
        pnlCardGap.TabStop = false;

        crdSender.Dock = DockStyle.Fill;
        crdSender.HeaderText = "Thông tin nộp tiền";
        crdSender.Name = "crdSender";
        crdSender.TabIndex = 1;
        crdSender.Controls.Add(tblSender);
        SetUpFieldTable(tblSender, "tblSender");

        txtSenderFullName.Name = "txtSenderFullName";
        frmSenderFullName.Name = "frmSenderFullName";
        AddField(tblSender, lblSender, "&Người gửi *", frmSenderFullName, txtSenderFullName, errSenderFullName, 0, 0);

        txtRelationship.Name = "txtRelationship";
        frmRelationship.Name = "frmRelationship";
        AddField(tblSender, lblRelationship, "&Quan hệ", frmRelationship, txtRelationship, errRelationship, 1, 0);

        cmbPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPaymentMethod.Name = "cmbPaymentMethod";
        cmbPaymentMethod.SelectedIndexChanged += OnPaymentMethodChanged;
        frmPaymentMethod.Name = "frmPaymentMethod";
        AddField(tblSender, lblPaymentMethod, "&Hình thức *", frmPaymentMethod, cmbPaymentMethod, errPaymentMethod, 0, 1);

        txtAccountNumber.Name = "txtAccountNumber";
        frmAccountNumber.Name = "frmAccountNumber";
        AddField(tblSender, lblAccountNumber, "Số tài &khoản", frmAccountNumber, txtAccountNumber, errAccountNumber, 1, 1);

        dtpVoucherDate.CustomFormat = "dd/MM/yyyy";
        dtpVoucherDate.Format = DateTimePickerFormat.Custom;
        dtpVoucherDate.Name = "dtpVoucherDate";
        frmVoucherDate.Name = "frmVoucherDate";
        AddField(tblSender, lblVoucherDate, "Ngày &chứng từ *", frmVoucherDate, dtpVoucherDate, errVoucherDate, 0, 2);

        txtDescription.Multiline = true;
        txtDescription.Height = DescriptionHeight - 8;
        txtDescription.Name = "txtDescription";
        frmDescription.Height = DescriptionHeight;
        frmDescription.Name = "frmDescription";
        AddField(tblSender, lblDescription, "Nội &dung", frmDescription, txtDescription, errDescription, 0, 3, isWide: true);

        pnlColumnGap.Dock = DockStyle.Right;
        pnlColumnGap.Name = "pnlColumnGap";
        pnlColumnGap.TabStop = false;
        pnlColumnGap.Width = AppTheme.Gap;

        // ---- Right column: "Số tiền", 340px, full height -----------------------------------------------------------
        crdAmount.Dock = DockStyle.Right;
        crdAmount.HeaderText = "Số tiền";
        crdAmount.Name = "crdAmount";
        crdAmount.TabIndex = 1;
        crdAmount.Width = AppTheme.VoucherSideWidth;
        crdAmount.Controls.Add(tblAmount);

        tblAmount.ColumnCount = 1;
        tblAmount.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tblAmount.Dock = DockStyle.Fill;
        tblAmount.Margin = Padding.Empty;
        tblAmount.Name = "tblAmount";
        tblAmount.RowCount = 8;
        for (var i = 0; i < 5; i++)
            tblAmount.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblAmount.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tblAmount.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tblAmount.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        StyleLabel(lblAmount, "&Số tiền (đồng) *", isFirst: true);
        txtAmount.Name = "txtAmount";
        txtAmount.TextAlign = HorizontalAlignment.Right;
        txtAmount.TextChanged += OnAmountChanged;
        frmAmount.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        frmAmount.Margin = Padding.Empty;
        frmAmount.Name = "frmAmount";
        frmAmount.Inner = txtAmount;
        errAmount.Name = "errAmount";
        tblAmount.Controls.Add(lblAmount, 0, 0);
        tblAmount.Controls.Add(frmAmount, 0, 1);
        tblAmount.Controls.Add(errAmount, 0, 2);
        lblAmount.TabIndex = 0;
        frmAmount.TabIndex = 1;

        // The total box: caps caption, the amount in amount-large, the amount in words under it.
        pnlTotal.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        pnlTotal.AutoSize = true;
        pnlTotal.BackColor = AppTheme.AccentSoft;
        pnlTotal.ColumnCount = 1;
        pnlTotal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        pnlTotal.Margin = new Padding(0, 12, 0, 12);
        pnlTotal.Name = "pnlTotal";
        pnlTotal.Padding = new Padding(16, 14, 16, 14);
        pnlTotal.TabStop = false;
        pnlTotal.Controls.Add(lblTotalCaption, 0, 0);
        pnlTotal.Controls.Add(lblTotal, 0, 1);
        pnlTotal.Controls.Add(lblAmountInWords, 0, 2);
        tblAmount.Controls.Add(pnlTotal, 0, 3);

        lblTotalCaption.AutoSize = true;
        lblTotalCaption.Font = AppTheme.LabelFont;
        lblTotalCaption.ForeColor = AppTheme.AccentHover;
        lblTotalCaption.Margin = Padding.Empty;
        lblTotalCaption.Name = "lblTotalCaption";
        lblTotalCaption.Text = "TỔNG TIỀN";
        lblTotalCaption.UseMnemonic = false;

        lblTotal.AutoSize = true;
        lblTotal.Font = AppTheme.AmountLargeFont;
        lblTotal.ForeColor = AppTheme.Text;
        lblTotal.Margin = new Padding(0, 2, 0, 0);
        lblTotal.Name = "lblTotal";
        lblTotal.UseMnemonic = false;

        lblAmountInWords.AutoSize = true;
        lblAmountInWords.Font = AppTheme.AmountInWordsFont;
        lblAmountInWords.ForeColor = AppTheme.Text2;
        lblAmountInWords.Margin = new Padding(0, 4, 0, 0);
        lblAmountInWords.MaximumSize = new Size(AppTheme.VoucherSideWidth - 2 * AppTheme.CardPadding - 32, 0);
        lblAmountInWords.Name = "lblAmountInWords";
        lblAmountInWords.UseMnemonic = false;

        lblStatus.AutoSize = true;
        lblStatus.Font = AppTheme.BodySemiboldFont;
        lblStatus.Margin = Padding.Empty;
        lblStatus.MaximumSize = new Size(AppTheme.VoucherSideWidth - 2 * AppTheme.CardPadding - 2, 0);
        lblStatus.Name = "lblStatus";
        lblStatus.UseMnemonic = false;
        tblAmount.Controls.Add(lblStatus, 0, 4);

        btnPost.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        btnPost.Height = AppTheme.ButtonHeightLarge;
        btnPost.Margin = Padding.Empty;
        btnPost.Name = "btnPost";
        btnPost.TabIndex = 2;
        btnPost.Text = "&Ghi sổ";
        btnPost.Click += OnPostClicked;
        AppTheme.StylePrimary(btnPost);
        AppTheme.SetGlyph(btnPost, Glyphs.Post);
        tblAmount.Controls.Add(btnPost, 0, 6);

        pnlSecondary.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        pnlSecondary.AutoSize = true;
        pnlSecondary.ColumnCount = 2;
        pnlSecondary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        pnlSecondary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        pnlSecondary.Margin = new Padding(0, AppTheme.GapSmall, 0, 0);
        pnlSecondary.Name = "pnlSecondary";
        pnlSecondary.TabIndex = 3;
        pnlSecondary.Controls.Add(btnIn, 0, 0);
        pnlSecondary.Controls.Add(btnReset, 1, 0);
        tblAmount.Controls.Add(pnlSecondary, 0, 7);

        btnIn.Dock = DockStyle.Fill;
        btnIn.Enabled = false;
        btnIn.Margin = new Padding(0, 0, AppTheme.GapSmall / 2, 0);
        btnIn.Name = "btnIn";
        btnIn.TabIndex = 0;
        btnIn.Text = "&In biên nhận";
        btnIn.Click += OnPrintClicked;
        AppTheme.StyleSecondary(btnIn);
        AppTheme.SetGlyph(btnIn, Glyphs.Print);

        btnReset.Dock = DockStyle.Fill;
        btnReset.Margin = new Padding(AppTheme.GapSmall / 2, 0, 0, 0);
        btnReset.Name = "btnReset";
        btnReset.TabIndex = 1;
        btnReset.Text = "Làm &mới";
        btnReset.Click += OnResetClicked;
        AppTheme.StyleSecondary(btnReset);
        AppTheme.SetGlyph(btnReset, Glyphs.Refresh);

        // Dock order is reverse add order: the amount card takes the right, the gap next to it, the main column the rest.
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Surface;
        Controls.Add(pnlMain);
        Controls.Add(pnlColumnGap);
        Controls.Add(crdAmount);
        Font = AppTheme.BodyFont;
        Name = "DepositReceiptForm";
        Size = new Size(1106, 615);
        ResumeLayout(false);
    }

    private static void SetUpFieldTable(TableLayoutPanel table, string name)
    {
        table.AutoSize = true;
        table.ColumnCount = 2;
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        table.Dock = DockStyle.Top;
        table.Margin = Padding.Empty;
        table.Name = name;
        table.TabIndex = 0;
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
    /// One field in a 2-column card: label, framed input and error line on three table rows, so the input follows the
    /// column's width. Columns are 16px apart; field rows are 14px apart.
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
