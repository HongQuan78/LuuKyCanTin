namespace LuuKyCanTin.IntegrationTests.Common;

/// <summary>
/// One database migrated with every migration, shared by the tests of <see cref="LocalDbCollection"/>.
/// Tests that change the schema or the migration history use their own <see cref="TestDatabase"/> instead.
/// </summary>
public sealed class LocalDbFixture : IAsyncLifetime
{
    public TestDatabase Database { get; } = new();

    // xUnit builds collection fixtures even when every test in the collection is skipped.
    public Task InitializeAsync() => LocalDbFactAttribute.ShouldRun ? Database.MigrateAsync() : Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (LocalDbFactAttribute.ShouldRun)
            await Database.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class LocalDbCollection : ICollectionFixture<LocalDbFixture>
{
    public const string Name = "LocalDb";
}
