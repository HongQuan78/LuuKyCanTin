namespace LuuKyCanTin.Application.HeThong;

/// <summary>
/// Demo data must never reach a production database by accident. Outside Development the admin has to
/// confirm by typing the name of the database that is about to be filled.
/// </summary>
public static class DemoSeedPolicy
{
    public static DemoSeedDecision KiemTra(bool laMoiTruongPhatTrien, string? tenCoSoDuLieuXacNhan, string tenCoSoDuLieuDich)
    {
        if (laMoiTruongPhatTrien)
            return DemoSeedDecision.Allowed;
        if (tenCoSoDuLieuXacNhan is null)
            return DemoSeedDecision.NotDevelopment;
        // A bare --force confirms nothing, even when the connection string names no database (the target is then empty too).
        if (string.IsNullOrWhiteSpace(tenCoSoDuLieuXacNhan))
            return DemoSeedDecision.DatabaseNameMismatch;

        return string.Equals(tenCoSoDuLieuXacNhan, tenCoSoDuLieuDich, StringComparison.OrdinalIgnoreCase)
            ? DemoSeedDecision.Allowed
            : DemoSeedDecision.DatabaseNameMismatch;
    }
}
