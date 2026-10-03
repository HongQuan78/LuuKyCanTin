namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// One numbering counter per document type and year. Numbers are allocated with an atomic UPDATE under a
/// row lock, so two workstations can never draw the same number.
/// </summary>
public sealed class VoucherCounter
{
    public string VoucherTypeCode { get; set; } = "";

    public short Year { get; set; }

    public string Prefix { get; set; } = "";

    public int CurrentNumber { get; set; }
}
