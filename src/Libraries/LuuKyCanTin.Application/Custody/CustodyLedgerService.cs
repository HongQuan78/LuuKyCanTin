using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;

namespace LuuKyCanTin.Application.Custody;

/// <summary>
/// The one engine that writes custodial documents and balances. Every later money flow calls it; nothing
/// bypasses it, even in the walking skeleton. This version supports receipts only (Epic 4 adds the rest).
/// </summary>
public sealed class CustodyLedgerService(
    IInmateStore inmateStore,
    ICustodyVoucherStore voucherStore,
    IAppDbContext db,
    INumberingService numberingService,
    ICustodyBalanceWriter balanceWriter,
    IClock clock)
{
    public const string ReceiptVoucherTypeCode = "BNT";

    private readonly PostDepositReceiptRequestValidator _validator = new();

    /// <summary>
    /// Posts a receipt: number, balance, document and audit row commit or roll back together. Business
    /// failures come back as <see cref="PostingResult.Fail"/> without touching the database.
    /// </summary>
    public async Task<PostingResult> PostDepositReceiptAsync(PostDepositReceiptRequest request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return PostingResult.Fail(validation.Errors);

        await using var transaction = await db.BeginTransactionAsync(ct);

        // Snapshot from the master inside the transaction, and refuse a detainee who is no longer managed.
        var inmate = await inmateStore.FindByIdAsync(request.InmateId, ct);
        if (inmate is null || inmate.Status != InmateStatus.InCustody)
        {
            await transaction.RollbackAsync(ct);
            return PostingResult.Fail("Đối tượng không tồn tại hoặc không còn được quản lý.");
        }

        // The counter year follows the working date; back-date rules arrive with the period lock (Epic 4/6).
        var voucherNumber = await numberingService.AllocateNumberAsync(ReceiptVoucherTypeCode, clock.Today.Year, ct);

        // The balance update is the concurrency point: a conditional UPDATE under a row lock, never read-then-write.
        var balance = await balanceWriter.IncreaseAsync(inmate.Id, request.Amount, ct);
        if (balance is null)
        {
            await transaction.RollbackAsync(ct);
            return PostingResult.Fail("Không cập nhật được số dư lưu ký.");
        }

        var voucher = CustodyVoucher.CreatePostedDepositReceipt(
            voucherNumber,
            request.VoucherDate,
            inmate,
            VoucherType.Receipt,
            request.TransactionType,
            request.PaymentMethod,
            request.SenderFullName?.Trim(),
            request.Relationship?.Trim(),
            request.SourceDocumentNumber?.Trim(),
            request.SenderAccountNumber?.Trim(),
            request.ReceivedDate,
            request.Description?.Trim(),
            request.Amount,
            AmountInWords.ToWords(request.Amount),
            balance.Value.BalanceBefore,
            balance.Value.BalanceAfter);

        voucherStore.Add(voucher);

        // The audit interceptor sees the open transaction and writes the Create row into it.
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return PostingResult.Ok(voucher.Id, voucher.VoucherNumber, balance.Value.BalanceAfter);
    }
}
