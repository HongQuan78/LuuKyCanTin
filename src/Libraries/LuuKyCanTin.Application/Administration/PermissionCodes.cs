namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// The only place permission codes are spelled. A code is <c>&lt;Module&gt;.&lt;Action&gt;</c>, for example
/// <c>LK-C.Duyet</c>. Services reference <see cref="Administration"/>, <see cref="CustodyDecrease"/> and friends
/// instead of literals.
/// </summary>
/// <remarks>
/// <see cref="All"/> is the seed for the <c>Permission</c> table and the allow-list for role editing. Its ids are
/// explicit and append-only: later stories add special permissions with the next free id, and the standard grid
/// keeps its numbering. The code values are spec-defined data stored in the database and shown in the role screen,
/// so they stay as the spec spells them.
/// </remarks>
public static class PermissionCodes
{
    public const string ActionView = "Xem";
    public const string ActionCreate = "Them";
    public const string ActionUpdate = "Sua";
    public const string ActionCancel = "Huy";
    public const string ActionPrint = "In";
    public const string ActionApprove = "Duyet";

    public const string ModuleAdministration = "HT";
    public const string ModuleMasterData = "DM";
    public const string ModuleCustodyIncrease = "LK-T";
    public const string ModuleCustodyDecrease = "LK-C";
    public const string ModuleCustodyReporting = "LK-BC";
    public const string ModuleGoodsReceipt = "NH";
    public const string ModuleSales = "BH";
    public const string ModuleInventoryReporting = "HH-BC";

    public sealed record PermissionDefinition(int Id, string Code, string Name, string Module);

    public sealed record ModuleDefinition(string Code, string Name);

    public static class Administration
    {
        public const string View = ModuleAdministration + "." + ActionView;
        public const string Create = ModuleAdministration + "." + ActionCreate;
        public const string Update = ModuleAdministration + "." + ActionUpdate;
        public const string Cancel = ModuleAdministration + "." + ActionCancel;
        public const string Print = ModuleAdministration + "." + ActionPrint;
        public const string Approve = ModuleAdministration + "." + ActionApprove;
    }

    public static class MasterData
    {
        public const string View = ModuleMasterData + "." + ActionView;
        public const string Create = ModuleMasterData + "." + ActionCreate;
        public const string Update = ModuleMasterData + "." + ActionUpdate;
        public const string Cancel = ModuleMasterData + "." + ActionCancel;
        public const string Print = ModuleMasterData + "." + ActionPrint;
        public const string Approve = ModuleMasterData + "." + ActionApprove;
    }

    public static class CustodyIncrease
    {
        public const string View = ModuleCustodyIncrease + "." + ActionView;
        public const string Create = ModuleCustodyIncrease + "." + ActionCreate;
        public const string Update = ModuleCustodyIncrease + "." + ActionUpdate;
        public const string Cancel = ModuleCustodyIncrease + "." + ActionCancel;
        public const string Print = ModuleCustodyIncrease + "." + ActionPrint;
        public const string Approve = ModuleCustodyIncrease + "." + ActionApprove;
    }

    public static class CustodyDecrease
    {
        public const string View = ModuleCustodyDecrease + "." + ActionView;
        public const string Create = ModuleCustodyDecrease + "." + ActionCreate;
        public const string Update = ModuleCustodyDecrease + "." + ActionUpdate;
        public const string Cancel = ModuleCustodyDecrease + "." + ActionCancel;
        public const string Print = ModuleCustodyDecrease + "." + ActionPrint;
        public const string Approve = ModuleCustodyDecrease + "." + ActionApprove;
    }

    public static class CustodyReporting
    {
        public const string View = ModuleCustodyReporting + "." + ActionView;
        public const string Create = ModuleCustodyReporting + "." + ActionCreate;
        public const string Update = ModuleCustodyReporting + "." + ActionUpdate;
        public const string Cancel = ModuleCustodyReporting + "." + ActionCancel;
        public const string Print = ModuleCustodyReporting + "." + ActionPrint;
        public const string Approve = ModuleCustodyReporting + "." + ActionApprove;
    }

    public static class GoodsReceipt
    {
        public const string View = ModuleGoodsReceipt + "." + ActionView;
        public const string Create = ModuleGoodsReceipt + "." + ActionCreate;
        public const string Update = ModuleGoodsReceipt + "." + ActionUpdate;
        public const string Cancel = ModuleGoodsReceipt + "." + ActionCancel;
        public const string Print = ModuleGoodsReceipt + "." + ActionPrint;
        public const string Approve = ModuleGoodsReceipt + "." + ActionApprove;
    }

