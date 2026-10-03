namespace LuuKyCanTin.Application.Custody;

/// <summary>
/// The only way a custodial balance changes. The implementation raises the balance with a conditional
/// UPDATE under a row lock and returns the before/after values from the same statement.
/// </summary>
public interface ICustodyBalanceWriter
{
    /// <summary>
    /// Adds <paramref name="amount"/> to the balance of a managed detainee.
    /// </summary>
    /// <returns>The balance before and after, or null when the detainee does not exist or is not managed.</returns>
    Task<(decimal BalanceBefore, decimal BalanceAfter)?> IncreaseAsync(int inmateId, decimal amount, CancellationToken ct = default);
}
