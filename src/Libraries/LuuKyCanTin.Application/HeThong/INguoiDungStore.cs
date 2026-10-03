using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.HeThong;

public interface INguoiDungStore
{
    Task<NguoiDung?> TimTheoDangNhapAsync(string tenDangNhap, CancellationToken ct = default);

    Task<NguoiDung?> TimTheoIdAsync(int nguoiDungId, CancellationToken ct = default);

    /// <summary>The staff name to show for a signed-in account; null when the link is missing.</summary>
    Task<string?> LayHoTenCanBoAsync(int canBoId, CancellationToken ct = default);

    /// <summary>Saves the account through the context that translates provider errors and writes the audit row.</summary>
    Task LuuAsync(NguoiDung nguoiDung, CancellationToken ct = default);

    /// <summary>
    /// Refreshes a tracked account after a row-version conflict, so the caller can re-apply its change to the
    /// values another workstation wrote in the meantime.
    /// </summary>
    Task TaiLaiAsync(NguoiDung nguoiDung, CancellationToken ct = default);
}