    public static class Sales
    {
        public const string View = ModuleSales + "." + ActionView;
        public const string Create = ModuleSales + "." + ActionCreate;
        public const string Update = ModuleSales + "." + ActionUpdate;
        public const string Cancel = ModuleSales + "." + ActionCancel;
        public const string Print = ModuleSales + "." + ActionPrint;
        public const string Approve = ModuleSales + "." + ActionApprove;
    }

    public static class InventoryReporting
    {
        public const string View = ModuleInventoryReporting + "." + ActionView;
        public const string Create = ModuleInventoryReporting + "." + ActionCreate;
        public const string Update = ModuleInventoryReporting + "." + ActionUpdate;
        public const string Cancel = ModuleInventoryReporting + "." + ActionCancel;
        public const string Print = ModuleInventoryReporting + "." + ActionPrint;
        public const string Approve = ModuleInventoryReporting + "." + ActionApprove;
    }

    public static readonly IReadOnlyList<ModuleDefinition> Modules =
    [
        new(ModuleAdministration, "Hệ thống"),
        new(ModuleMasterData, "Danh mục"),
        new(ModuleCustodyIncrease, "Tăng tiền lưu ký"),
        new(ModuleCustodyDecrease, "Giảm tiền lưu ký"),
        new(ModuleCustodyReporting, "Báo cáo lưu ký"),
        new(ModuleGoodsReceipt, "Nhập hàng"),
        new(ModuleSales, "Bán hàng"),
        new(ModuleInventoryReporting, "Báo cáo hàng hóa"),
    ];

    public static readonly IReadOnlyList<string> Actions =
    [
        ActionView,
        ActionCreate,
        ActionUpdate,
        ActionCancel,
        ActionPrint,
        ActionApprove,
    ];

