using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Custody;

/// <summary>
/// The deposit receipt, hosted in the shell's content area: the voucher-entry screen of key-03 A. It is cached, so a
/// half-filled receipt survives switching screens; it holds no scope, each operation of the presenter opens its own.
/// </summary>
public partial class DepositReceiptForm : UserControl, IDepositReceiptView
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

    private static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");

    private readonly FieldErrorDisplay<DepositReceiptField> _fieldErrors = new();
    private bool _isPosted;

    public DepositReceiptForm()
    {
        InitializeComponent();
        _fieldErrors.Add(DepositReceiptField.Inmate, frmInmate, errInmate);
        _fieldErrors.Add(DepositReceiptField.TransactionType, frmTransactionType, errTransactionType);
        _fieldErrors.Add(DepositReceiptField.SenderFullName, frmSenderFullName, errSenderFullName);
        _fieldErrors.Add(DepositReceiptField.Relationship, frmRelationship, errRelationship);
        _fieldErrors.Add(DepositReceiptField.PaymentMethod, frmPaymentMethod, errPaymentMethod);
        _fieldErrors.Add(DepositReceiptField.AccountNumber, frmAccountNumber, errAccountNumber);
        _fieldErrors.Add(DepositReceiptField.VoucherDate, frmVoucherDate, errVoucherDate);
        _fieldErrors.Add(DepositReceiptField.Description, frmDescription, errDescription);
        _fieldErrors.Add(DepositReceiptField.Amount, frmAmount, errAmount);

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
        ShowTotal();
        ActiveControl = cmbInmate;
    }

    public event EventHandler? LoadRequested;

    public event EventHandler? AmountChanged;

    public event EventHandler? PostClicked;

    public event EventHandler? PrintClicked;

    public event EventHandler? ResetClicked;

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

            return decimal.TryParse(text, NumberStyles.AllowThousands, Vietnamese, out var amount)
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
        // The post is over; allow another attempt, unless this screen already posted a receipt.
        if (!_isPosted)
            btnPost.Enabled = true;

        MessageBox.Show(FindForm(), message, "Không ghi sổ được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void ShowFieldErrors(IReadOnlyList<FieldMessage<DepositReceiptField>> errors)
    {
        if (!_isPosted)
            btnPost.Enabled = true;

        _fieldErrors.Show(errors);
    }

    public void ShowPosted(string voucherNumber, decimal balanceAfter)
    {
        _isPosted = true;
        lblStatus.Text = $"Đã ghi sổ {voucherNumber} · Số dư mới: {balanceAfter.ToString("N0", Vietnamese)} đồng";
        lblStatus.ForeColor = balanceAfter > 0 ? AppTheme.Success : AppTheme.Text;
        btnPost.Enabled = false;
        btnIn.Enabled = true;
        btnIn.Focus();
    }

    public void ShowPrintPreview(byte[] pdf, string fileName)
    {
        using var preview = new PdfPreviewForm(pdf, fileName);
        preview.ShowDialog(FindForm());
    }

    public void Reset()
    {
        _isPosted = false;
        _fieldErrors.ClearAll();
        cmbTransactionType.SelectedIndex = 0;
        txtSenderFullName.Clear();
        txtRelationship.Clear();
        cmbPaymentMethod.SelectedIndex = 0;
        txtAccountNumber.Clear();
        txtDescription.Clear();
        txtAmount.Clear();
        lblStatus.Text = "";
        btnPost.Enabled = true;
        btnIn.Enabled = false;
        cmbInmate.Focus();
    }

    // A cached screen loads once, the first time the shell shows it.
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        LoadRequested?.Invoke(this, EventArgs.Empty);
    }

    // Voucher keys (EXPERIENCE.md): Esc starts the next receipt. Enter never posts: this screen has no AcceptButton.
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // An open drop-down keeps Esc to close itself.
        var isDroppedDown = cmbInmate.DroppedDown || cmbTransactionType.DroppedDown || cmbPaymentMethod.DroppedDown;
        if (keyData == Keys.Escape && !isDroppedDown)
        {
            ResetClicked?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void OnAmountChanged(object? sender, EventArgs e)
    {
        ShowTotal();
        AmountChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ShowTotal() => lblTotal.Text = $"{(Amount ?? 0).ToString("#,##0", Vietnamese)} đ";

    private void OnPostClicked(object? sender, EventArgs e)
    {
        // A second click while the post runs would start a second transaction and could post twice.
        btnPost.Enabled = false;
        _fieldErrors.ClearAll();
        PostClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnPrintClicked(object? sender, EventArgs e) => PrintClicked?.Invoke(this, EventArgs.Empty);

    private void OnResetClicked(object? sender, EventArgs e) => ResetClicked?.Invoke(this, EventArgs.Empty);

    private void OnPaymentMethodChanged(object? sender, EventArgs e) => UpdateAccountNumberState();

    private void UpdateAccountNumberState()
    {
        var isBankTransfer = cmbPaymentMethod.SelectedItem is PaymentMethodItem { Value: PaymentMethod.BankTransfer };
        frmAccountNumber.ReadOnly = !isBankTransfer;
        txtAccountNumber.TabStop = isBankTransfer;
        if (!isBankTransfer)
            _fieldErrors.Clear(DepositReceiptField.AccountNumber);
    }
}
