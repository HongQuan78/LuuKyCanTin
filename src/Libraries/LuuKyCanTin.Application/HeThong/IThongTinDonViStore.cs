using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.HeThong;

public interface IThongTinDonViStore
{
    /// <summary>The single unit-information row (Id = 1), or null on a database that was never seeded.</summary>
    Task<ThongTinDonVi?> LayAsync(CancellationToken ct = default);
}
