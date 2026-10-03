using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.Administration;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

public class SignedInUserQueryTests
{
    private readonly InMemoryAppDbContext _db = new();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IFacilityInfoStore _facility = Substitute.For<IFacilityInfoStore>();

    private SignedInUserQuery CreateQuery() => new(_db, _currentUser, _facility);

    private async Task SeedAsync(params int[] roleIds)
    {
        _db.Role.AddRange(
            new Role { Id = 1, Code = "QUAN_TRI", Name = "Quản trị hệ thống" },
            new Role { Id = 2, Code = "KE_TOAN", Name = "Kế toán" });
        foreach (var roleId in roleIds)
            _db.UserRole.Add(new UserRole { UserId = 7, RoleId = roleId });
        await _db.SaveChangesAsync();
        _currentUser.UserId.Returns(7);
        _currentUser.UserName.Returns("lan.nt");
    }

    [Fact]
    public async Task Get_NobodySignedIn_ReturnsNull()
    {
        (await CreateQuery().GetAsync()).ShouldBeNull();
    }

    [Fact]
    public async Task Get_ASignedInUser_ReturnsTheUserNameAsDisplayNameAndTheRoleName()
    {
        await SeedAsync(1);

        var user = await CreateQuery().GetAsync();

        user.ShouldNotBeNull();
        user.UserName.ShouldBe("lan.nt");
        user.DisplayName.ShouldBe("lan.nt");
        user.RoleNames.ShouldBe("Quản trị hệ thống");
    }

    [Fact]
    public async Task Get_SeveralRoles_JoinsTheNamesInRoleOrder()
    {
        await SeedAsync(2, 1);

        (await CreateQuery().GetAsync())!.RoleNames.ShouldBe("Quản trị hệ thống, Kế toán");
    }

    [Fact]
    public async Task Get_NoRole_ReturnsAnEmptyRoleText()
    {
        await SeedAsync();

        (await CreateQuery().GetAsync())!.RoleNames.ShouldBe("");
    }

    [Fact]
    public async Task Get_AFacilityRow_ReturnsItsName()
    {
        await SeedAsync(1);
        _facility.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new FacilityInfo { Id = 1, FacilityName = "TRẠI TẠM GIAM SỐ 1" });

        (await CreateQuery().GetAsync())!.FacilityName.ShouldBe("TRẠI TẠM GIAM SỐ 1");
    }

    [Fact]
    public async Task Get_NoFacilityRow_ReturnsAnEmptyFacilityName()
    {
        await SeedAsync(1);
        _facility.GetAsync(Arg.Any<CancellationToken>()).Returns((FacilityInfo?)null);

        (await CreateQuery().GetAsync())!.FacilityName.ShouldBe("");
    }
}
