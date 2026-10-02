namespace LuuKyCanTin.Domain.HeThong;

/// <summary>Assigns one role to one account. A join row, not audited; account administration logs it explicitly.</summary>
public sealed class NguoiDungVaiTro
{
    public int NguoiDungId { get; set; }

    public int VaiTroId { get; set; }
}
