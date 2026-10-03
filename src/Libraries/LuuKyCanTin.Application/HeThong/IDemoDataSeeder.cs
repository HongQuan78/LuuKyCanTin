namespace LuuKyCanTin.Application.HeThong;

/// <summary>Fills a database with demo data. Reference data never goes here: it belongs in migrations.</summary>
public interface IDemoDataSeeder
{
    /// <returns>The policy decision; data is written only when it is <see cref="DemoSeedDecision.Allowed"/>.</returns>
    Task<DemoSeedDecision> NapDuLieuMauAsync(
        bool laMoiTruongPhatTrien, string? tenCoSoDuLieuXacNhan, CancellationToken ct = default);
}
