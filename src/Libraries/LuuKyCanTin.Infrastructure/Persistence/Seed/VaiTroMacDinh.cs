using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.Infrastructure.Persistence.Seed;

/// <summary>
/// The 6 standard roles and their default permission sets (Story 2.3 AC 3). Permission codes come only from
/// <see cref="MaQuyen"/>, never from string literals here.
/// </summary>
internal static class VaiTroMacDinh
{
    public sealed record DinhNghiaVaiTro(int Id, string Ma, string Ten, IReadOnlyList<string> Quyen);

    public static readonly IReadOnlyList<DinhNghiaVaiTro> TatCa =
    [
        new(1, MaVaiTro.QuanTri, "Quản trị hệ thống",
        [
            .. MaQuyen.QuyenCuaModule(MaQuyen.ModuleHt),
            .. MaQuyen.QuyenCuaModule(MaQuyen.ModuleDm),
        ]),
        new(2, MaVaiTro.LuuKy, "Cán bộ theo dõi tiền lưu ký",
        [
            .. MaQuyen.QuyenCuaModule(MaQuyen.ModuleLkt, baoGomDuyet: false),
            .. MaQuyen.QuyenCuaModule(MaQuyen.ModuleLkc, baoGomDuyet: false),
            .. MaQuyen.QuyenCuaModule(MaQuyen.ModuleLkbc, baoGomDuyet: false),
            MaQuyen.DM.Xem,
        ]),
        new(3, MaVaiTro.CanTin, "Cán bộ căn tin / bán hàng",
        [
            .. MaQuyen.QuyenCuaModule(MaQuyen.ModuleNh),
            .. MaQuyen.QuyenCuaModule(MaQuyen.ModuleBh),
            .. MaQuyen.QuyenCuaModule(MaQuyen.ModuleHhbc),
            MaQuyen.LKBC.Xem,
        ]),
        new(4, MaVaiTro.QuanGiao, "Cán bộ quản giáo",
        [
            MaQuyen.LKBC.Xem,
        ]),
        new(5, MaVaiTro.LanhDao, "Chỉ huy phụ trách / Lãnh đạo đơn vị",
        [
            MaQuyen.LKT.Duyet,
            MaQuyen.LKC.Duyet,
            MaQuyen.NH.Duyet,
            MaQuyen.LKBC.Xem,
            MaQuyen.HHBC.Xem,
            MaQuyen.HT.Xem,
        ]),
        new(6, MaVaiTro.KeToan, "Kế toán đơn vị",
        [
            MaQuyen.LKBC.Xem,
            MaQuyen.HHBC.Xem,
        ]),
    ];
}
