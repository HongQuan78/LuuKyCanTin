using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Custody;

/// <summary>
/// The only writer of Inmate.CustodyBalance. One conditional UPDATE under UPDLOCK+ROWLOCK both locks the row and
/// returns the before/after balances, so no read-then-write race can lose a deposit. Inmate.CustodyBalance is
/// ignored on every EF save, so no other code path can change it.
/// </summary>
internal sealed class CustodyBalanceWriter(AppDbContext db) : ICustodyBalanceWriter
{
    public async Task<(decimal BalanceBefore, decimal BalanceAfter)?> IncreaseAsync(int inmateId, decimal amount, CancellationToken ct = default)
    {
        // Materialize before composing LINQ: EF would otherwise wrap the UPDATE in a sub-select, which SQL rejects.
        var rows = await db.Database.SqlQuery<BalanceRow>($"""
            UPDATE Inmate WITH (UPDLOCK, ROWLOCK)
            SET CustodyBalance = CustodyBalance + {amount}
            OUTPUT deleted.CustodyBalance AS BalanceBefore, inserted.CustodyBalance AS BalanceAfter
            WHERE Id = {inmateId} AND Status = 1
            """).ToListAsync(ct);

        return rows.Count == 0 ? null : (rows[0].BalanceBefore, rows[0].BalanceAfter);
    }

    private sealed class BalanceRow
    {
        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }
    }
}
