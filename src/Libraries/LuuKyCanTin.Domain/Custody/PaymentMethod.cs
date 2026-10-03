namespace LuuKyCanTin.Domain.Custody;

/// <summary>How the money is handed over. A transfer requires the sender's account number.</summary>
public enum PaymentMethod : byte
{
    Cash = 1,
    BankTransfer = 2,
}
