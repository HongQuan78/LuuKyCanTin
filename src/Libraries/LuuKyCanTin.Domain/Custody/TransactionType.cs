namespace LuuKyCanTin.Domain.Custody;

/// <summary>
/// Why money moved. The tens digit is the <see cref="VoucherType"/> (1 receipt, 2 payout), which the database
/// enforces with a CHECK constraint.
/// </summary>
public enum TransactionType : byte
{
    BroughtOnAdmission = 11,
    SentByRelative = 12,
    GiftSlip = 13,
    ReceivedFromOtherInmate = 14,
    CanteenPurchase = 21,
    GivenToOtherInmate = 22,
    ReturnedToRelative = 23,
    FacilityTransfer = 24,
    SentenceCompleted = 25,
}
