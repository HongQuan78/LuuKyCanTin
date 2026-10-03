using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.HeThong;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.HeThong;

public sealed class VaiTroService(
    IAppDbContext db,
    IKiemTraQuyen kiemTraQuyen,
    IGhiNhatKy ghiNhatKy,
    KiemTraConQuanTri kiemTraConQuanTri) : IVaiTroService
{
    public const string LoiKhongTimThayVaiTro = "Không tìm thấy vai trò.";
    public const string LoiMaQuyenKhongHopLe = "Mã quyền không hợp lệ: ";

    public async Task<IReadOnlyList<VaiTroDto>> LayDanhSachAsync(CancellationToken ct = default) =>
        await db.VaiTro
            .AsNoTracking()
            .OrderBy(v => v.Id)
            .Select(v => new VaiTroDto(v.Id, v.Ma, v.Ten, v.RowVer))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> LayQuyenCuaVaiTroAsync(int vaiTroId, CancellationToken ct = default) =>
        await (from vaiTroQuyen in db.VaiTroQuyen
               join quyen in db.Quyen on vaiTroQuyen.QuyenId equals quyen.Id
               where vaiTroQuyen.VaiTroId == vaiTroId
               orderby quyen.Id
               select quyen.Ma).ToListAsync(ct);

    public async Task CapNhatQuyenAsync(
        int vaiTroId, IReadOnlyCollection<string> maQuyen, byte[] rowVer, CancellationToken ct = default)
    {
        // Authorization first, before any read or transaction, so a refused call writes nothing at all.
        await kiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua, ct);

        var maKhongHopLe = maQuyen.Where(ma => MaQuyen.TatCa.All(q => q.Ma != ma)).ToList();
        if (maKhongHopLe.Count > 0)
            throw new LoiNghiepVuException(LoiMaQuyenKhongHopLe + string.Join(", ", maKhongHopLe));

        // Serializable, started before the first read: the last-administrator guard and the write are one atomic
        // unit, so removing HT.Sua from the only role that grants it can never slip through.
        await using var giaoDich = await db.BeginTransactionAsync(MucDoCoLapGiaoDich.TuanTu, ct);

        var vaiTro = await db.VaiTro.SingleOrDefaultAsync(v => v.Id == vaiTroId, ct)
            ?? throw new LoiNghiepVuException(LoiKhongTimThayVaiTro);

        var hienTai = await db.VaiTroQuyen.Where(v => v.VaiTroId == vaiTroId).ToListAsync(ct);
        var maTheoId = await db.Quyen.AsNoTracking().ToDictionaryAsync(q => q.Id, q => q.Ma, ct);
        var maCu = hienTai.Select(v => maTheoId[v.QuyenId]).Order(StringComparer.Ordinal).ToList();
        var maMoi = maQuyen.ToHashSet(StringComparer.Ordinal);
        var quyenTheoMa = MaQuyen.TatCa.ToDictionary(q => q.Ma, q => q.Id, StringComparer.Ordinal);

        await kiemTraConQuanTri.YeuCauKhiDoiQuyenVaiTroAsync(vaiTroId, maMoi, ct);

        db.VaiTroQuyen.RemoveRange(hienTai.Where(v => !maMoi.Contains(maTheoId[v.QuyenId])));
        foreach (var ma in maMoi.Where(ma => hienTai.All(v => maTheoId[v.QuyenId] != ma)))
            db.VaiTroQuyen.Add(new VaiTroQuyen { VaiTroId = vaiTroId, QuyenId = quyenTheoMa[ma] });

        // Touch the role so its row version changes: two administrators editing the same role then conflict.
        var entry = db.Entry(vaiTro);
        entry.Property(v => v.RowVer).OriginalValue = rowVer;
        entry.State = EntityState.Modified;

        await db.SaveChangesAsync(ct);
        await ghiNhatKy.GhiAsync(
            HanhDong.Sua, "VaiTro", vaiTroId,
            new { Quyen = maCu },
            new { Quyen = maMoi.Order(StringComparer.Ordinal).ToList() }, ct);
        await giaoDich.CommitAsync(ct);
    }
}
