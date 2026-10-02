using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.DanhMuc;

namespace LuuKyCanTin.Domain.LuuKy;

/// <summary>
/// One posted movement of custodial money. The detainee's name and type are snapshotted at posting, so a
/// reprint always matches the original even after the master record changes.
/// </summary>
public sealed class ChungTuLuuKy : AuditableEntity, IAuditable, ICoTrangThaiHuy
{
    private ChungTuLuuKy()
    {
    }

    public long Id { get; private set; }

    public string SoChungTu { get; private set; } = "";

    public DateOnly NgayChungTu { get; private set; }

    public LoaiPhieu LoaiPhieu { get; private set; }

    public NghiepVu NghiepVu { get; private set; }

    public HinhThuc HinhThuc { get; private set; }

    public TrangThaiChungTu TrangThai { get; private set; }

    public int DoiTuongId { get; private set; }

    /// <summary>Snapshot of the detainee's name at posting time; never read back from the master.</summary>
    public string HoTenDoiTuong { get; private set; } = "";

    /// <summary>Snapshot of the detainee's type at posting time.</summary>
    public LoaiDoiTuong LoaiDoiTuong { get; private set; }

    public string? NguoiGuiHoTen { get; private set; }

    public string? QuanHe { get; private set; }

    /// <summary>The source paper (custodial deposit slip, gift slip, give-receive minutes).</summary>
    public string? SoPhieuGoc { get; private set; }

    public string? SoTaiKhoanNguoiGui { get; private set; }

    public DateOnly? NgayNhan { get; private set; }

    public string? NoiDung { get; private set; }

    public decimal SoTien { get; private set; }

    /// <summary>Stored at posting time so a reprint matches the original even if the converter changes.</summary>
    public string SoTienBangChu { get; private set; } = "";

    public decimal SoDuTruoc { get; private set; }

    public decimal SoDuSau { get; private set; }

    public string? LyDoHuy { get; private set; }

    public DateTime? NgayHuy { get; private set; }

    public int? NguoiHuyId { get; private set; }

    public short SoLanIn { get; private set; }

    public bool DaHuy => TrangThai == TrangThaiChungTu.DaHuy;

    /// <summary>
    /// The walking skeleton creates a receipt directly as posted. The full Draft → Posted → Cancelled state
    /// machine (NEN-14) arrives in Epic 4; the factory keeps the invariants the database CHECKs enforce.
    /// </summary>
    public static ChungTuLuuKy TaoBienNhanThuDaGhiSo(
        string soChungTu,
        DateOnly ngayChungTu,
        DoiTuong doiTuong,
        LoaiPhieu loaiPhieu,
        NghiepVu nghiepVu,
        HinhThuc hinhThuc,
        string? nguoiGuiHoTen,
        string? quanHe,
        string? soPhieuGoc,
        string? soTaiKhoanNguoiGui,
        DateOnly? ngayNhan,
        string? noiDung,
        decimal soTien,
        string soTienBangChu,
        decimal soDuTruoc,
        decimal soDuSau)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(soChungTu);
        ArgumentNullException.ThrowIfNull(doiTuong);
        ArgumentException.ThrowIfNullOrWhiteSpace(soTienBangChu);

        if (soTien <= 0)
            throw new ArgumentOutOfRangeException(nameof(soTien), soTien, "Số tiền phải lớn hơn 0.");

        if (!Enum.IsDefined(nghiepVu) || (int)nghiepVu / 10 != (int)loaiPhieu)
            throw new ArgumentException("Nghiệp vụ phải thuộc loại phiếu.", nameof(nghiepVu));

        if (hinhThuc == HinhThuc.ChuyenKhoan && string.IsNullOrWhiteSpace(soTaiKhoanNguoiGui))
            throw new ArgumentException("Chuyển khoản phải có số tài khoản người gửi.", nameof(soTaiKhoanNguoiGui));

        return new ChungTuLuuKy
        {
            SoChungTu = soChungTu,
            NgayChungTu = ngayChungTu,
            LoaiPhieu = loaiPhieu,
            NghiepVu = nghiepVu,
            HinhThuc = hinhThuc,
            TrangThai = TrangThaiChungTu.DaGhiSo,
            DoiTuongId = doiTuong.Id,
            HoTenDoiTuong = doiTuong.HoTen,
            LoaiDoiTuong = doiTuong.LoaiDoiTuong,
            NguoiGuiHoTen = nguoiGuiHoTen,
            QuanHe = quanHe,
            SoPhieuGoc = soPhieuGoc,
            SoTaiKhoanNguoiGui = soTaiKhoanNguoiGui,
            NgayNhan = ngayNhan,
            NoiDung = noiDung,
            SoTien = soTien,
            SoTienBangChu = soTienBangChu,
            SoDuTruoc = soDuTruoc,
            SoDuSau = soDuSau,
        };
    }
}
