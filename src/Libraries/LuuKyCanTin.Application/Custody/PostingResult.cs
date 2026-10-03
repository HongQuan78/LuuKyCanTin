namespace LuuKyCanTin.Application.Custody;

public sealed record PostingResult(bool Succeeded, long Id, string VoucherNumber, decimal BalanceAfter, string? Message)
{
    public static PostingResult Ok(long id, string voucherNumber, decimal balanceAfter) => new(true, id, voucherNumber, balanceAfter, null);

    public static PostingResult Fail(string message) => new(false, 0, "", 0, message);
}
