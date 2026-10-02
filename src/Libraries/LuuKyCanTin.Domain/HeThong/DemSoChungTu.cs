namespace LuuKyCanTin.Domain.HeThong;

/// <summary>
/// One numbering counter per document type and year. Numbers are allocated with an atomic UPDATE under a
/// row lock, so two workstations can never draw the same number.
/// </summary>
public sealed class DemSoChungTu
{
    public string LoaiChungTu { get; set; } = "";

    public short Nam { get; set; }

    public string TienTo { get; set; } = "";

    public int SoHienTai { get; set; }
}
