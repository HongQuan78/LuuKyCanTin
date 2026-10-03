using LuuKyCanTin.Infrastructure.HeThong;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.HeThong;

public class CurrentUserSessionTests
{
    [Fact]
    public void BeforeSignIn_NobodyIsSignedIn()
    {
        var session = new CurrentUserSession();

        session.DaDangNhap.ShouldBeFalse();
        session.NguoiDungId.ShouldBeNull();
        session.TenDangNhap.ShouldBeNull();
    }

    [Fact]
    public void DangNhap_ThenDangXuat_SetsAndClearsTheUser()
    {
        var session = new CurrentUserSession();

        session.DangNhap(3, "thuquy", canBoId: 7, hoTen: "Nguyễn Thị Thủ Quỹ");
        session.DaDangNhap.ShouldBeTrue();
        session.NguoiDungId.ShouldBe(3);
        session.TenDangNhap.ShouldBe("thuquy");
        session.CanBoId.ShouldBe(7);
        session.HoTen.ShouldBe("Nguyễn Thị Thủ Quỹ");

        session.DangXuat();
        session.DaDangNhap.ShouldBeFalse();
        session.NguoiDungId.ShouldBeNull();
        session.CanBoId.ShouldBeNull();
        session.HoTen.ShouldBeNull();
    }
}
