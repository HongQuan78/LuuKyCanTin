namespace LuuKyCanTin.Application.DanhMuc;

/// <summary>The minimal list behind the skeleton's detainee selector: managed detainees only.</summary>
public sealed class LayDoiTuongDangQuanLyQuery(IDoiTuongStore doiTuongStore)
{
    public async Task<IReadOnlyList<DoiTuongChon>> LayAsync(CancellationToken ct = default)
    {
        var danhSach = await doiTuongStore.LayDangQuanLyAsync(ct);
        return [.. danhSach.Select(d => new DoiTuongChon(d.Id, d.MaSo, d.HoTen))];
    }
}
