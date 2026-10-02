using System.Reflection;
using System.Text.RegularExpressions;
using LuuKyCanTin.Application.HeThong;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

public class MaQuyenTests
{
    // The full module × action grid: 8 modules, 6 actions.
    private const int SoQuyenChuan = 48;

    [Fact]
    public void TatCa_HasThe48StandardEntries()
    {
        MaQuyen.TatCa.Count.ShouldBe(SoQuyenChuan);
        MaQuyen.CacModule.Count.ShouldBe(8);
        MaQuyen.CacHanhDong.Count.ShouldBe(6);
    }

    [Fact]
    public void TatCa_HasUniqueCodesAndIds()
    {
        MaQuyen.TatCa.Select(q => q.Ma).Distinct(StringComparer.Ordinal).Count().ShouldBe(SoQuyenChuan);
        MaQuyen.TatCa.Select(q => q.Id).Distinct().Count().ShouldBe(SoQuyenChuan);
        MaQuyen.TatCa.Select(q => q.Id).Order().ShouldBe(Enumerable.Range(1, SoQuyenChuan));
    }

    [Fact]
    public void TatCa_EveryCodeMatchesTheModuleActionPattern()
    {
        foreach (var quyen in MaQuyen.TatCa)
        {
            Regex.IsMatch(quyen.Ma, "^[A-Z-]+\\.[A-Za-z]+$").ShouldBeTrue(quyen.Ma);
            quyen.Ma.ShouldStartWith(quyen.Module + ".");
            MaQuyen.CacModule.ShouldContain(m => m.Ma == quyen.Module);
        }
    }

    [Fact]
    public void TatCa_CoversEveryModuleAndActionExactlyOnce()
    {
        foreach (var module in MaQuyen.CacModule)
        {
            var maCuaModule = MaQuyen.QuyenCuaModule(module.Ma);
            maCuaModule.Count.ShouldBe(MaQuyen.CacHanhDong.Count);
        }
    }

    [Fact]
    public void EveryConstantInTheNestedClasses_AppearsInTatCa()
    {
        var nested = typeof(MaQuyen).GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .Where(t => t is { IsAbstract: true, IsSealed: true })
            .ToList();
        nested.Count.ShouldBe(MaQuyen.CacModule.Count);

        var maTrongDanhMuc = MaQuyen.TatCa.Select(q => q.Ma).ToHashSet(StringComparer.Ordinal);
        foreach (var lopModule in nested)
        {
            var hangSo = lopModule
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!)
                .ToList();
            hangSo.Count.ShouldBe(MaQuyen.CacHanhDong.Count);
            foreach (var ma in hangSo)
                maTrongDanhMuc.ShouldContain(ma);
        }
    }

    [Fact]
    public void QuyenCuaModule_CanLeaveDuyetOut()
    {
        MaQuyen.QuyenCuaModule(MaQuyen.ModuleLkt).Count.ShouldBe(6);
        MaQuyen.QuyenCuaModule(MaQuyen.ModuleLkt, baoGomDuyet: false).ShouldNotContain(MaQuyen.LKT.Duyet);
        MaQuyen.QuyenCuaModule(MaQuyen.ModuleLkt, baoGomDuyet: false).Count.ShouldBe(5);
    }

    [Fact]
    public void LaQuyenDacBiet_IsTrueOnlyOutsideTheStandardGrid()
    {
        var xemHt = MaQuyen.TatCa.Single(q => q.Ma == MaQuyen.HT.Xem);
        MaQuyen.LaQuyenDacBiet(xemHt).ShouldBeFalse();

        // What a later story adds: a new action on an existing module, or a new module.
        MaQuyen.LaQuyenDacBiet(new MaQuyen.DinhNghiaQuyen(49, "HT.KhoaSo", "Khoá sổ — Hệ thống", "HT")).ShouldBeTrue();
        MaQuyen.LaQuyenDacBiet(new MaQuyen.DinhNghiaQuyen(50, "LK.LuiNgay", "Lùi ngày — Lưu ký", "LK")).ShouldBeTrue();
    }
}
