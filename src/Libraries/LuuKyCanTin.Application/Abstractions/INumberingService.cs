namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// Allocates document numbers atomically, per type and year. The implementation must raise the counter with a
/// conditional UPDATE under a row lock on the caller's connection and transaction, so a rollback returns the number.
/// </summary>
public interface INumberingService
{
    /// <param name="voucherTypeCode">The counter key, e.g. <c>BNT</c>.</param>
    /// <returns>The full number, e.g. <c>BNT-2026-00001</c>.</returns>
    Task<string> AllocateNumberAsync(string voucherTypeCode, int year, CancellationToken ct = default);
}
