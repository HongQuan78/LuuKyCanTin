using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

// Pure checks of the comparer; EnumCheckConstraintTests runs it against definitions read back from SQL Server.
public sealed class EnumCheckVerifierTests
{
    private static readonly EnumColumn TrangThai = new("dbo", "MauChungTu", "TrangThai", typeof(MauTrangThai));

    private static DeployedCheck TaoCheck(string definition) => new("dbo", "MauChungTu", definition);

    [Theory]
    [InlineData("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))")]
    [InlineData("([TrangThai] IN (1, 2, 3))")]
    [InlineData("([TrangThai]=(3) OR [TrangThai]=(1) OR [TrangThai]=(2))")]
    public void KiemTra_MatchingConstraint_HasNoProblems(string definition)
    {
        EnumCheckVerifier.KiemTra([TrangThai], [TaoCheck(definition)]).ShouldBeEmpty();
    }

    [Fact]
    public void KiemTra_EnumValueMissingFromConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.KiemTra([TrangThai], [TaoCheck("([TrangThai]=(1) OR [TrangThai]=(2))")]);

        problems.ShouldHaveSingleItem().ShouldContain("enum value DaHuy=3 is not allowed by the CHECK constraint");
    }

    [Fact]
    public void KiemTra_AllowedValueMissingFromEnum_IsReported()
    {
        var problems = EnumCheckVerifier.KiemTra(
            [TrangThai], [TaoCheck("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3) OR [TrangThai]=(4))")]);

        problems.ShouldHaveSingleItem().ShouldContain("CHECK allows 4, which MauTrangThai does not define");
    }

    [Fact]
    public void KiemTra_ColumnWithoutConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.KiemTra([TrangThai], [TaoCheck("([SoTien]>=(0))")]);

        problems.ShouldHaveSingleItem().ShouldContain("no CHECK constraint");
    }

    [Fact]
    public void KiemTra_ConstraintOnAnotherTable_DoesNotCount()
    {
        var otherTable = new DeployedCheck("dbo", "KhacBang", "([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))");

        EnumCheckVerifier.KiemTra([TrangThai], [otherTable]).ShouldHaveSingleItem().ShouldContain("no CHECK constraint");
    }

    [Fact]
    public void KiemTra_ColumnWithLongerName_ValuesAreNotMixedIn()
    {
        DeployedCheck[] checks =
        [
            TaoCheck("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))"),
            TaoCheck("([TrangThaiTruoc]=(1) OR [TrangThaiTruoc]=(9))"),
        ];

        EnumCheckVerifier.KiemTra([TrangThai], checks).ShouldBeEmpty();
    }

    [Fact]
    public void KiemTra_SeveralConstraintsOnOneColumn_RequiresAllToAllowTheValue()
    {
        DeployedCheck[] checks =
        [
            TaoCheck("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))"),
            TaoCheck("([TrangThai]=(1) OR [TrangThai]=(2))"),
        ];

        EnumCheckVerifier.KiemTra([TrangThai], checks).ShouldHaveSingleItem().ShouldContain("DaHuy=3");
    }

    private static readonly EnumColumn TrangThaiTheoTen = TrangThai with { LaLuuTheoTen = true };

    [Theory]
    [InlineData("([TrangThai]='Nhap' OR [TrangThai]='DaGhiSo' OR [TrangThai]='DaHuy')")]
    [InlineData("([TrangThai]=N'Nhap' OR [TrangThai]=N'DaGhiSo' OR [TrangThai]=N'DaHuy')")]
    [InlineData("([TrangThai] IN ('Nhap', 'DaGhiSo', 'DaHuy'))")]
    public void KiemTra_NameStoredMatchingConstraint_HasNoProblems(string definition)
    {
        EnumCheckVerifier.KiemTra([TrangThaiTheoTen], [TaoCheck(definition)]).ShouldBeEmpty();
    }

    [Fact]
    public void KiemTra_NameStoredNameMissingFromConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.KiemTra([TrangThaiTheoTen], [TaoCheck("([TrangThai]='Nhap' OR [TrangThai]='DaGhiSo')")]);

        problems.ShouldHaveSingleItem().ShouldContain("enum value DaHuy is not allowed by the CHECK constraint");
    }

    [Fact]
    public void KiemTra_NameStoredExtraNameInConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.KiemTra(
            [TrangThaiTheoTen], [TaoCheck("([TrangThai]='Nhap' OR [TrangThai]='DaGhiSo' OR [TrangThai]='DaHuy' OR [TrangThai]='Moi')")]);

        problems.ShouldHaveSingleItem().ShouldContain("CHECK allows 'Moi', which MauTrangThai does not define");
    }

    [Fact]
    public void KiemTra_NameStoredNumericConstraint_IsReported()
    {
        // A tinyint-style CHECK on a text column means the conversion and the constraint disagree.
        EnumCheckVerifier.KiemTra([TrangThaiTheoTen], [TaoCheck("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))")])
            .ShouldNotBeEmpty();
    }
}
