using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.LuuKy;

namespace LuuKyCanTin.Application.LuuKy;

/// <summary>
/// The one engine that writes custodial documents and balances. Every later money flow calls it; nothing
/// bypasses it, even in the walking skeleton. This version supports receipts only (Epic 4 adds the rest).
/// </summary>
public sealed class GhiSoLuuKyService(
    IDoiTuongStore doiTuongStore,
    IChungTuLuuKyStore chungTuStore,
    IAppDbContext db,
    INumberingService numberingService,
    ISoDuLuuKyWriter soDuWriter,
    IClock clock)
{
    public const string MaLoaiChungTu = "BNT";

    private readonly GhiSoBienNhanThuValidator _validator = new();

    /// <summary>
    /// Posts a receipt: number, balance, document and audit row commit or roll back together. Business
    /// failures come back as <see cref="KetQuaGhiSo.Loi"/> without touching the database.
    /// </summary>
    public async Task<KetQuaGhiSo> GhiSoBienNhanThuAsync(GhiSoBienNhanThuRequest request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return KetQuaGhiSo.Loi(validation.Errors[0].ErrorMessage);

        await using var giaoDich = await db.BeginTransactionAsync(ct);

        // Snapshot from the master inside the transaction, and refuse a detainee who is no longer managed.
        var doiTuong = await doiTuongStore.TimTheoIdAsync(request.DoiTuongId, ct);
        if (doiTuong is null || doiTuong.TrangThai != TrangThaiDoiTuong.DangQuanLy)
        {
            await giaoDich.RollbackAsync(ct);
            return KetQuaGhiSo.Loi("Đối tượng không tồn tại hoặc không còn được quản lý.");
        }

        // The counter year follows the working date; back-date rules arrive with the period lock (Epic 4/6).
        var soChungTu = await numberingService.CapSoAsync(MaLoaiChungTu, clock.Today.Year, ct);

        // The balance update is the concurrency point: a conditional UPDATE under a row lock, never read-then-write.
        var soDu = await soDuWriter.CongAsync(doiTuong.Id, request.SoTien, ct);
        if (soDu is null)
        {
            await giaoDich.RollbackAsync(ct);
            return KetQuaGhiSo.Loi("Không cập nhật được số dư lưu ký.");
        }

        var chungTu = ChungTuLuuKy.TaoBienNhanThuDaGhiSo(
            soChungTu,
            request.NgayChungTu,
            doiTuong,
            LoaiPhieu.Thu,
            request.NghiepVu,
            request.HinhThuc,
            request.NguoiGuiHoTen?.Trim(),
            request.QuanHe?.Trim(),
            request.SoPhieuGoc?.Trim(),
            request.SoTaiKhoanNguoiGui?.Trim(),
            request.NgayNhan,
            request.NoiDung?.Trim(),
            request.SoTien,
            SoTienBangChu.Doc(request.SoTien),
            soDu.Value.SoDuTruoc,
            soDu.Value.SoDuSau);

        chungTuStore.Them(chungTu);

        // The audit interceptor sees the open transaction and writes the "Them" row into it.
        await db.SaveChangesAsync(ct);
        await giaoDich.CommitAsync(ct);

        return KetQuaGhiSo.Ok(chungTu.Id, chungTu.SoChungTu, soDu.Value.SoDuSau);
    }
}
