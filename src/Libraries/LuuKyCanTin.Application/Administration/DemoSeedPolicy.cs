namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// Demo data must never reach a production database by accident. Outside Development the admin has to
/// confirm by typing the name of the database that is about to be filled.
/// </summary>
public static class DemoSeedPolicy
{
    public static DemoSeedDecision Decide(bool isDevelopment, string? confirmedDatabaseName, string targetDatabaseName)
    {
        if (isDevelopment)
            return DemoSeedDecision.Allowed;
        if (confirmedDatabaseName is null)
            return DemoSeedDecision.NotDevelopment;
        // A bare --force confirms nothing, even when the connection string names no database (the target is then empty too).
        if (string.IsNullOrWhiteSpace(confirmedDatabaseName))
            return DemoSeedDecision.DatabaseNameMismatch;

        return string.Equals(confirmedDatabaseName, targetDatabaseName, StringComparison.OrdinalIgnoreCase)
            ? DemoSeedDecision.Allowed
            : DemoSeedDecision.DatabaseNameMismatch;
    }
}
