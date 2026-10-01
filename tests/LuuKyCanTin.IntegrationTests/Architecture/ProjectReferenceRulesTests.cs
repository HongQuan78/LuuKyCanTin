using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Architecture;

// Proves the rule checker actually detects violations, so the architecture test can't silently pass.
public class ProjectReferenceRulesTests
{
    private const string DomainReference =
        """<ProjectReference Include="..\LuuKyCanTin.Domain\LuuKyCanTin.Domain.csproj" />""";

    private static string Csproj(string items, string properties = "") => $"""
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
    public void Application_WithForbiddenPackage_IsReported(string package)
    {
        var xml = Csproj(DomainReference + $"""<PackageReference Include="{package}" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Application, xml)
            .ShouldContain(v => v.Contains(package));
    }

    [Fact]
    public void Application_WithWindowsForms_IsReported()
    {
        var xml = Csproj(DomainReference, "<UseWindowsForms>true</UseWindowsForms>");

        ProjectReferenceRules.Check(ProjectReferenceRules.Application, xml).ShouldNotBeEmpty();
    }

    [Fact]
    public void Application_ReferencingInfrastructure_IsReported()
    {
        var xml = Csproj(DomainReference
            + """<ProjectReference Include="..\LuuKyCanTin.Infrastructure\LuuKyCanTin.Infrastructure.csproj" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Application, xml)
            .ShouldContain(v => v.Contains(ProjectReferenceRules.Infrastructure));
    }

    [Fact]
    public void Application_WithAllowedPackage_Passes()
    {
        var xml = Csproj(DomainReference + """<PackageReference Include="FluentValidation" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Application, xml).ShouldBeEmpty();
    }

    [Fact]
    public void Domain_WithAnyPackage_IsReported()
    {
        var xml = Csproj("""<PackageReference Include="Newtonsoft.Json" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Domain, xml).ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("""<PackageReference Include="Some.Analyzer" PrivateAssets="all" />""")]
    [InlineData("""<PackageReference Include="Some.Analyzer"><PrivateAssets>all</PrivateAssets></PackageReference>""")]
    public void Domain_WithAnalyzerOnlyPackage_Passes(string package)
    {
        ProjectReferenceRules.Check(ProjectReferenceRules.Domain, Csproj(package)).ShouldBeEmpty();
    }

    [Fact]
    public void Infrastructure_MissingDomainReference_IsReported()
    {
        var xml = Csproj("""<ProjectReference Include="..\LuuKyCanTin.Application\LuuKyCanTin.Application.csproj" />""");

        ProjectReferenceRules.Check(ProjectReferenceRules.Infrastructure, xml)
            .ShouldContain(v => v.Contains(ProjectReferenceRules.Domain));
    }

    [Fact]
    public void UnknownProject_IsReported()
    {
        ProjectReferenceRules.Check("LuuKyCanTin.Something", Csproj("")).ShouldNotBeEmpty();
    }
}
