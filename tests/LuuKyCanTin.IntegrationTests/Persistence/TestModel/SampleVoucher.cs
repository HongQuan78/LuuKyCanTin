using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.IntegrationTests.Persistence.TestModel;

// A test-only voucher that exercises every shared convention and the audit log, since the real model has no vouchers yet.
public sealed class SampleVoucher : AuditableEntity, IAuditable, ICancellable
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateOnly VoucherDate { get; set; }
    public DateTime? PrintedAt { get; set; }
    public string Description { get; set; } = "";
    public SampleStatus Status { get; set; }
    public SampleStatus? PreviousStatus { get; set; }

    [NotAudited]
    public string? Secret { get; set; }

    public bool IsCancelled => Status == SampleStatus.Cancelled;
}
