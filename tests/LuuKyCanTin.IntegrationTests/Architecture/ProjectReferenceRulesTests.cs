using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Architecture;

// Proves the rule checker actually detects violations, so the architecture test can't silently pass.
public class ProjectReferenceRulesTests
{
    private const string DomainReference =
        """<ProjectReference Include="..\LuuKyCanTin.Domain\LuuKyCanTin.Domain.csproj" />""";

    private static string TaoCsproj(string items, string properties = "") => $"""
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
    public void KiemTra_ApplicationWithForbiddenPackage_IsReported(string package)
    {
        var xml = TaoCsproj(DomainReference + $"""<PackageReference Include="{package}" />""");

        ProjectReferenceRules.KiemTra(ProjectReferenceRules.Application, xml)
            .ShouldContain(v => v.Contains(package));
    }

    [Fact]
    public void KiemTra_ApplicationWithWindowsForms_IsReported()
    {
        var xml = TaoCsproj(DomainReference, "<UseWindowsForms>true</UseWindowsForms>");

        ProjectReferenceRules.KiemTra(ProjectReferenceRules.Application, xml).ShouldNotBeEmpty();
    }

    [Fact]
    public void KiemTra_ApplicationWithFrameworkReference_IsReported()
    {
        var xml = TaoCsproj(DomainReference + """<FrameworkReference Include="Microsoft.WindowsDesktop.App" />""");

        ProjectReferenceRules.KiemTra(ProjectReferenceRules.Application, xml)
            .ShouldContain(v => v.Contains("Microsoft.WindowsDesktop.App"));
    }

    [Fact]
    public void KiemTra_ApplicationReferencingInfrastructure_IsReported()
    {
        var xml = TaoCsproj(DomainReference
            + """<ProjectReference Include="..\LuuKyCanTin.Infrastructure\LuuKyCanTin.Infrastructure.csproj" />""");

        ProjectReferenceRules.KiemTra(ProjectReferenceRules.Application, xml)
            .ShouldContain(v => v.Contains(ProjectReferenceRules.Infrastructure));
    }

    [Theory]
    [InlineData("FluentValidation")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    public void KiemTra_ApplicationWithAllowedPackage_Passes(string package)
    {
        var xml = TaoCsproj(DomainReference + $"""<PackageReference Include="{package}" />""");

        ProjectReferenceRules.KiemTra(ProjectReferenceRules.Application, xml).ShouldBeEmpty();
    }

    [Fact]
    public void KiemTra_DomainWithAnyPackage_IsReported()
    {
        var xml = TaoCsproj("""<PackageReference Include="Newtonsoft.Json" />""");

        ProjectReferenceRules.KiemTra(ProjectReferenceRules.Domain, xml).ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("""<PackageReference Include="Some.Analyzer" PrivateAssets="all" />""")]
    [InlineData("""<PackageReference Include="Some.Analyzer"><PrivateAssets>all</PrivateAssets></PackageReference>""")]
    public void KiemTra_DomainWithAnalyzerOnlyPackage_Passes(string package)
    {
        ProjectReferenceRules.KiemTra(ProjectReferenceRules.Domain, TaoCsproj(package)).ShouldBeEmpty();
    }

    [Fact]
    public void KiemTra_InfrastructureMissingDomainReference_IsReported()
    {
        var xml = TaoCsproj("""<ProjectReference Include="..\LuuKyCanTin.Application\LuuKyCanTin.Application.csproj" />""");

        ProjectReferenceRules.KiemTra(ProjectReferenceRules.Infrastructure, xml)
            .ShouldContain(v => v.Contains(ProjectReferenceRules.Domain));
    }

    [Fact]
    public void KiemTra_UnknownProject_IsReported()
    {
        ProjectReferenceRules.KiemTra("LuuKyCanTin.Something", TaoCsproj("")).ShouldNotBeEmpty();
    }
}
