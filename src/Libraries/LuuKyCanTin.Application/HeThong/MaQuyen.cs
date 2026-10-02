namespace LuuKyCanTin.Application.HeThong;

/// <summary>
/// The only place permission codes are spelled. A code is <c>&lt;Module&gt;.&lt;Action&gt;</c>, for example
/// <c>LK-C.Duyet</c>. Services reference <see cref="HT"/>, <see cref="LKC"/> and friends instead of literals.
/// </summary>
/// <remarks>
/// <see cref="TatCa"/> is the seed for the <c>Quyen</c> table and the allow-list for role editing. Its ids are
/// explicit and append-only: later stories add special permissions with the next free id, and the standard grid
/// keeps its numbering.
/// </remarks>
public static class MaQuyen
{
    public const string HanhDongXem = "Xem";
    public const string HanhDongThem = "Them";
    public const string HanhDongSua = "Sua";
    public const string HanhDongHuy = "Huy";
    public const string HanhDongIn = "In";
    public const string HanhDongDuyet = "Duyet";

    public const string ModuleHt = "HT";
    public const string ModuleDm = "DM";
    public const string ModuleLkt = "LK-T";
    public const string ModuleLkc = "LK-C";
    public const string ModuleLkbc = "LK-BC";
    public const string ModuleNh = "NH";
    public const string ModuleBh = "BH";
    public const string ModuleHhbc = "HH-BC";

    public sealed record DinhNghiaQuyen(int Id, string Ma, string Ten, string Module);

    public sealed record DinhNghiaModule(string Ma, string Ten);

    public static class HT
    {
        public const string Xem = ModuleHt + "." + HanhDongXem;
        public const string Them = ModuleHt + "." + HanhDongThem;
        public const string Sua = ModuleHt + "." + HanhDongSua;
        public const string Huy = ModuleHt + "." + HanhDongHuy;
        public const string In = ModuleHt + "." + HanhDongIn;
        public const string Duyet = ModuleHt + "." + HanhDongDuyet;
    }

    public static class DM
    {
        public const string Xem = ModuleDm + "." + HanhDongXem;
        public const string Them = ModuleDm + "." + HanhDongThem;
        public const string Sua = ModuleDm + "." + HanhDongSua;
        public const string Huy = ModuleDm + "." + HanhDongHuy;
        public const string In = ModuleDm + "." + HanhDongIn;
        public const string Duyet = ModuleDm + "." + HanhDongDuyet;
    }

    public static class LKT
    {
        public const string Xem = ModuleLkt + "." + HanhDongXem;
        public const string Them = ModuleLkt + "." + HanhDongThem;
        public const string Sua = ModuleLkt + "." + HanhDongSua;
        public const string Huy = ModuleLkt + "." + HanhDongHuy;
        public const string In = ModuleLkt + "." + HanhDongIn;
        public const string Duyet = ModuleLkt + "." + HanhDongDuyet;
    }

    public static class LKC
    {
        public const string Xem = ModuleLkc + "." + HanhDongXem;
        public const string Them = ModuleLkc + "." + HanhDongThem;
        public const string Sua = ModuleLkc + "." + HanhDongSua;
        public const string Huy = ModuleLkc + "." + HanhDongHuy;
        public const string In = ModuleLkc + "." + HanhDongIn;
        public const string Duyet = ModuleLkc + "." + HanhDongDuyet;
    }

    public static class LKBC
    {
        public const string Xem = ModuleLkbc + "." + HanhDongXem;
        public const string Them = ModuleLkbc + "." + HanhDongThem;
        public const string Sua = ModuleLkbc + "." + HanhDongSua;
        public const string Huy = ModuleLkbc + "." + HanhDongHuy;
        public const string In = ModuleLkbc + "." + HanhDongIn;
        public const string Duyet = ModuleLkbc + "." + HanhDongDuyet;
    }

