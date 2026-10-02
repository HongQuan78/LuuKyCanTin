using LuuKyCanTin.Application.Common;

namespace LuuKyCanTin.Application.HeThong;

/// <summary>The role list and the permission set of one role. Roles are fixed; only their grants change.</summary>
public interface IVaiTroService
{
    Task<IReadOnlyList<VaiTroDto>> LayDanhSachAsync(CancellationToken ct = default);

    /// <summary>The permission codes granted to the role, in catalogue order.</summary>
    Task<IReadOnlyList<string>> LayQuyenCuaVaiTroAsync(int vaiTroId, CancellationToken ct = default);

    /// <summary>Replaces the role's permission set and writes one audit row with the before/after lists.</summary>
    /// <exception cref="LoiNghiepVuException">The role doesn't exist, or a code is not in the catalogue.</exception>
    /// <exception cref="XungDotDuLieuException">Someone else changed the role after <paramref name="rowVer"/> was loaded.</exception>
    /// <exception cref="Domain.HeThong.KhongCoQuyenException">The current user lacks HT.Sua.</exception>
    Task CapNhatQuyenAsync(int vaiTroId, IReadOnlyCollection<string> maQuyen, byte[] rowVer, CancellationToken ct = default);
}
