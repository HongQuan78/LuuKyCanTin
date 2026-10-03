using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Infrastructure.Administration;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Administration;

public sealed class CurrentUserSessionTests
{
    [Fact]
    public void IsSignedIn_BeforeSignIn_IsFalseWithNoUserAndNoPermission()
    {
        var session = new CurrentUserSession();

        session.IsSignedIn.ShouldBeFalse();
        session.UserId.ShouldBeNull();
        session.UserName.ShouldBeNull();
        session.OfficerId.ShouldBeNull();
        session.FullName.ShouldBeNull();
        session.HasPermission(PermissionCodes.MasterData.View).ShouldBeFalse();
    }

    [Fact]
    public void SignIn_ThenSignOut_SetsAndClearsTheUserAndThePermissionSnapshot()
    {
        var session = new CurrentUserSession();

        session.SignIn(
            3, "thuquy", officerId: 7, fullName: "Nguyễn Thị Thủ Quỹ",
            permissionCodes: [PermissionCodes.MasterData.View, PermissionCodes.CustodyIncrease.Create]);
        session.IsSignedIn.ShouldBeTrue();
        session.UserId.ShouldBe(3);
        session.UserName.ShouldBe("thuquy");
        session.OfficerId.ShouldBe(7);
        session.FullName.ShouldBe("Nguyễn Thị Thủ Quỹ");
        session.HasPermission(PermissionCodes.MasterData.View).ShouldBeTrue();
        session.HasPermission(PermissionCodes.MasterData.Create).ShouldBeFalse();

        session.SignOut();
        session.IsSignedIn.ShouldBeFalse();
        session.UserId.ShouldBeNull();
        session.OfficerId.ShouldBeNull();
        session.FullName.ShouldBeNull();
        session.HasPermission(PermissionCodes.MasterData.View).ShouldBeFalse();
    }
}
