namespace LuuKyCanTin.WinForms.Custody;

partial class DepositReceiptForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblInmate = null!;
    private ComboBox cmbInmate = null!;
    private Label lblTransactionType = null!;
    private ComboBox cmbTransactionType = null!;
    private Label lblSender = null!;
    private TextBox txtSenderFullName = null!;
    private Label lblRelationship = null!;
    private TextBox txtRelationship = null!;
    private Label lblPaymentMethod = null!;
    private ComboBox cmbPaymentMethod = null!;
    private Label lblAccountNumber = null!;
    private TextBox txtAccountNumber = null!;
    private Label lblVoucherDate = null!;
    private DateTimePicker dtpVoucherDate = null!;
    private Label lblAmount = null!;
    private TextBox txtAmount = null!;
    private Label lblAmountInWords = null!;
    private Label lblDescription = null!;
    private TextBox txtDescription = null!;
    private Label lblStatus = null!;
    private Button btnPost = null!;
    private Button btnIn = null!;
    private Button btnClose = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblInmate = new Label();
        cmbInmate = new ComboBox();
        lblTransactionType = new Label();
        cmbTransactionType = new ComboBox();
        lblSender = new Label();
        txtSenderFullName = new TextBox();
        lblRelationship = new Label();
        txtRelationship = new TextBox();
        lblPaymentMethod = new Label();
        cmbPaymentMethod = new ComboBox();
        lblAccountNumber = new Label();
        txtAccountNumber = new TextBox();
        lblVoucherDate = new Label();
        dtpVoucherDate = new DateTimePicker();
        lblAmount = new Label();
        txtAmount = new TextBox();
        lblAmountInWords = new Label();
        lblDescription = new Label();
        txtDescription = new TextBox();
        lblStatus = new Label();
        btnPost = new Button();
        btnIn = new Button();
        btnClose = new Button();
        SuspendLayout();

        lblInmate.AutoSize = true;
        lblInmate.Location = new Point(20, 20);
        lblInmate.Text = "Đối tượng (*)";
        cmbInmate.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbInmate.Location = new Point(160, 17);
        cmbInmate.Size = new Size(380, 27);
        cmbInmate.Name = "cmbInmate";

        lblTransactionType.AutoSize = true;
        lblTransactionType.Location = new Point(20, 56);
        lblTransactionType.Text = "Nghiệp vụ (*)";
        cmbTransactionType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTransactionType.Location = new Point(160, 53);
        cmbTransactionType.Size = new Size(380, 27);
        cmbTransactionType.Name = "cmbTransactionType";

        lblSender.AutoSize = true;
        lblSender.Location = new Point(20, 92);
        lblSender.Text = "Người gửi (*)";
        txtSenderFullName.Location = new Point(160, 89);
        txtSenderFullName.Size = new Size(380, 27);
        txtSenderFullName.Name = "txtSenderFullName";

        lblRelationship.AutoSize = true;
        lblRelationship.Location = new Point(20, 128);
        lblRelationship.Text = "Quan hệ";
        txtRelationship.Location = new Point(160, 125);
        txtRelationship.Size = new Size(380, 27);
        txtRelationship.Name = "txtRelationship";

        lblPaymentMethod.AutoSize = true;
        lblPaymentMethod.Location = new Point(20, 164);
        lblPaymentMethod.Text = "Hình thức (*)";
        cmbPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPaymentMethod.Location = new Point(160, 161);
        cmbPaymentMethod.Size = new Size(170, 27);
        cmbPaymentMethod.Name = "cmbPaymentMethod";
        cmbPaymentMethod.SelectedIndexChanged += OnPaymentMethodChanged;

        lblAccountNumber.AutoSize = true;
        lblAccountNumber.Location = new Point(20, 200);
        lblAccountNumber.Text = "Số tài khoản";
        txtAccountNumber.Location = new Point(160, 197);
        txtAccountNumber.Size = new Size(380, 27);
        txtAccountNumber.Name = "txtAccountNumber";

        lblVoucherDate.AutoSize = true;
        lblVoucherDate.Location = new Point(20, 236);
        lblVoucherDate.Text = "Ngày chứng từ (*)";
        dtpVoucherDate.Format = DateTimePickerFormat.Short;
        dtpVoucherDate.Location = new Point(160, 233);
        dtpVoucherDate.Size = new Size(140, 27);
        dtpVoucherDate.Name = "dtpVoucherDate";

        lblAmount.AutoSize = true;
        lblAmount.Location = new Point(20, 272);
        lblAmount.Text = "Số tiền (đồng) (*)";
        txtAmount.Location = new Point(160, 269);
        txtAmount.Size = new Size(170, 27);
        txtAmount.Name = "txtAmount";
        txtAmount.TextChanged += OnAmountChanged;

        lblAmountInWords.AutoSize = true;
        lblAmountInWords.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
        lblAmountInWords.ForeColor = Color.DimGray;
        lblAmountInWords.Location = new Point(20, 302);
        lblAmountInWords.Size = new Size(520, 20);
        lblAmountInWords.Text = "";

        lblDescription.AutoSize = true;
        lblDescription.Location = new Point(20, 330);
        lblDescription.Text = "Nội dung";
        txtDescription.Location = new Point(160, 327);
        txtDescription.Multiline = true;
        txtDescription.Size = new Size(380, 60);
        txtDescription.Name = "txtDescription";

        lblStatus.Location = new Point(20, 400);
        lblStatus.Size = new Size(520, 24);
        lblStatus.ForeColor = Color.ForestGreen;
        lblStatus.Text = "";

        btnPost.Location = new Point(160, 432);
        btnPost.Size = new Size(140, 34);
        btnPost.Text = "Ghi sổ";
        btnPost.Click += OnPostClicked;

        btnIn.Enabled = false;
        btnIn.Location = new Point(310, 432);
        btnIn.Size = new Size(140, 34);
        btnIn.Text = "In biên nhận";
        btnIn.Click += OnPrintClicked;

        btnClose.DialogResult = DialogResult.Cancel;
        btnClose.Location = new Point(460, 432);
        btnClose.Size = new Size(120, 34);
        btnClose.Text = "Đóng";

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(600, 490);
        Controls.Add(lblInmate);
        Controls.Add(cmbInmate);
        Controls.Add(lblTransactionType);
        Controls.Add(cmbTransactionType);
        Controls.Add(lblSender);
        Controls.Add(txtSenderFullName);
        Controls.Add(lblRelationship);
        Controls.Add(txtRelationship);
        Controls.Add(lblPaymentMethod);
        Controls.Add(cmbPaymentMethod);
        Controls.Add(lblAccountNumber);
        Controls.Add(txtAccountNumber);
        Controls.Add(lblVoucherDate);
        Controls.Add(dtpVoucherDate);
        Controls.Add(lblAmount);
        Controls.Add(txtAmount);
        Controls.Add(lblAmountInWords);
        Controls.Add(lblDescription);
        Controls.Add(txtDescription);
        Controls.Add(lblStatus);
        Controls.Add(btnPost);
        Controls.Add(btnIn);
        Controls.Add(btnClose);
        CancelButton = btnClose;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Lập biên nhận thu";
        ResumeLayout(false);
        PerformLayout();
    }
}
