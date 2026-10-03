using System.Text.RegularExpressions;
using LuuKyCanTin.IntegrationTests.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Architecture;

/// <summary>
/// The cached <c>HasPermission</c> is UI-only. Application code must call <c>IPermissionChecker</c>, which reads
/// the database, so this scans the Application sources for any other caller.
/// </summary>
public class CachedPermissionUsageTests
{
    private const string AbstractionFile = "ICurrentUser.cs";

    // Word boundary, so a method-group use (`Func<string, bool> f = currentUser.HasPermission;`) is caught too.
    private static readonly Regex CachedCheck = new(@"HasPermission\b", RegexOptions.Compiled);

    [Fact]
    public void Application_HasPermission_IsOnlyDeclaredOnTheAbstraction()
    {
        var applicationDirectory = Path.Combine(RepositoryPaths.Root, "src", "Libraries", "LuuKyCanTin.Application");

        var filesCalling = Directory.EnumerateFiles(applicationDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(file => CachedCheck.IsMatch(File.ReadAllText(file)))
            .Select(Path.GetFileName)
            .ToList();

        filesCalling.ShouldBe([AbstractionFile]);
    }
}
