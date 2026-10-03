namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

/// <summary>An enum-typed column of the EF model; <paramref name="IsStoredAsText"/> when it holds the enum as text.</summary>
public sealed record EnumColumn(string Schema, string Table, string Column, Type EnumType, bool IsStoredAsText = false)
{
    /// <summary>The text a text column stores for an enum value; null means the value's name.</summary>
    public Func<object, string>? ToStoredText { get; init; }
}
