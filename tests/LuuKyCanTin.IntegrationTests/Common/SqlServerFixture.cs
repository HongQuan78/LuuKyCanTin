namespace LuuKyCanTin.IntegrationTests.Common;

/// <summary>
/// One database migrated with every migration, shared by the tests of <see cref="SqlServerCollection"/>.
/// Tests that change the schema or the migration history use their own <see cref="TestDatabase"/> instead.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    public TestDatabase Database { get; } = new();

    // xUnit builds collection fixtures even when every test in the collection is skipped.
    public Task InitializeAsync() => SqlServerFactAttribute.ShouldRun ? Database.MigrateAsync() : Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (SqlServerFactAttribute.ShouldRun)
            await Database.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
