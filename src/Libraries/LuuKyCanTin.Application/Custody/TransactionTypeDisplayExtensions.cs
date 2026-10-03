using LuuKyCanTin.Domain.Custody;

namespace LuuKyCanTin.Application.Custody;

public static class TransactionTypeDisplayExtensions
{
    public static string ToDisplayText(this TransactionType value) => value switch
    {
        TransactionType.BroughtOnAdmission => "Mang theo khi vào",
        TransactionType.SentByRelative => "Người thân gửi",
        TransactionType.GiftSlip => "Phiếu gửi quà",
        TransactionType.ReceivedFromOtherInmate => "Nhận từ đối tượng khác",
        TransactionType.CanteenPurchase => "Mua hàng",
        TransactionType.GivenToOtherInmate => "Cho tiền",
        TransactionType.ReturnedToRelative => "Chuyển về người thân",
        TransactionType.FacilityTransfer => "Chuyển trại",
        TransactionType.SentenceCompleted => "Chấp hành xong án",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown transaction type."),
    };
}
