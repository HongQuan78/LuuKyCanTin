namespace LuuKyCanTin.Domain.Common;

/// <summary>
/// A voucher that can be cancelled. Lets the audit log record a cancellation as such without knowing
/// each voucher's status enum.
/// </summary>
public interface ICoTrangThaiHuy
{
    bool DaHuy { get; }
}
