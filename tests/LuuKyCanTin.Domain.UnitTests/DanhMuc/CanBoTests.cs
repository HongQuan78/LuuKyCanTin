using LuuKyCanTin.Domain.DanhMuc;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.DanhMuc;

public class CanBoTests
{
    [Fact]
    public void New_TrimsInput_UpperCasesTheCode_AndIsActive()
    {
        var canBo = new CanBo("  cb-01 ", " Nguyễn Văn An  ", " Cán bộ quản giáo ", laQuanGiao: true);

        canBo.MaCanBo.ShouldBe("CB-01");
        canBo.HoTen.ShouldBe("Nguyễn Văn An");
        canBo.ChucVu.ShouldBe("Cán bộ quản giáo");
        canBo.LaQuanGiao.ShouldBeTrue();
        canBo.DangCongTac.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankPosition_IsStoredAsNull(string? chucVu)
    {
        new CanBo("CB01", "Lê Thị Bình", chucVu, laQuanGiao: false).ChucVu.ShouldBeNull();
    }

    [Theory]
    [InlineData("", "Lê Thị Bình")]
    [InlineData("CB01", "  ")]
    public void BlankCodeOrName_IsRejected(string maCanBo, string hoTen)
    {
        Should.Throw<ArgumentException>(() => new CanBo(maCanBo, hoTen, null, laQuanGiao: false));
    }

    [Fact]
    public void CapNhat_NormalizesLikeTheConstructor()
    {
        var canBo = new CanBo("CB01", "Lê Thị Bình", null, laQuanGiao: false);

        canBo.CapNhat(" cb02 ", " Lê Thị Bình Minh ", "  ", laQuanGiao: true);

        canBo.MaCanBo.ShouldBe("CB02");
        canBo.HoTen.ShouldBe("Lê Thị Bình Minh");
        canBo.ChucVu.ShouldBeNull();
        canBo.LaQuanGiao.ShouldBeTrue();
    }

    [Fact]
    public void DangCongTac_CanBeTurnedOffAndBackOn()
    {
        var canBo = new CanBo("CB01", "Lê Thị Bình", null, laQuanGiao: false) { DangCongTac = false };
        canBo.DangCongTac.ShouldBeFalse();

        canBo.DangCongTac = true;
        canBo.DangCongTac.ShouldBeTrue();
    }
}