    public static class NH
    {
        public const string Xem = ModuleNh + "." + HanhDongXem;
        public const string Them = ModuleNh + "." + HanhDongThem;
        public const string Sua = ModuleNh + "." + HanhDongSua;
        public const string Huy = ModuleNh + "." + HanhDongHuy;
        public const string In = ModuleNh + "." + HanhDongIn;
        public const string Duyet = ModuleNh + "." + HanhDongDuyet;
    }

    public static class BH
    {
        public const string Xem = ModuleBh + "." + HanhDongXem;
        public const string Them = ModuleBh + "." + HanhDongThem;
        public const string Sua = ModuleBh + "." + HanhDongSua;
        public const string Huy = ModuleBh + "." + HanhDongHuy;
        public const string In = ModuleBh + "." + HanhDongIn;
        public const string Duyet = ModuleBh + "." + HanhDongDuyet;
    }

    public static class HHBC
    {
        public const string Xem = ModuleHhbc + "." + HanhDongXem;
        public const string Them = ModuleHhbc + "." + HanhDongThem;
        public const string Sua = ModuleHhbc + "." + HanhDongSua;
        public const string Huy = ModuleHhbc + "." + HanhDongHuy;
        public const string In = ModuleHhbc + "." + HanhDongIn;
        public const string Duyet = ModuleHhbc + "." + HanhDongDuyet;
    }

    public static readonly IReadOnlyList<DinhNghiaModule> CacModule =
    [
        new(ModuleHt, "Hệ thống"),
        new(ModuleDm, "Danh mục"),
        new(ModuleLkt, "Tăng tiền lưu ký"),
        new(ModuleLkc, "Giảm tiền lưu ký"),
        new(ModuleLkbc, "Báo cáo lưu ký"),
        new(ModuleNh, "Nhập hàng"),
        new(ModuleBh, "Bán hàng"),
        new(ModuleHhbc, "Báo cáo hàng hóa"),
    ];

    public static readonly IReadOnlyList<string> CacHanhDong =
    [
        HanhDongXem,
        HanhDongThem,
        HanhDongSua,
        HanhDongHuy,
        HanhDongIn,
        HanhDongDuyet,
    ];

