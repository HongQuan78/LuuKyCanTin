using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

// Pure checks of the comparer; EnumCheckConstraintTests runs it against definitions read back from SQL Server.
public class EnumCheckVerifierTests
{
    private static readonly EnumColumn TrangThai = new("dbo", "MauChungTu", "TrangThai", typeof(MauTrangThai));

    private static DeployedCheck Check(string definition) => new("dbo", "MauChungTu", definition);

    [Theory]
    [InlineData("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))")]
    [InlineData("([TrangThai] IN (1, 2, 3))")]
    [InlineData("([TrangThai]=(3) OR [TrangThai]=(1) OR [TrangThai]=(2))")]
    public void MatchingConstraint_HasNoProblems(string definition)
    {
        EnumCheckVerifier.FindProblems([TrangThai], [Check(definition)]).ShouldBeEmpty();
    }

    [Fact]
    public void EnumValueMissingFromConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.FindProblems([TrangThai], [Check("([TrangThai]=(1) OR [TrangThai]=(2))")]);

        problems.ShouldHaveSingleItem().ShouldContain("enum value DaHuy=3 is not allowed by the CHECK constraint");
    }

    [Fact]
    public void AllowedValueMissingFromEnum_IsReported()
    {
        var problems = EnumCheckVerifier.FindProblems(
            [TrangThai], [Check("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3) OR [TrangThai]=(4))")]);

        problems.ShouldHaveSingleItem().ShouldContain("CHECK allows 4, which MauTrangThai does not define");
    }

    [Fact]
    public void ColumnWithoutConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.FindProblems([TrangThai], [Check("([SoTien]>=(0))")]);

        problems.ShouldHaveSingleItem().ShouldContain("no CHECK constraint");
    }

    [Fact]
    public void ConstraintOnAnotherTable_DoesNotCount()
    {
        var otherTable = new DeployedCheck("dbo", "KhacBang", "([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))");

        EnumCheckVerifier.FindProblems([TrangThai], [otherTable]).ShouldHaveSingleItem().ShouldContain("no CHECK constraint");
    }

    [Fact]
    public void ValuesOfAColumnWithLongerName_AreNotMixedIn()
    {
        DeployedCheck[] checks =
        [
            Check("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))"),
            Check("([TrangThaiTruoc]=(1) OR [TrangThaiTruoc]=(9))"),
        ];

        EnumCheckVerifier.FindProblems([TrangThai], checks).ShouldBeEmpty();
    }

    [Fact]
    public void SeveralConstraintsOnOneColumn_AllMustAllowTheValue()
    {
        DeployedCheck[] checks =
        [
            Check("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))"),
            Check("([TrangThai]=(1) OR [TrangThai]=(2))"),
        ];

        EnumCheckVerifier.FindProblems([TrangThai], checks).ShouldHaveSingleItem().ShouldContain("DaHuy=3");
    }

    private static readonly EnumColumn TrangThaiTheoTen = TrangThai with { StoredAsName = true };

    [Theory]
    [InlineData("([TrangThai]='Nhap' OR [TrangThai]='DaGhiSo' OR [TrangThai]='DaHuy')")]
    [InlineData("([TrangThai]=N'Nhap' OR [TrangThai]=N'DaGhiSo' OR [TrangThai]=N'DaHuy')")]
    [InlineData("([TrangThai] IN ('Nhap', 'DaGhiSo', 'DaHuy'))")]
    public void NameStoredColumn_MatchingConstraint_HasNoProblems(string definition)
    {
        EnumCheckVerifier.FindProblems([TrangThaiTheoTen], [Check(definition)]).ShouldBeEmpty();
    }

    [Fact]
    public void NameStoredColumn_NameMissingFromConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.FindProblems([TrangThaiTheoTen], [Check("([TrangThai]='Nhap' OR [TrangThai]='DaGhiSo')")]);

        problems.ShouldHaveSingleItem().ShouldContain("enum value DaHuy is not allowed by the CHECK constraint");
    }

    [Fact]
    public void NameStoredColumn_ExtraNameInConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.FindProblems(
            [TrangThaiTheoTen], [Check("([TrangThai]='Nhap' OR [TrangThai]='DaGhiSo' OR [TrangThai]='DaHuy' OR [TrangThai]='Moi')")]);

        problems.ShouldHaveSingleItem().ShouldContain("CHECK allows 'Moi', which MauTrangThai does not define");
    }

    [Fact]
    public void NameStoredColumn_NumericConstraint_IsReported()
    {
        // A tinyint-style CHECK on a text column means the conversion and the constraint disagree.
        EnumCheckVerifier.FindProblems([TrangThaiTheoTen], [Check("([TrangThai]=(1) OR [TrangThai]=(2) OR [TrangThai]=(3))")])
            .ShouldNotBeEmpty();
    }
}
