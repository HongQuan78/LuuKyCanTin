namespace LuuKyCanTin.Domain.Common;

/// <summary>How a zero tens digit is read before a units digit: 101 is "một trăm lẻ một" or "một trăm linh một".</summary>
public enum KieuDocLe : byte
{
    Le = 1,
    Linh = 2,
}