    public static readonly IReadOnlyList<PermissionDefinition> All =
    [
        new(1, Administration.View, BuildName(ActionView, ModuleAdministration), ModuleAdministration),
        new(2, Administration.Create, BuildName(ActionCreate, ModuleAdministration), ModuleAdministration),
        new(3, Administration.Update, BuildName(ActionUpdate, ModuleAdministration), ModuleAdministration),
        new(4, Administration.Cancel, BuildName(ActionCancel, ModuleAdministration), ModuleAdministration),
        new(5, Administration.Print, BuildName(ActionPrint, ModuleAdministration), ModuleAdministration),
        new(6, Administration.Approve, BuildName(ActionApprove, ModuleAdministration), ModuleAdministration),
        new(7, MasterData.View, BuildName(ActionView, ModuleMasterData), ModuleMasterData),
        new(8, MasterData.Create, BuildName(ActionCreate, ModuleMasterData), ModuleMasterData),
        new(9, MasterData.Update, BuildName(ActionUpdate, ModuleMasterData), ModuleMasterData),
        new(10, MasterData.Cancel, BuildName(ActionCancel, ModuleMasterData), ModuleMasterData),
        new(11, MasterData.Print, BuildName(ActionPrint, ModuleMasterData), ModuleMasterData),
        new(12, MasterData.Approve, BuildName(ActionApprove, ModuleMasterData), ModuleMasterData),
        new(13, CustodyIncrease.View, BuildName(ActionView, ModuleCustodyIncrease), ModuleCustodyIncrease),
        new(14, CustodyIncrease.Create, BuildName(ActionCreate, ModuleCustodyIncrease), ModuleCustodyIncrease),
        new(15, CustodyIncrease.Update, BuildName(ActionUpdate, ModuleCustodyIncrease), ModuleCustodyIncrease),
        new(16, CustodyIncrease.Cancel, BuildName(ActionCancel, ModuleCustodyIncrease), ModuleCustodyIncrease),
        new(17, CustodyIncrease.Print, BuildName(ActionPrint, ModuleCustodyIncrease), ModuleCustodyIncrease),
        new(18, CustodyIncrease.Approve, BuildName(ActionApprove, ModuleCustodyIncrease), ModuleCustodyIncrease),
        new(19, CustodyDecrease.View, BuildName(ActionView, ModuleCustodyDecrease), ModuleCustodyDecrease),
        new(20, CustodyDecrease.Create, BuildName(ActionCreate, ModuleCustodyDecrease), ModuleCustodyDecrease),
        new(21, CustodyDecrease.Update, BuildName(ActionUpdate, ModuleCustodyDecrease), ModuleCustodyDecrease),
        new(22, CustodyDecrease.Cancel, BuildName(ActionCancel, ModuleCustodyDecrease), ModuleCustodyDecrease),
        new(23, CustodyDecrease.Print, BuildName(ActionPrint, ModuleCustodyDecrease), ModuleCustodyDecrease),
        new(24, CustodyDecrease.Approve, BuildName(ActionApprove, ModuleCustodyDecrease), ModuleCustodyDecrease),
        new(25, CustodyReporting.View, BuildName(ActionView, ModuleCustodyReporting), ModuleCustodyReporting),
        new(26, CustodyReporting.Create, BuildName(ActionCreate, ModuleCustodyReporting), ModuleCustodyReporting),
        new(27, CustodyReporting.Update, BuildName(ActionUpdate, ModuleCustodyReporting), ModuleCustodyReporting),
        new(28, CustodyReporting.Cancel, BuildName(ActionCancel, ModuleCustodyReporting), ModuleCustodyReporting),
        new(29, CustodyReporting.Print, BuildName(ActionPrint, ModuleCustodyReporting), ModuleCustodyReporting),
        new(30, CustodyReporting.Approve, BuildName(ActionApprove, ModuleCustodyReporting), ModuleCustodyReporting),
        new(31, GoodsReceipt.View, BuildName(ActionView, ModuleGoodsReceipt), ModuleGoodsReceipt),
        new(32, GoodsReceipt.Create, BuildName(ActionCreate, ModuleGoodsReceipt), ModuleGoodsReceipt),
        new(33, GoodsReceipt.Update, BuildName(ActionUpdate, ModuleGoodsReceipt), ModuleGoodsReceipt),
        new(34, GoodsReceipt.Cancel, BuildName(ActionCancel, ModuleGoodsReceipt), ModuleGoodsReceipt),
        new(35, GoodsReceipt.Print, BuildName(ActionPrint, ModuleGoodsReceipt), ModuleGoodsReceipt),
        new(36, GoodsReceipt.Approve, BuildName(ActionApprove, ModuleGoodsReceipt), ModuleGoodsReceipt),
        new(37, Sales.View, BuildName(ActionView, ModuleSales), ModuleSales),
        new(38, Sales.Create, BuildName(ActionCreate, ModuleSales), ModuleSales),
        new(39, Sales.Update, BuildName(ActionUpdate, ModuleSales), ModuleSales),
        new(40, Sales.Cancel, BuildName(ActionCancel, ModuleSales), ModuleSales),
        new(41, Sales.Print, BuildName(ActionPrint, ModuleSales), ModuleSales),
        new(42, Sales.Approve, BuildName(ActionApprove, ModuleSales), ModuleSales),
        new(43, InventoryReporting.View, BuildName(ActionView, ModuleInventoryReporting), ModuleInventoryReporting),
        new(44, InventoryReporting.Create, BuildName(ActionCreate, ModuleInventoryReporting), ModuleInventoryReporting),
        new(45, InventoryReporting.Update, BuildName(ActionUpdate, ModuleInventoryReporting), ModuleInventoryReporting),
        new(46, InventoryReporting.Cancel, BuildName(ActionCancel, ModuleInventoryReporting), ModuleInventoryReporting),
        new(47, InventoryReporting.Print, BuildName(ActionPrint, ModuleInventoryReporting), ModuleInventoryReporting),
        new(48, InventoryReporting.Approve, BuildName(ActionApprove, ModuleInventoryReporting), ModuleInventoryReporting),
    ];

    /// <summary>The standard permissions of one module, in catalogue order.</summary>
    public static IReadOnlyList<string> ForModule(string module, bool includeApprove = true) =>
        All
            .Where(p => p.Module == module && (includeApprove || p.Code != $"{module}.{ActionApprove}"))
            .OrderBy(p => p.Id)
            .Select(p => p.Code)
            .ToList();

    /// <summary>True for a permission outside the module × action grid (the "Quyền đặc biệt" list).</summary>
    public static bool IsSpecial(PermissionDefinition permission) =>
        !Modules.Any(m => m.Code == permission.Module)
        || !Actions.Any(a => permission.Code == $"{permission.Module}.{a}");

    private static string BuildName(string action, string module) =>
        $"{action} — {Modules.Single(m => m.Code == module).Name}";
}
