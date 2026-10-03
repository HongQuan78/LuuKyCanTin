namespace LuuKyCanTin.IntegrationTests.Common;

internal static class RepositoryPaths
{
    public static string Root { get; } = TimThuMucGoc();

    private static string TimThuMucGoc()
    {
        for (var thuMuc = new DirectoryInfo(AppContext.BaseDirectory); thuMuc is not null; thuMuc = thuMuc.Parent)
        {
            if (File.Exists(Path.Combine(thuMuc.FullName, "LuuKyCanTin.slnx")))
                return thuMuc.FullName;
        }

        throw new InvalidOperationException("Could not find LuuKyCanTin.slnx above " + AppContext.BaseDirectory);
    }
}
