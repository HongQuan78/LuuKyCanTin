namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// How strictly a transaction isolates its reads from other workstations. Normal postings keep the provider
/// default; the last-administrator guard asks for <see cref="TuanTu"/> so two administrators demoting each
/// other at the same moment cannot both succeed.
/// </summary>
public enum MucDoCoLapGiaoDich : byte
{
    MacDinh = 1,
    TuanTu = 2,
}
