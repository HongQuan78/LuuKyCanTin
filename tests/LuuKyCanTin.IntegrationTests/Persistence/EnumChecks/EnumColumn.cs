namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

/// <summary>An enum-typed column of the EF model; <paramref name="StoredAsName"/> when it holds the enum's names as text.</summary>
public sealed record EnumColumn(string Schema, string Table, string Column, Type EnumType, bool StoredAsName = false);
