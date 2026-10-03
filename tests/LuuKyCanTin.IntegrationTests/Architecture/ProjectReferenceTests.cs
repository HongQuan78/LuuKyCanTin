using System.Reflection;
using LuuKyCanTin.IntegrationTests.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Architecture;

public class ProjectReferenceTests
{
    private static readonly Dictionary<string, string> SourceProjects = Directory
        .EnumerateFiles(Path.Combine(RepositoryPaths.Root, "src"), "*.csproj", SearchOption.AllDirectories)
        .ToDictionary(p => Path.GetFileNameWithoutExtension(p), p => p);

    public static TheoryData<string> ProjectNames => [.. ProjectReferenceRules.KnownProjects];

    [Fact]
    public void Check_EverySourceProject_HasARule()
    {
        SourceProjects.Keys.ShouldBe(ProjectReferenceRules.KnownProjects, ignoreOrder: true);
    }

    [Theory]
    [MemberData(nameof(ProjectNames))]
    public void Check_SourceProject_FollowsTheDependencyRule(string projectName)
    {
        var violations = ProjectReferenceRules.Check(projectName, File.ReadAllText(SourceProjects[projectName]));

        violations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(ProjectReferenceRules.Domain)]
    [InlineData(ProjectReferenceRules.Application)]
    public void CompiledInnerLayer_DoesNotReferenceForbiddenAssemblies(string assemblyName)
    {
        var referenced = Assembly.Load(assemblyName).GetReferencedAssemblies().Select(a => a.Name).ToList();

        referenced.ShouldNotContain(name => ProjectReferenceRules.ForbiddenInInnerLayers.Contains(name));
        referenced.ShouldNotContain(ProjectReferenceRules.Infrastructure);
        referenced.ShouldNotContain(ProjectReferenceRules.WinForms);
    }

    [Fact]
    public void CompiledDomain_DoesNotReferenceOtherProjects()
    {
        Assembly.Load(ProjectReferenceRules.Domain).GetReferencedAssemblies()
            .ShouldNotContain(a => a.Name!.StartsWith("LuuKyCanTin.", StringComparison.Ordinal));
    }
}
