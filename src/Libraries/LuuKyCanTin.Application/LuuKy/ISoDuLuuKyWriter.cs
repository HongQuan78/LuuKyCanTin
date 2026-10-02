namespace LuuKyCanTin.Application.LuuKy;

/// <summary>
/// The only way a custodial balance changes. The implementation raises the balance with a conditional
/// UPDATE under a row lock and returns the before/after values from the same statement.
/// </summary>
public interface ISoDuLuuKyWriter
{
    /// <summary>
    /// Adds <paramref name="soTien"/> to the balance of a managed detainee.
    /// </summary>
    /// <returns>The balance before and after, or null when the detainee does not exist or is not managed.</returns>
    Task<(decimal SoDuTruoc, decimal SoDuSau)?> CongAsync(int doiTuongId, decimal soTien, CancellationToken ct = default);
}
