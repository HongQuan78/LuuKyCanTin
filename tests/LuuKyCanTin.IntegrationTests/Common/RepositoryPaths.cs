namespace LuuKyCanTin.IntegrationTests.Common;

internal static class RepositoryPaths
{
    public static string Root { get; } = FindRoot();

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LuuKyCanTin.slnx")))
                return directory.FullName;
        }

        throw new InvalidOperationException("Could not find LuuKyCanTin.slnx above " + AppContext.BaseDirectory);
    }
}
