namespace LuuKyCanTin.Application.Common;

/// <summary>
/// A unique index rejected the save. Not a <see cref="LoiNghiepVuException"/>: only the service knows which value was
/// duplicated, so it catches this and throws its own message.
/// </summary>
public sealed class TrungGiaTriDuyNhatException(Exception innerException)
    : Exception("A unique index rejected the save.", innerException);
