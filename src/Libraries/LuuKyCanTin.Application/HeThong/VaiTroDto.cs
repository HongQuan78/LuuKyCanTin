namespace LuuKyCanTin.Application.HeThong;

/// <param name="RowVer">Sent back when saving, so two administrators editing the same role conflict cleanly.</param>
public sealed record VaiTroDto(int Id, string Ma, string Ten, byte[] RowVer);
