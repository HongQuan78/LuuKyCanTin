namespace LuuKyCanTin.Domain.Common;

/// <summary>The shared document state machine: Draft → Posted → Cancelled (with a reason).</summary>
public enum VoucherStatus : byte
{
    Draft = 1,
    Posted = 2,
    Cancelled = 3,
}
