using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SP03Search.Data;

namespace SP03Search.Search;

internal sealed record SearchOutcome(IReadOnlyList<SearchResult> Rows, string Pattern);

internal sealed partial class SearchService
{
    public const int Top = 50;
    public const int MinimumNameLength = 2;
    public const int MinimumCodeLength = 1;

    private readonly SpikeDbContextFactory _factory;

    public SearchService(SpikeDbContextFactory factory) => _factory = factory;

    /// <summary>Code-like means 0-6 letters followed by a digit ("dt0001", "DT1", "1").</summary>
    public static bool LooksLikeCode(string term) => CodePattern().IsMatch(term);

    public async Task<SearchOutcome> SearchAsync(string rawTerm, SearchStrategy strategy, CancellationToken ct)
    {
        // Option (a): the stored column is NFC + diacritics stripped + đ→d, so fold the term the
        // same way. Both "nguyen van a" and "Nguyễn Văn A" then hit HoTenKhongDau identically.
        var term = VietnameseText.RemoveDiacritics(VietnameseText.NormalizeForSearch(rawTerm));
        if (strategy == SearchStrategy.Auto)
        {
            return LooksLikeCode(term)
                ? await SearchCodeAsync(term, ct)
                : await SearchNameCascadeAsync(term, ct);
        }

        return strategy switch
        {
            SearchStrategy.MaSoPrefix => await SearchCodeAsync(term, ct),
            SearchStrategy.TenKhongDauPrefix => await SearchNameAsync(term, SearchStrategy.TenKhongDauPrefix, ct),
            SearchStrategy.TenKhongDauTuBatKy => await SearchNameAsync(term, SearchStrategy.TenKhongDauTuBatKy, ct),
            SearchStrategy.TenKhongDauChua => await SearchNameAsync(term, SearchStrategy.TenKhongDauChua, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy)),
        };
    }

    /// <summary>
    /// Returns the code-search SQL (with its parameter declaration) so the benchmark can record
    /// whether EF sends nvarchar (implicit conversion scan) or varchar (seek) for MaSo.
    /// </summary>
    public string ExplainCodeSearch(string rawTerm)
    {
        var term = VietnameseText.RemoveDiacritics(VietnameseText.NormalizeForSearch(rawTerm));
        var pattern = VietnameseText.EscapeLike(term) + "%";
        using var context = _factory.CreateDbContext();
        return context.DoiTuongSpike
            .AsNoTracking()
            .Where(x => EF.Functions.Like(x.MaSo, pattern, "\\"))
            .OrderBy(x => x.HoTen)
            .ThenBy(x => x.MaSo)
            .Take(Top)
            .Select(x => new SearchResult(x.Id, x.MaSo, x.HoTen, x.NamSinh, x.BuongGiam))
            .ToQueryString();
    }

    private async Task<SearchOutcome> SearchNameCascadeAsync(string term, CancellationToken ct)
    {
        if (term.Length < MinimumNameLength)
        {
            return new SearchOutcome([], "min-length (names from 2 characters)");
        }

        var prefix = await SearchNameAsync(term, SearchStrategy.TenKhongDauPrefix, ct);
        if (prefix.Rows.Count > 0)
        {
            return prefix;
        }

        var wordStart = await SearchNameAsync(term, SearchStrategy.TenKhongDauTuBatKy, ct);
        return wordStart.Rows.Count > 0
            ? wordStart
            : await SearchNameAsync(term, SearchStrategy.TenKhongDauChua, ct);
    }

    private async Task<SearchOutcome> SearchCodeAsync(string term, CancellationToken ct)
    {
        if (term.Length < MinimumCodeLength)
        {
            return new SearchOutcome([], "min-length (code from 1 character)");
        }

        var pattern = VietnameseText.EscapeLike(term) + "%";
        using var context = _factory.CreateDbContext();
        var rows = await context.DoiTuongSpike
            .AsNoTracking()
            .Where(x => EF.Functions.Like(x.MaSo, pattern, "\\"))
            .OrderBy(x => x.HoTen)
            .ThenBy(x => x.MaSo)
            .Take(Top)
            .Select(x => new SearchResult(x.Id, x.MaSo, x.HoTen, x.NamSinh, x.BuongGiam))
            .ToListAsync(ct);
        return new SearchOutcome(rows, "MaSo prefix (seek on UQ_MaSo when narrow)");
    }

    private async Task<SearchOutcome> SearchNameAsync(string term, SearchStrategy strategy, CancellationToken ct)
    {
        if (term.Length < MinimumNameLength)
        {
            return new SearchOutcome([], "min-length (names from 2 characters)");
        }

        var escaped = VietnameseText.EscapeLike(term);
        using var context = _factory.CreateDbContext();
        var query = context.DoiTuongSpike.AsNoTracking();
        string pattern;

        switch (strategy)
        {
            case SearchStrategy.TenKhongDauPrefix:
                pattern = escaped + "%";
                query = query.Where(x => EF.Functions.Like(x.HoTenKhongDau!, pattern, "\\"));
                break;
            case SearchStrategy.TenKhongDauTuBatKy:
                pattern = escaped + "%";
                query = query.Where(x =>
                    EF.Functions.Like(x.HoTenKhongDau!, pattern, "\\") ||
                    EF.Functions.Like(x.HoTenKhongDau!, "% " + pattern, "\\"));
                break;
            case SearchStrategy.TenKhongDauChua:
                pattern = "%" + escaped + "%";
                query = query.Where(x => EF.Functions.Like(x.HoTenKhongDau!, pattern, "\\"));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy));
        }

        var rows = await query
            .OrderBy(x => x.HoTen)
            .ThenBy(x => x.MaSo)
            .Take(Top)
            .Select(x => new SearchResult(x.Id, x.MaSo, x.HoTen, x.NamSinh, x.BuongGiam))
            .ToListAsync(ct);
        return new SearchOutcome(rows, strategy.ToString());
    }

    [GeneratedRegex("^[A-Za-z]{0,6}\\d", RegexOptions.CultureInvariant)]
    private static partial Regex CodePattern();
}
