using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

// Pure checks of the comparer; EnumCheckConstraintTests runs it against definitions read back from SQL Server.
public sealed class EnumCheckVerifierTests
{
    private static readonly EnumColumn Status = new("dbo", "SampleVoucher", "Status", typeof(SampleStatus));

    private static DeployedCheck CreateCheck(string definition) => new("dbo", "SampleVoucher", definition);

    [Theory]
    [InlineData("([Status]=(1) OR [Status]=(2) OR [Status]=(3))")]
    [InlineData("([Status] IN (1, 2, 3))")]
    [InlineData("([Status]=(3) OR [Status]=(1) OR [Status]=(2))")]
    public void Verify_MatchingConstraint_HasNoProblems(string definition)
    {
        EnumCheckVerifier.Verify([Status], [CreateCheck(definition)]).ShouldBeEmpty();
    }

    [Fact]
    public void Verify_EnumValueMissingFromConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.Verify([Status], [CreateCheck("([Status]=(1) OR [Status]=(2))")]);

        problems.ShouldHaveSingleItem().ShouldContain("enum value Cancelled=3 is not allowed by the CHECK constraint");
    }

    [Fact]
    public void Verify_AllowedValueMissingFromEnum_IsReported()
    {
        var problems = EnumCheckVerifier.Verify(
            [Status], [CreateCheck("([Status]=(1) OR [Status]=(2) OR [Status]=(3) OR [Status]=(4))")]);

        problems.ShouldHaveSingleItem().ShouldContain("CHECK allows 4, which SampleStatus does not define");
    }

    [Fact]
    public void Verify_ColumnWithoutConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.Verify([Status], [CreateCheck("([Amount]>=(0))")]);

        problems.ShouldHaveSingleItem().ShouldContain("no CHECK constraint");
    }

    [Fact]
    public void Verify_ConstraintOnAnotherTable_DoesNotCount()
    {
        var otherTable = new DeployedCheck("dbo", "KhacBang", "([Status]=(1) OR [Status]=(2) OR [Status]=(3))");

        EnumCheckVerifier.Verify([Status], [otherTable]).ShouldHaveSingleItem().ShouldContain("no CHECK constraint");
    }

    [Fact]
    public void Verify_ColumnWithLongerName_ValuesAreNotMixedIn()
    {
        DeployedCheck[] checks =
        [
            CreateCheck("([Status]=(1) OR [Status]=(2) OR [Status]=(3))"),
            CreateCheck("([PreviousStatus]=(1) OR [PreviousStatus]=(9))"),
        ];

        EnumCheckVerifier.Verify([Status], checks).ShouldBeEmpty();
    }

    [Fact]
    public void Verify_SeveralConstraintsOnOneColumn_RequiresAllToAllowTheValue()
    {
        DeployedCheck[] checks =
        [
            CreateCheck("([Status]=(1) OR [Status]=(2) OR [Status]=(3))"),
            CreateCheck("([Status]=(1) OR [Status]=(2))"),
        ];

        EnumCheckVerifier.Verify([Status], checks).ShouldHaveSingleItem().ShouldContain("Cancelled=3");
    }

    private static readonly EnumColumn StatusAsText = Status with { IsStoredAsText = true };

    [Theory]
    [InlineData("([Status]='Draft' OR [Status]='Posted' OR [Status]='Cancelled')")]
    [InlineData("([Status]=N'Draft' OR [Status]=N'Posted' OR [Status]=N'Cancelled')")]
    [InlineData("([Status] IN ('Draft', 'Posted', 'Cancelled'))")]
    public void Verify_TextStoredMatchingConstraint_HasNoProblems(string definition)
    {
        EnumCheckVerifier.Verify([StatusAsText], [CreateCheck(definition)]).ShouldBeEmpty();
    }

    [Fact]
    public void Verify_TextStoredValueMissingFromConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.Verify([StatusAsText], [CreateCheck("([Status]='Draft' OR [Status]='Posted')")]);

        problems.ShouldHaveSingleItem().ShouldContain("enum value Cancelled is not allowed by the CHECK constraint");
    }

    [Fact]
    public void Verify_TextStoredExtraValueInConstraint_IsReported()
    {
        var problems = EnumCheckVerifier.Verify(
            [StatusAsText], [CreateCheck("([Status]='Draft' OR [Status]='Posted' OR [Status]='Cancelled' OR [Status]='New')")]);

        problems.ShouldHaveSingleItem().ShouldContain("CHECK allows 'New', which SampleStatus does not define");
    }

    [Fact]
    public void Verify_TextStoredNumericConstraint_IsReported()
    {
        // A tinyint-style CHECK on a text column means the conversion and the constraint disagree.
        EnumCheckVerifier.Verify([StatusAsText], [CreateCheck("([Status]=(1) OR [Status]=(2) OR [Status]=(3))")])
            .ShouldNotBeEmpty();
    }

    [Fact]
    public void Verify_TextStoredAsCodes_ChecksTheCodesNotTheNames()
    {
        var statusAsCode = StatusAsText with { ToStoredText = value => ((byte)(SampleStatus)value).ToString("X2") };

        EnumCheckVerifier.Verify([statusAsCode], [CreateCheck("([Status] IN ('01', '02', '03'))")]).ShouldBeEmpty();
        EnumCheckVerifier.Verify([statusAsCode], [CreateCheck("([Status] IN ('Draft', 'Posted', 'Cancelled'))")])
            .ShouldNotBeEmpty();
    }
}
