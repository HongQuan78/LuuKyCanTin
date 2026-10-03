using LuuKyCanTin.Application.Administration;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

/// <summary>
/// The pure decision behind the last-administrator guard: a snapshot of accounts × roles in, "is anyone left?" out.
/// </summary>
public class LastAdministratorGuardTests
{
    private const int AdministratorRoleId = 1;
    private const int AccountantRoleId = 2;

    private static readonly Dictionary<int, IReadOnlyCollection<string>> PermissionsByRole = new()
    {
        [AdministratorRoleId] = [PermissionCodes.Administration.View, PermissionCodes.Administration.Update],
        [AccountantRoleId] = [PermissionCodes.CustodyReporting.View, PermissionCodes.InventoryReporting.View],
    };

    private static LastAdministratorGuard.AccountState Account(int id, bool isActive, params int[] roleIds) =>
        new(id, isActive, roleIds);

    [Fact]
    public void NoAccounts_LeavesNobody()
    {
        LastAdministratorGuard.AnyAdministratorRemains([], PermissionsByRole).ShouldBeFalse();
    }

    [Fact]
    public void ActiveAccountWithTheAdminRole_IsEnough()
    {
        LastAdministratorGuard.AnyAdministratorRemains(
            [Account(1, isActive: true, AdministratorRoleId)], PermissionsByRole).ShouldBeTrue();
    }

    [Fact]
    public void InactiveAdmin_DoesNotCount()
    {
        LastAdministratorGuard.AnyAdministratorRemains(
            [Account(1, isActive: false, AdministratorRoleId)], PermissionsByRole).ShouldBeFalse();
    }

    [Fact]
    public void ActiveAccountWithoutTheAdminRole_DoesNotCount()
    {
        LastAdministratorGuard.AnyAdministratorRemains(
            [Account(1, isActive: true, AccountantRoleId)], PermissionsByRole).ShouldBeFalse();
    }

    [Fact]
    public void OneAdminAmongSeveralAccounts_IsEnough()
    {
        LastAdministratorGuard.AnyAdministratorRemains(
            [
                Account(1, isActive: true, AccountantRoleId),
                Account(2, isActive: false, AdministratorRoleId),
                Account(3, isActive: true, AdministratorRoleId),
            ],
            PermissionsByRole).ShouldBeTrue();
    }

    [Fact]
    public void UnknownRoleId_DoesNotCount()
    {
        LastAdministratorGuard.AnyAdministratorRemains(
            [Account(1, isActive: true, 99)], PermissionsByRole).ShouldBeFalse();
    }
}
