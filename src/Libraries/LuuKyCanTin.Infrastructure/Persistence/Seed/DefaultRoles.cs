using LuuKyCanTin.Application.Administration;

namespace LuuKyCanTin.Infrastructure.Persistence.Seed;

/// <summary>
/// The 6 standard roles and their default permission sets (Story 2.3 AC 3). Permission codes come only from
/// <see cref="PermissionCodes"/>, never from string literals here.
/// </summary>
internal static class DefaultRoles
{
    public sealed record RoleDefinition(int Id, string Code, string Name, IReadOnlyList<string> Permission);

    public static readonly IReadOnlyList<RoleDefinition> All =
    [
        new(1, RoleCodes.Administrator, "Quản trị hệ thống",
        [
            .. PermissionCodes.ForModule(PermissionCodes.ModuleAdministration),
            .. PermissionCodes.ForModule(PermissionCodes.ModuleMasterData),
        ]),
        new(2, RoleCodes.CustodyOfficer, "Cán bộ theo dõi tiền lưu ký",
        [
            .. PermissionCodes.ForModule(PermissionCodes.ModuleCustodyIncrease, includeApprove: false),
            .. PermissionCodes.ForModule(PermissionCodes.ModuleCustodyDecrease, includeApprove: false),
            .. PermissionCodes.ForModule(PermissionCodes.ModuleCustodyReporting, includeApprove: false),
            PermissionCodes.MasterData.View,
        ]),
        new(3, RoleCodes.CanteenOfficer, "Cán bộ căn tin / bán hàng",
        [
            .. PermissionCodes.ForModule(PermissionCodes.ModuleGoodsReceipt),
            .. PermissionCodes.ForModule(PermissionCodes.ModuleSales),
            .. PermissionCodes.ForModule(PermissionCodes.ModuleInventoryReporting),
            PermissionCodes.CustodyReporting.View,
        ]),
        new(4, RoleCodes.SupervisingOfficer, "Cán bộ quản giáo",
        [
            PermissionCodes.CustodyReporting.View,
        ]),
        new(5, RoleCodes.Leader, "Chỉ huy phụ trách / Lãnh đạo đơn vị",
        [
            PermissionCodes.CustodyIncrease.Approve,
            PermissionCodes.CustodyDecrease.Approve,
            PermissionCodes.GoodsReceipt.Approve,
            PermissionCodes.CustodyReporting.View,
            PermissionCodes.InventoryReporting.View,
            PermissionCodes.Administration.View,
        ]),
        new(6, RoleCodes.Accountant, "Kế toán đơn vị",
        [
            PermissionCodes.CustodyReporting.View,
            PermissionCodes.InventoryReporting.View,
        ]),
    ];
}
