namespace LuuKyCanTin.Domain.Common;

/// <summary>The shared document state machine: Draft → Posted → Cancelled (with a reason).</summary>
public enum TrangThaiChungTu : byte
{
    Nhap = 1,
    DaGhiSo = 2,
    DaHuy = 3,
}
