using LuuKyCanTin.Domain.Custody;

namespace LuuKyCanTin.Application.Custody;

/// <summary>The inputs of one cash receipt posting; the service fills number, words and balances itself.</summary>
public sealed record PostDepositReceiptRequest
{
    public int InmateId { get; init; }

    public DateOnly VoucherDate { get; init; }

    public TransactionType TransactionType { get; init; } = TransactionType.SentByRelative;

    public PaymentMethod PaymentMethod { get; init; } = PaymentMethod.Cash;

    public string? SenderFullName { get; init; }

    public string? Relationship { get; init; }

    public string? SourceDocumentNumber { get; init; }

    public string? SenderAccountNumber { get; init; }

    public DateOnly? ReceivedDate { get; init; }

    public string? Description { get; init; }

    public decimal Amount { get; init; }
}
