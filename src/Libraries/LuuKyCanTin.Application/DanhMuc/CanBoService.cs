using System.Linq.Expressions;
using FluentValidation;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.DanhMuc;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.DanhMuc;

public sealed class CanBoService(IAppDbContext db, IValidator<LuuCanBoRequest> validator) : ICanBoService
{
    public const string LoiTrungMa = "Mã cán bộ đã tồn tại";
    public const string LoiKhongTimThay = "Không tìm thấy cán bộ.";

    private static readonly Expression<Func<CanBo, CanBoDto>> ThanhDto =
        c => new CanBoDto(c.Id, c.MaCanBo, c.HoTen, c.ChucVu, c.LaQuanGiao, c.DangCongTac, c.RowVer);

    private static readonly Func<CanBo, CanBoDto> SangDto = ThanhDto.Compile();

    public async Task<CanBoDto> ThemAsync(LuuCanBoRequest request, CancellationToken ct = default)
    {
        // TODO: require permission DM.Them once IKiemTraQuyen exists.
        await KiemTraHopLeAsync(request, ct);
        var canBo = new CanBo(request.MaCanBo, request.HoTen, request.ChucVu, request.LaQuanGiao)
        {
            DangCongTac = request.DangCongTac,
        };
        await KiemTraTrungMaAsync(canBo.MaCanBo, idDangSua: null, ct);

        db.CanBo.Add(canBo);
        await LuuAsync(ct);
        return SangDto(canBo);
    }

    public async Task<CanBoDto> SuaAsync(int id, LuuCanBoRequest request, CancellationToken ct = default)
    {
        // TODO: require permission DM.Sua once IKiemTraQuyen exists.
        if (request.RowVer is null)
            throw new ArgumentException("An edit must carry the row version the user loaded.", nameof(request));
        await KiemTraHopLeAsync(request, ct);

        var canBo = await db.CanBo.SingleOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new LoiNghiepVuException(LoiKhongTimThay);
        // The UPDATE then only succeeds while the row is still the version the user saw.
        db.Entry(canBo).Property(c => c.RowVer).OriginalValue = request.RowVer;
        canBo.CapNhat(request.MaCanBo, request.HoTen, request.ChucVu, request.LaQuanGiao);
        canBo.DangCongTac = request.DangCongTac;
        await KiemTraTrungMaAsync(canBo.MaCanBo, idDangSua: id, ct);

        await LuuAsync(ct);
        return SangDto(canBo);
    }

    public async Task<IReadOnlyList<CanBoDto>> TimAsync(string? tuKhoa, bool baoGomNgungCongTac, CancellationToken ct = default)
    {
        var query = db.CanBo.AsNoTracking();
        if (!baoGomNgungCongTac)
            query = query.Where(c => c.DangCongTac);
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            // Two variables give two SQL parameters. A shared one would be typed varchar after MaCanBo and lose the
            // diacritics of a keyword meant for the nvarchar HoTen.
            var theoMa = tuKhoa.Trim();
            var theoTen = theoMa;
            query = query.Where(c => c.MaCanBo.Contains(theoMa) || c.HoTen.Contains(theoTen));
        }

        return await SapXep(query).Select(ThanhDto).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CanBoDto>> LayCanBoDangCongTacAsync(bool chiQuanGiao, CancellationToken ct = default)
    {
        var query = db.CanBo.AsNoTracking().Where(c => c.DangCongTac);
        if (chiQuanGiao)
            query = query.Where(c => c.LaQuanGiao);

        return await SapXep(query).Select(ThanhDto).ToListAsync(ct);
    }

    private static IQueryable<CanBo> SapXep(IQueryable<CanBo> query) => query.OrderBy(c => c.HoTen).ThenBy(c => c.MaCanBo);

    private async Task KiemTraHopLeAsync(LuuCanBoRequest request, CancellationToken ct)
    {
        var ketQua = await validator.ValidateAsync(request, ct);
        if (!ketQua.IsValid)
            throw new LoiNghiepVuException(string.Join('\n', ketQua.Errors.Select(e => e.ErrorMessage)));
    }

    // The unique index is the backstop when two workstations save the same code at once.
    private async Task KiemTraTrungMaAsync(string maCanBo, int? idDangSua, CancellationToken ct)
    {
        if (await db.CanBo.AnyAsync(c => c.MaCanBo == maCanBo && c.Id != idDangSua, ct))
            throw new LoiNghiepVuException(LoiTrungMa);
    }

    private async Task LuuAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (TrungGiaTriDuyNhatException ex)
        {
            throw new LoiNghiepVuException(LoiTrungMa, ex);
        }
    }
}
