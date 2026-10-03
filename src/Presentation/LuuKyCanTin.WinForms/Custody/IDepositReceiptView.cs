using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Custody;

namespace LuuKyCanTin.WinForms.Custody;

public interface IDepositReceiptView
{
    event EventHandler? LoadRequested;

    event EventHandler? AmountChanged;

    event EventHandler? PostClicked;

    event EventHandler? PrintClicked;

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

    void ShowError(string message);

    /// <summary>Posting succeeded: show the number, the new balance and enable Print.</summary>
    void ShowPosted(string voucherNumber, decimal balanceAfter);

    void ShowPrintPreview(byte[] pdf, string fileName);
}
