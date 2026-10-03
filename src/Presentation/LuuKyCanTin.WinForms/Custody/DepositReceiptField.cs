namespace LuuKyCanTin.WinForms.Custody;

/// <summary>The validated fields of the deposit-receipt screen, in visual order, so an error lands under the field it concerns.</summary>
public enum DepositReceiptField : byte
{
    Inmate = 1,
    TransactionType = 2,
    SenderFullName = 3,
    Relationship = 4,
    PaymentMethod = 5,
    AccountNumber = 6,
    VoucherDate = 7,
    Description = 8,
    Amount = 9,
}
