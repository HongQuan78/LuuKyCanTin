using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Custody;

public interface IDepositReceiptView
{
    event EventHandler? LoadRequested;

    event EventHandler? AmountChanged;

    event EventHandler? PostClicked;

    event EventHandler? PrintClicked;

    /// <summary>"Làm mới" (or Esc): start the next receipt.</summary>
    event EventHandler? ResetClicked;

    IReadOnlyList<InmateOption> Inmates { set; }

    int? InmateId { get; }

    TransactionType TransactionType { get; }

    string? SenderFullName { get; }

    string? Relationship { get; }

    PaymentMethod PaymentMethod { get; }

    string? SenderAccountNumber { get; }

    DateOnly VoucherDate { get; }

    string? Description { get; }

    decimal? Amount { get; }

    /// <summary>Live preview of the amount in words.</summary>
    string AmountInWords { set; }

    /// <summary>A failure that is not about one field (loading, the server's re-check, printing): a MessageBox.</summary>
    void ShowError(string message);

    /// <summary>Marks each field invalid with its message under it and focuses the first.</summary>
    void ShowFieldErrors(IReadOnlyList<FieldMessage<DepositReceiptField>> errors);

    /// <summary>Posting succeeded: show the number, the new balance and enable Print.</summary>
    void ShowPosted(string voucherNumber, decimal balanceAfter);

    void ShowPrintPreview(byte[] pdf, string fileName);

    /// <summary>Clears every field, error and posted state for the next receipt and focuses Đối tượng; the date stays.</summary>
    void Reset();
}
