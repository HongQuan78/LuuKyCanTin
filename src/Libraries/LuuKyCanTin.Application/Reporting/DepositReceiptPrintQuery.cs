using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Custody;

namespace LuuKyCanTin.Application.Reporting;

/// <summary>
/// Builds the receipt print model from the stored snapshot columns plus the unit header. It never reads the
/// detainee master, so a reprint matches the original.
/// </summary>
public sealed class DepositReceiptPrintQuery(ICustodyVoucherStore voucherStore, IFacilityInfoStore facilityInfoStore)
{
    public async Task<DepositReceiptModel?> GetAsync(long voucherId, CancellationToken ct = default)
    {
        var voucher = await voucherStore.FindByIdAsync(voucherId, ct);
        if (voucher is null)
            return null;

        var facility = await facilityInfoStore.GetAsync(ct);

        return new DepositReceiptModel
        {
            ParentAgencyName = facility?.ParentAgencyName,
            FacilityName = facility?.FacilityName ?? "",
            Address = facility?.Address ?? "",
            VoucherNumber = voucher.VoucherNumber,
            VoucherDate = voucher.VoucherDate,
            InmateFullName = voucher.InmateFullName,
            InmateType = voucher.InmateType,
            SenderFullName = voucher.SenderFullName,
            Relationship = voucher.Relationship,
            PaymentMethod = voucher.PaymentMethod,
            SenderAccountNumber = voucher.SenderAccountNumber,
            Description = voucher.Description,
            Amount = voucher.Amount,
            AmountInWords = voucher.AmountInWords,
        };
    }
}