    public static readonly IReadOnlyList<DinhNghiaQuyen> TatCa =
    [
        new(1, HT.Xem, Ten(HanhDongXem, ModuleHt), ModuleHt),
        new(2, HT.Them, Ten(HanhDongThem, ModuleHt), ModuleHt),
        new(3, HT.Sua, Ten(HanhDongSua, ModuleHt), ModuleHt),
        new(4, HT.Huy, Ten(HanhDongHuy, ModuleHt), ModuleHt),
        new(5, HT.In, Ten(HanhDongIn, ModuleHt), ModuleHt),
        new(6, HT.Duyet, Ten(HanhDongDuyet, ModuleHt), ModuleHt),
        new(7, DM.Xem, Ten(HanhDongXem, ModuleDm), ModuleDm),
        new(8, DM.Them, Ten(HanhDongThem, ModuleDm), ModuleDm),
        new(9, DM.Sua, Ten(HanhDongSua, ModuleDm), ModuleDm),
        new(10, DM.Huy, Ten(HanhDongHuy, ModuleDm), ModuleDm),
        new(11, DM.In, Ten(HanhDongIn, ModuleDm), ModuleDm),
        new(12, DM.Duyet, Ten(HanhDongDuyet, ModuleDm), ModuleDm),
        new(13, LKT.Xem, Ten(HanhDongXem, ModuleLkt), ModuleLkt),
        new(14, LKT.Them, Ten(HanhDongThem, ModuleLkt), ModuleLkt),
        new(15, LKT.Sua, Ten(HanhDongSua, ModuleLkt), ModuleLkt),
        new(16, LKT.Huy, Ten(HanhDongHuy, ModuleLkt), ModuleLkt),
        new(17, LKT.In, Ten(HanhDongIn, ModuleLkt), ModuleLkt),
        new(18, LKT.Duyet, Ten(HanhDongDuyet, ModuleLkt), ModuleLkt),
        new(19, LKC.Xem, Ten(HanhDongXem, ModuleLkc), ModuleLkc),
        new(20, LKC.Them, Ten(HanhDongThem, ModuleLkc), ModuleLkc),
        new(21, LKC.Sua, Ten(HanhDongSua, ModuleLkc), ModuleLkc),
        new(22, LKC.Huy, Ten(HanhDongHuy, ModuleLkc), ModuleLkc),
        new(23, LKC.In, Ten(HanhDongIn, ModuleLkc), ModuleLkc),
        new(24, LKC.Duyet, Ten(HanhDongDuyet, ModuleLkc), ModuleLkc),
        new(25, LKBC.Xem, Ten(HanhDongXem, ModuleLkbc), ModuleLkbc),
        new(26, LKBC.Them, Ten(HanhDongThem, ModuleLkbc), ModuleLkbc),
        new(27, LKBC.Sua, Ten(HanhDongSua, ModuleLkbc), ModuleLkbc),
        new(28, LKBC.Huy, Ten(HanhDongHuy, ModuleLkbc), ModuleLkbc),
        new(29, LKBC.In, Ten(HanhDongIn, ModuleLkbc), ModuleLkbc),
        new(30, LKBC.Duyet, Ten(HanhDongDuyet, ModuleLkbc), ModuleLkbc),
        new(31, NH.Xem, Ten(HanhDongXem, ModuleNh), ModuleNh),
        new(32, NH.Them, Ten(HanhDongThem, ModuleNh), ModuleNh),
        new(33, NH.Sua, Ten(HanhDongSua, ModuleNh), ModuleNh),
        new(34, NH.Huy, Ten(HanhDongHuy, ModuleNh), ModuleNh),
        new(35, NH.In, Ten(HanhDongIn, ModuleNh), ModuleNh),
        new(36, NH.Duyet, Ten(HanhDongDuyet, ModuleNh), ModuleNh),
        new(37, BH.Xem, Ten(HanhDongXem, ModuleBh), ModuleBh),
        new(38, BH.Them, Ten(HanhDongThem, ModuleBh), ModuleBh),
        new(39, BH.Sua, Ten(HanhDongSua, ModuleBh), ModuleBh),
        new(40, BH.Huy, Ten(HanhDongHuy, ModuleBh), ModuleBh),
        new(41, BH.In, Ten(HanhDongIn, ModuleBh), ModuleBh),
        new(42, BH.Duyet, Ten(HanhDongDuyet, ModuleBh), ModuleBh),
        new(43, HHBC.Xem, Ten(HanhDongXem, ModuleHhbc), ModuleHhbc),
        new(44, HHBC.Them, Ten(HanhDongThem, ModuleHhbc), ModuleHhbc),
        new(45, HHBC.Sua, Ten(HanhDongSua, ModuleHhbc), ModuleHhbc),
        new(46, HHBC.Huy, Ten(HanhDongHuy, ModuleHhbc), ModuleHhbc),
        new(47, HHBC.In, Ten(HanhDongIn, ModuleHhbc), ModuleHhbc),
        new(48, HHBC.Duyet, Ten(HanhDongDuyet, ModuleHhbc), ModuleHhbc),
    ];

    /// <summary>The standard permissions of one module, in catalogue order.</summary>
    public static IReadOnlyList<string> QuyenCuaModule(string module, bool baoGomDuyet = true) =>
        TatCa
            .Where(q => q.Module == module && (baoGomDuyet || q.Ma != $"{module}.{HanhDongDuyet}"))
            .OrderBy(q => q.Id)
            .Select(q => q.Ma)
            .ToList();

    /// <summary>True for a permission outside the module × action grid (the "Quyền đặc biệt" list).</summary>
    public static bool LaQuyenDacBiet(DinhNghiaQuyen quyen) =>
        !CacModule.Any(m => m.Ma == quyen.Module)
        || !CacHanhDong.Any(h => quyen.Ma == $"{quyen.Module}.{h}");

    private static string Ten(string hanhDong, string module) =>
        $"{hanhDong} — {CacModule.Single(m => m.Ma == module).Ten}";
}
