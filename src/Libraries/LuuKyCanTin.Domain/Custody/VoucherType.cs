namespace LuuKyCanTin.Domain.Custody;

/// <summary>Receipt or payout. Also the tens digit of <see cref="TransactionType"/>.</summary>
public enum VoucherType : byte
{
    Receipt = 1,
    Payout = 2,
}
