using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Architecture;

// Proves the rule checker actually detects violations, so the architecture test can't silently pass.
public class ProjectReferenceRulesTests
{
    private const string DomainReference =
        """<ProjectReference Include="..\LuuKyCanTin.Domain\LuuKyCanTin.Domain.csproj" />""";

    private static string CreateCsproj(string items, string properties = "") => $"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            {properties}
          </PropertyGroup>
          <ItemGroup>
            {items}
          </ItemGroup>
        </Project>
        """;

    [Theory]
    [InlineData("QuestPDF")]
    [InlineData("ClosedXML")]
    [InlineData("Microsoft.EntityFrameworkCore.SqlServer")]
    [InlineData("Microsoft.EntityFrameworkCore.Sqlite")]
    public void Check_ApplicationWithForbiddenPackage_IsReported(string package)
    {
        var xml = CreateCsproj(DomainReference + $"""<PackageReference Include="{package}" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Application, xml)
            .ShouldContain(v => v.Contains(package));
    }

    [Fact]
    public void Check_ApplicationWithWindowsForms_IsReported()
    {
        var xml = CreateCsproj(DomainReference, "<UseWindowsForms>true</UseWindowsForms>");

        ProjectReferenceRules.Check(ProjectReferenceRules.Application, xml).ShouldNotBeEmpty();
    }

    [Fact]
    public void Check_ApplicationWithFrameworkReference_IsReported()
    {
        var xml = CreateCsproj(DomainReference + """<FrameworkReference Include="Microsoft.WindowsDesktop.App" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Application, xml)
            .ShouldContain(v => v.Contains("Microsoft.WindowsDesktop.App"));
    }

    [Fact]
    public void Check_ApplicationReferencingInfrastructure_IsReported()
    {
        var xml = CreateCsproj(DomainReference
            + """<ProjectReference Include="..\LuuKyCanTin.Infrastructure\LuuKyCanTin.Infrastructure.csproj" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Application, xml)
            .ShouldContain(v => v.Contains(ProjectReferenceRules.Infrastructure));
    }

    [Theory]
    [InlineData("FluentValidation")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    public void Check_ApplicationWithAllowedPackage_Passes(string package)
    {
        var xml = CreateCsproj(DomainReference + $"""<PackageReference Include="{package}" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Application, xml).ShouldBeEmpty();
    }

    [Fact]
    public void Check_DomainWithAnyPackage_IsReported()
    {
        var xml = CreateCsproj("""<PackageReference Include="Newtonsoft.Json" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Domain, xml).ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("""<PackageReference Include="Some.Analyzer" PrivateAssets="all" />""")]
    [InlineData("""<PackageReference Include="Some.Analyzer"><PrivateAssets>all</PrivateAssets></PackageReference>""")]
    public void Check_DomainWithAnalyzerOnlyPackage_Passes(string package)
    {
        ProjectReferenceRules.Check(ProjectReferenceRules.Domain, CreateCsproj(package)).ShouldBeEmpty();
    }

    [Fact]
    public void Check_InfrastructureMissingDomainReference_IsReported()
    {
        var xml = CreateCsproj("""<ProjectReference Include="..\LuuKyCanTin.Application\LuuKyCanTin.Application.csproj" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Infrastructure, xml)
            .ShouldContain(v => v.Contains(ProjectReferenceRules.Domain));
    }

    [Fact]
    public void Check_UnknownProject_IsReported()
    {
        ProjectReferenceRules.Check("LuuKyCanTin.Something", CreateCsproj("")).ShouldNotBeEmpty();
    }
}
