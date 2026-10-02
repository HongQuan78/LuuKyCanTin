using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.LuuKy;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.LuuKy;

public class ChungTuLuuKyTests
{
    private static DoiTuong DoiTuong() => new()
    {
        Id = 7,
        MaSo = "DT-0001",
        HoTen = "Nguyễn Văn A",
        LoaiDoiTuong = LoaiDoiTuong.TamGiuTamGiam,
        NgayVao = new DateOnly(2026, 9, 1),
    };

    private static ChungTuLuuKy Tao(
        decimal soTien = 500_000,
        LoaiPhieu loaiPhieu = LoaiPhieu.Thu,
        NghiepVu nghiepVu = NghiepVu.NguoiThanGui,
        HinhThuc hinhThuc = HinhThuc.TienMat,
        string? soTaiKhoanNguoiGui = null) =>
        ChungTuLuuKy.TaoBienNhanThuDaGhiSo(
            "BNT-2026-00001",
            new DateOnly(2026, 10, 1),
            DoiTuong(),
            loaiPhieu,
            nghiepVu,
            hinhThuc,
            "Trần Thị B",
            "Mẹ",
            soPhieuGoc: null,
            soTaiKhoanNguoiGui,
            ngayNhan: null,
            "Tiền gửi lưu ký",
            soTien,
            "Năm trăm nghìn đồng",
            soDuTruoc: 0,
            soDuSau: soTien);

    [Fact]
    public void Factory_PostsTheReceiptWithTheSnapshotAndBalances()
    {
        var chungTu = Tao();

        chungTu.SoChungTu.ShouldBe("BNT-2026-00001");
        chungTu.NgayChungTu.ShouldBe(new DateOnly(2026, 10, 1));
        chungTu.TrangThai.ShouldBe(TrangThaiChungTu.DaGhiSo);
        chungTu.DaHuy.ShouldBeFalse();
        chungTu.LoaiPhieu.ShouldBe(LoaiPhieu.Thu);
        chungTu.NghiepVu.ShouldBe(NghiepVu.NguoiThanGui);
        chungTu.DoiTuongId.ShouldBe(7);
        chungTu.HoTenDoiTuong.ShouldBe("Nguyễn Văn A");
        chungTu.LoaiDoiTuong.ShouldBe(LoaiDoiTuong.TamGiuTamGiam);
        chungTu.SoTien.ShouldBe(500_000);
        chungTu.SoDuTruoc.ShouldBe(0);
        chungTu.SoDuSau.ShouldBe(500_000);
        chungTu.SoLanIn.ShouldBe((short)0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Factory_RejectsNonPositiveAmounts(decimal soTien)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Tao(soTien));
    }

    [Fact]
    public void Factory_RejectsABusinessCodeThatDoesNotMatchTheVoucherType()
    {
        Should.Throw<ArgumentException>(() => Tao(loaiPhieu: LoaiPhieu.Chi));
    }

    [Fact]
    public void Factory_RejectsAnUnknownBusinessCode()
    {
        Should.Throw<ArgumentException>(() => Tao(nghiepVu: (NghiepVu)99));
    }

    [Fact]
    public void Factory_RequiresAnAccountForATransfer()
    {
        Should.Throw<ArgumentException>(() => Tao(hinhThuc: HinhThuc.ChuyenKhoan));
    }

    [Fact]
    public void Factory_AcceptsATransferWithAnAccount()
    {
        var chungTu = Tao(hinhThuc: HinhThuc.ChuyenKhoan, soTaiKhoanNguoiGui: "123456789");

        chungTu.HinhThuc.ShouldBe(HinhThuc.ChuyenKhoan);
        chungTu.SoTaiKhoanNguoiGui.ShouldBe("123456789");
    }
}
