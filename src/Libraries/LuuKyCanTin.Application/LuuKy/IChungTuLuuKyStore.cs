using LuuKyCanTin.Domain.LuuKy;

namespace LuuKyCanTin.Application.LuuKy;

/// <summary>Persistence port for vouchers. Writing goes through SaveChanges so the audit interceptor sees it.</summary>
public interface IChungTuLuuKyStore
{
    void Them(ChungTuLuuKy chungTu);

    Task<ChungTuLuuKy?> TimTheoIdAsync(long id, CancellationToken ct = default);
}
