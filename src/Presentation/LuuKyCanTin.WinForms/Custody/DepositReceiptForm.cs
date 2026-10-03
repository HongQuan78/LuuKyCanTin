using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Custody;

namespace LuuKyCanTin.WinForms.Custody;

public partial class DepositReceiptForm : Form, IDepositReceiptView
{
    // The spec numbers transaction types, and the form shows that number in front of the name.
    private sealed record TransactionTypeItem(TransactionType Value)
    {
        public override string ToString() => $"{(byte)Value} – {Value.ToDisplayText()}";
    }

    private sealed record PaymentMethodItem(PaymentMethod Value)
    {
        public override string ToString() => Value.ToDisplayText();
    }

    private bool _isPosted;

    public DepositReceiptForm()
    {
        InitializeComponent();

        // Nghiệp vụ 13 (phiếu gửi qua) stays out until DEC-05 is decided (alignment A4).
        cmbTransactionType.Items.AddRange(
        [
            new TransactionTypeItem(TransactionType.SentByRelative),
            new TransactionTypeItem(TransactionType.BroughtOnAdmission),
            new TransactionTypeItem(TransactionType.ReceivedFromOtherInmate),
        ]);
        cmbTransactionType.SelectedIndex = 0;

        cmbPaymentMethod.Items.AddRange(
        [
            new PaymentMethodItem(PaymentMethod.Cash),
            new PaymentMethodItem(PaymentMethod.BankTransfer),
        ]);
        cmbPaymentMethod.SelectedIndex = 0;
        UpdateAccountNumberState();
        Load += OnFormLoad;
    }

    public event EventHandler? LoadRequested;

    public event EventHandler? AmountChanged;

    public event EventHandler? PostClicked;

    public event EventHandler? PrintClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<InmateOption> Inmates
    {
        set
        {
            cmbInmate.DataSource = value.ToList();
            cmbInmate.DisplayMember = nameof(InmateOption.DisplayText);
            cmbInmate.ValueMember = nameof(InmateOption.Id);
            cmbInmate.SelectedIndex = value.Count > 0 ? 0 : -1;
        }
    }

    public int? InmateId => cmbInmate.SelectedItem is InmateOption option ? option.Id : null;

    public TransactionType TransactionType => ((TransactionTypeItem)cmbTransactionType.SelectedItem!).Value;

    public string? SenderFullName => txtSenderFullName.Text;

    public string? Relationship => txtRelationship.Text;

    public PaymentMethod PaymentMethod => ((PaymentMethodItem)cmbPaymentMethod.SelectedItem!).Value;

    public string? SenderAccountNumber => txtAccountNumber.Text;

    public DateOnly VoucherDate => DateOnly.FromDateTime(dtpVoucherDate.Value);

    public string? Description => txtDescription.Text;

    public decimal? Amount
    {
        get
        {
            var text = txtAmount.Text.Trim();

            // vi-VN: '.' groups thousands, ',' is the decimal separator. Money is whole đồng, so only a plain
            // integer or well-formed 3-digit groups parse; "12.5" and "1.234,5" return null for the validator.
            if (!Regex.IsMatch(text, @"^\d{1,3}(\.\d{3})*$|^\d+$"))
                return null;

            return decimal.TryParse(text, NumberStyles.AllowThousands, CultureInfo.GetCultureInfo("vi-VN"), out var amount)
                ? amount
                : null;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string AmountInWords
    {
        set => lblAmountInWords.Text = value.Length == 0 ? "" : $"Viết bằng chữ: {value}";
    }

    public void ShowError(string message)
    {
        // The post is over; allow another attempt, unless this form already posted a receipt.
        if (!_isPosted)
            btnPost.Enabled = true;

        MessageBox.Show(this, message, "Không ghi sổ được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void ShowPosted(string voucherNumber, decimal balanceAfter)
    {
        _isPosted = true;
        lblStatus.Text = $"Đã ghi sổ {voucherNumber} · Số dư mới: {balanceAfter.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đồng";
        btnPost.Enabled = false;
        btnIn.Enabled = true;
    }

    public void ShowPrintPreview(byte[] pdf, string fileName)
    {
        using var preview = new Common.PdfPreviewForm(pdf, fileName);
        preview.ShowDialog(this);
    }

    private void OnFormLoad(object? sender, EventArgs e) => LoadRequested?.Invoke(this, EventArgs.Empty);

    private void OnAmountChanged(object? sender, EventArgs e) => AmountChanged?.Invoke(this, EventArgs.Empty);

    private void OnPostClicked(object? sender, EventArgs e)
    {
        // A second click while the post runs would start a second transaction and could post twice.
        btnPost.Enabled = false;
        PostClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnPrintClicked(object? sender, EventArgs e) => PrintClicked?.Invoke(this, EventArgs.Empty);

    private void OnPaymentMethodChanged(object? sender, EventArgs e) => UpdateAccountNumberState();

    private void UpdateAccountNumberState()
    {
        var isBankTransfer = cmbPaymentMethod.SelectedItem is PaymentMethodItem { Value: PaymentMethod.BankTransfer };
        txtAccountNumber.Enabled = isBankTransfer;
        lblAccountNumber.Enabled = isBankTransfer;
    }
}
