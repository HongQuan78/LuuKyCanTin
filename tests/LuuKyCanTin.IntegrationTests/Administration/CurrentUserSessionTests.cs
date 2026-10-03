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
    }

    [Fact]
    public void SignIn_ThenSignOut_SetsAndClearsTheUser()
    {
        var session = new CurrentUserSession();

        session.SignIn(3, "thuquy");
        session.IsSignedIn.ShouldBeTrue();
        session.UserId.ShouldBe(3);
        session.UserName.ShouldBe("thuquy");

        session.SignOut();
        session.IsSignedIn.ShouldBeFalse();
        session.UserId.ShouldBeNull();
    }
}
