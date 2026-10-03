using LuuKyCanTin.Infrastructure.HeThong;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.HeThong;

public sealed class CurrentUserSessionTests
{
    [Fact]
    public void DaDangNhap_BeforeDangNhap_IsFalseWithNoUser()
    {
        var phien = new CurrentUserSession();

        phien.DaDangNhap.ShouldBeFalse();
        phien.NguoiDungId.ShouldBeNull();
        phien.TenDangNhap.ShouldBeNull();
    }

    [Fact]
    public void DangNhap_ThenDangXuat_SetsAndClearsTheUser()
    {
        var phien = new CurrentUserSession();

        phien.DangNhap(3, "thuquy");
        phien.DaDangNhap.ShouldBeTrue();
        phien.NguoiDungId.ShouldBe(3);
        phien.TenDangNhap.ShouldBe("thuquy");

        phien.DangXuat();
        phien.DaDangNhap.ShouldBeFalse();
        phien.NguoiDungId.ShouldBeNull();
    }
}
