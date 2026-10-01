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

        session.DangNhap(3, "thuquy");
        session.DaDangNhap.ShouldBeTrue();
        session.NguoiDungId.ShouldBe(3);
        session.TenDangNhap.ShouldBe("thuquy");

        session.DangXuat();
        session.DaDangNhap.ShouldBeFalse();
        session.NguoiDungId.ShouldBeNull();
    }
}
