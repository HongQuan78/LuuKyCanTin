using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Custody;

/// <summary>
/// Adding only queues the entity; the posting service calls SaveChanges inside its transaction so the audit
/// interceptor sees the voucher. Never use ExecuteUpdate/raw SQL for vouchers, or the log loses them.
/// </summary>
internal sealed class CustodyVoucherStore(AppDbContext db) : ICustodyVoucherStore
{
    public void Add(CustodyVoucher voucher) => db.CustodyVoucher.Add(voucher);

    public Task<CustodyVoucher?> FindByIdAsync(long id, CancellationToken ct = default) =>
        db.CustodyVoucher.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
}
