namespace LuuKyCanTin.Application.MasterData;

/// <summary>The minimal list behind the skeleton's detainee selector: managed detainees only.</summary>
public sealed class InmatesInCustodyQuery(IInmateStore inmateStore)
{
    public async Task<IReadOnlyList<InmateOption>> GetAsync(CancellationToken ct = default)
    {
        var items = await inmateStore.GetInCustodyAsync(ct);
        return [.. items.Select(d => new InmateOption(d.Id, d.InmateCode, d.FullName))];
    }
}
