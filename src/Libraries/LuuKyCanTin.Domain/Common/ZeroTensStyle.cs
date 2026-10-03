namespace LuuKyCanTin.Domain.Common;

/// <summary>
/// How a zero tens digit is read before a units digit: 101 is "một trăm lẻ một" (<see cref="Southern"/>) or
/// "một trăm linh một" (<see cref="Northern"/>).
/// </summary>
public enum ZeroTensStyle : byte
{
    Southern = 1,
    Northern = 2,
}
