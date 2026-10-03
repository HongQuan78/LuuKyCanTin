using LuuKyCanTin.Domain.Custody;

namespace LuuKyCanTin.Application.Custody;

/// <summary>Persistence port for vouchers. Writing goes through SaveChanges so the audit interceptor sees it.</summary>
public interface ICustodyVoucherStore
{
    void Add(CustodyVoucher voucher);

    Task<CustodyVoucher?> FindByIdAsync(long id, CancellationToken ct = default);
}
