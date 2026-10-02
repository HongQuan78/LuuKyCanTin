using LuuKyCanTin.Application.Common;

namespace LuuKyCanTin.Application.DanhMuc;

/// <summary>The staff register. Staff are never deleted; someone who has left is marked inactive.</summary>
public interface ICanBoService
{
    /// <exception cref="LoiNghiepVuException">Invalid input, or the code is already used.</exception>
    Task<CanBoDto> ThemAsync(LuuCanBoRequest request, CancellationToken ct = default);

    /// <exception cref="LoiNghiepVuException">Invalid input, the code is already used, or the record doesn't exist.</exception>
    /// <exception cref="XungDotDuLieuException">Someone else changed the record after <see cref="LuuCanBoRequest.RowVer"/> was loaded.</exception>
    Task<CanBoDto> SuaAsync(int id, LuuCanBoRequest request, CancellationToken ct = default);

    /// <summary>Matches part of the code or the name, ignoring case and diacritics.</summary>
    Task<IReadOnlyList<CanBoDto>> TimAsync(string? tuKhoa, bool baoGomNgungCongTac, CancellationToken ct = default);

    /// <summary>The staff a selection list may offer (wardens, signers, accounts): active staff only.</summary>
    Task<IReadOnlyList<CanBoDto>> LayCanBoDangCongTacAsync(bool chiQuanGiao, CancellationToken ct = default);
}
