using LuuKyCanTin.Domain.DanhMuc;

namespace LuuKyCanTin.Application.DanhMuc;

/// <summary>Persistence port for the detainee table. Narrow on purpose: only what the skeleton's use cases need.</summary>
public interface IDoiTuongStore
{
    Task<DoiTuong?> TimTheoIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<DoiTuong>> LayDangQuanLyAsync(CancellationToken ct = default);

    Task<bool> MaSoDaTonTaiAsync(string maSo, CancellationToken ct = default);

    /// <summary>
    /// Adds and saves the detainee. Returns false when the unique <c>MaSo</c> index rejects it because a
    /// concurrent save won the race; the failed entity must not stay tracked for the context's next save.
    /// </summary>
    Task<bool> ThemAsync(DoiTuong doiTuong, CancellationToken ct = default);
}
