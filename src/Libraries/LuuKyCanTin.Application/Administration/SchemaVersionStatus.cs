namespace LuuKyCanTin.Application.Administration;

public enum SchemaVersionStatus : byte
{
    Matches,
    Mismatch,
    ConnectionFailed,
}
