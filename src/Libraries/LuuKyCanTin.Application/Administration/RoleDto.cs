namespace LuuKyCanTin.Application.Administration;

/// <param name="RowVer">Sent back when saving, so two administrators editing the same role conflict cleanly.</param>
public sealed record RoleDto(int Id, string Code, string Name, byte[] RowVer);
