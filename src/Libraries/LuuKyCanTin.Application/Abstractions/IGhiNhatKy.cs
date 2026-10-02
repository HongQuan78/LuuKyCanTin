using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// Writes an audit-log row for a business event that changes no voucher row (sign-in, print, approval).
/// Voucher changes are logged automatically on save; don't log them again here.
/// </summary>
public interface IGhiNhatKy
{
    /// <param name="duLieuMoi">Serialized to JSON as the row's <c>DuLieuMoi</c>. Never pass secrets.</param>
    /// <remarks>Saves the current unit of work, so any pending changes in it are saved too.</remarks>
    Task GhiAsync(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieuMoi = null, CancellationToken ct = default);

    /// <summary>Logs a change with its before and after values, for join tables the interceptor can't follow.</summary>
    Task GhiAsync(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieuCu, object? duLieuMoi, CancellationToken ct = default);
}
