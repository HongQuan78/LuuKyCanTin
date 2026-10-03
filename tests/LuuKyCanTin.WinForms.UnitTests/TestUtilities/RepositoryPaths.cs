namespace LuuKyCanTin.WinForms.UnitTests.TestUtilities;

internal static class RepositoryPaths
{
    public static string Root { get; } = FindRoot();

    public static string WinFormsSource { get; } = Path.Combine(Root, "src", "Presentation", "LuuKyCanTin.WinForms");

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
