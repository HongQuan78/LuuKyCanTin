using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.MasterData;

namespace LuuKyCanTin.Domain.Custody;

/// <summary>
/// One posted movement of custodial money. The detainee's name and type are snapshotted at posting, so a
/// reprint always matches the original even after the master record changes.
/// </summary>
public sealed class CustodyVoucher : AuditableEntity, IAuditable, ICancellable
{
    private CustodyVoucher()
    {
    }

    public long Id { get; private set; }

    public string VoucherNumber { get; private set; } = "";

    public DateOnly VoucherDate { get; private set; }

    public VoucherType VoucherType { get; private set; }

    public TransactionType TransactionType { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public VoucherStatus Status { get; private set; }

    public int InmateId { get; private set; }

    /// <summary>Snapshot of the detainee's name at posting time; never read back from the master.</summary>
    public string InmateFullName { get; private set; } = "";

    /// <summary>Snapshot of the detainee's type at posting time.</summary>
    public InmateType InmateType { get; private set; }

    public string? SenderFullName { get; private set; }

    public string? Relationship { get; private set; }

    /// <summary>The source paper (custodial deposit slip, gift slip, give-receive minutes).</summary>
    public string? SourceDocumentNumber { get; private set; }

    public string? SenderAccountNumber { get; private set; }

    public DateOnly? ReceivedDate { get; private set; }

    public string? Description { get; private set; }

    public decimal Amount { get; private set; }

    /// <summary>Stored at posting time so a reprint matches the original even if the converter changes.</summary>
    public string AmountInWords { get; private set; } = "";

    public decimal BalanceBefore { get; private set; }

    public decimal BalanceAfter { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public int? CancelledById { get; private set; }

    public short PrintCount { get; private set; }

    public bool IsCancelled => Status == VoucherStatus.Cancelled;

    /// <summary>
    /// The walking skeleton creates a receipt directly as posted. The full Draft → Posted → Cancelled state
    /// machine (NEN-14) arrives in Epic 4; the factory keeps the invariants the database CHECKs enforce.
    /// </summary>
    public static CustodyVoucher CreatePostedDepositReceipt(
        string voucherNumber,
        DateOnly voucherDate,
        Inmate inmate,
        VoucherType voucherType,
        TransactionType transactionType,
        PaymentMethod paymentMethod,
        string? senderFullName,
        string? relationship,
        string? sourceDocumentNumber,
        string? senderAccountNumber,
        DateOnly? receivedDate,
        string? description,
        decimal amount,
        string amountInWords,
        decimal balanceBefore,
        decimal balanceAfter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(voucherNumber);
        ArgumentNullException.ThrowIfNull(inmate);
        ArgumentException.ThrowIfNullOrWhiteSpace(amountInWords);

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Số tiền phải lớn hơn 0.");

        if (!Enum.IsDefined(transactionType) || (int)transactionType / 10 != (int)voucherType)
            throw new ArgumentException("Nghiệp vụ phải thuộc loại phiếu.", nameof(transactionType));

        if (paymentMethod == PaymentMethod.BankTransfer && string.IsNullOrWhiteSpace(senderAccountNumber))
            throw new ArgumentException("Chuyển khoản phải có số tài khoản người gửi.", nameof(senderAccountNumber));

        return new CustodyVoucher
        {
            VoucherNumber = voucherNumber,
            VoucherDate = voucherDate,
            VoucherType = voucherType,
            TransactionType = transactionType,
            PaymentMethod = paymentMethod,
            Status = VoucherStatus.Posted,
            InmateId = inmate.Id,
            InmateFullName = inmate.FullName,
            InmateType = inmate.InmateType,
            SenderFullName = senderFullName,
            Relationship = relationship,
            SourceDocumentNumber = sourceDocumentNumber,
            SenderAccountNumber = senderAccountNumber,
            ReceivedDate = receivedDate,
            Description = description,
            Amount = amount,
            AmountInWords = amountInWords,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
        };
    }
}
