using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Custody;

/// <summary>
/// Allocates a number with one atomic UPDATE. UPDLOCK+ROWLOCK holds the counter row until the posting's
/// transaction ends, and because the raise is inside that transaction a rollback returns the number, leaving no gap.
/// </summary>
internal sealed class VoucherCounterNumberingService(AppDbContext db) : INumberingService
{
    public async Task<string> AllocateNumberAsync(string voucherTypeCode, int year, CancellationToken ct = default)
    {
        // Materialize before composing LINQ: EF would otherwise wrap the UPDATE in a sub-select, which SQL rejects.
        var rows = await db.Database.SqlQuery<AllocatedNumberRow>($"""
            UPDATE VoucherCounter WITH (UPDLOCK, ROWLOCK)
            SET CurrentNumber = CurrentNumber + 1
            OUTPUT inserted.Prefix AS Prefix, inserted.CurrentNumber AS CurrentNumber
            WHERE VoucherTypeCode = {voucherTypeCode} AND Year = {year}
            """).ToListAsync(ct);

        if (rows.Count == 0)
            throw new InvalidOperationException($"Không có bộ đếm cho loại chứng từ {voucherTypeCode} năm {year}.");

        return $"{rows[0].Prefix}-{year}-{rows[0].CurrentNumber:D5}";
    }

    private sealed class AllocatedNumberRow
    {
        public string Prefix { get; set; } = "";

        public int CurrentNumber { get; set; }
    }
}
