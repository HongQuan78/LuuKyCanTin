using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// Writes an audit-log row for a business event that changes no voucher row (sign-in, print, approval).
/// Voucher changes are logged automatically on save; don't log them again here.
/// </summary>
public interface IGhiNhatKy
{
    /// <param name="duLieu">Serialized to JSON as the row's <c>DuLieuMoi</c>. Never pass secrets.</param>
    /// <remarks>Saves the current unit of work, so any pending changes in it are saved too.</remarks>
    Task GhiAsync(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieu = null, CancellationToken ct = default);
}
