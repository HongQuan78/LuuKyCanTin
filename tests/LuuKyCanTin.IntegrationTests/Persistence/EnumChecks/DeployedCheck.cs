namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

/// <summary>A row of <c>sys.check_constraints</c>, as deployed.</summary>
public sealed record DeployedCheck(string Schema, string Table, string Definition);
