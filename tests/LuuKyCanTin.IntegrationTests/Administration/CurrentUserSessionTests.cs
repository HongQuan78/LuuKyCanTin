using LuuKyCanTin.Infrastructure.Administration;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

public sealed class CurrentUserSessionTests
{
    [Fact]
    public void IsSignedIn_BeforeSignIn_IsFalseWithNoUser()
    {
        var session = new CurrentUserSession();

        session.IsSignedIn.ShouldBeFalse();
        session.UserId.ShouldBeNull();
        session.UserName.ShouldBeNull();
        session.OfficerId.ShouldBeNull();
        session.FullName.ShouldBeNull();
    }

    [Fact]
    public void SignIn_ThenSignOut_SetsAndClearsTheUser()
    {
        var session = new CurrentUserSession();

        session.SignIn(3, "thuquy", officerId: 7, fullName: "Nguyễn Thị Thủ Quỹ");
        session.IsSignedIn.ShouldBeTrue();
        session.UserId.ShouldBe(3);
        session.UserName.ShouldBe("thuquy");
        session.OfficerId.ShouldBe(7);
        session.FullName.ShouldBe("Nguyễn Thị Thủ Quỹ");

        session.SignOut();
        session.IsSignedIn.ShouldBeFalse();
        session.UserId.ShouldBeNull();
        session.OfficerId.ShouldBeNull();
        session.FullName.ShouldBeNull();
    }
}
