using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.IntegrationTests.Persistence.TestModel;

// A test-only voucher that exercises every shared convention and the audit log, since the real model has no vouchers yet.
public sealed class MauChungTu : AuditableEntity, IAuditable, ICoTrangThaiHuy
{
    public int Id { get; set; }
    public decimal SoTien { get; set; }
    public DateOnly NgayChungTu { get; set; }
    public DateTime? ThoiDiemIn { get; set; }
    public string NoiDung { get; set; } = "";
    public MauTrangThai TrangThai { get; set; }
    public MauTrangThai? TrangThaiTruoc { get; set; }

    [KhongGhiNhatKy]
    public string? MaBiMat { get; set; }

    public bool DaHuy => TrangThai == MauTrangThai.DaHuy;
}
