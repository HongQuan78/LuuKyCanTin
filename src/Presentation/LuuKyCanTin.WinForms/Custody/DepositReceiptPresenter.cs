using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.WinForms.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Custody;

/// <summary>
/// The skeleton's receipt form: pick a detainee, enter the receipt, post through the ledger engine, print.
/// The full form (A.I.4 fields, money control UX-DR1, pre-posting confirmation UX-DR4) is Epic 4.
/// </summary>
public sealed class DepositReceiptPresenter
{
    private const string LoadFailureMessage = "Không tải được danh sách đối tượng";

    private readonly IDepositReceiptView _view;
    private readonly IServiceScopeFactory _scopeFactory;

    private long? _voucherId;

    public DepositReceiptPresenter(IDepositReceiptView view, IServiceScopeFactory scopeFactory)
    {
        _view = view;
        _scopeFactory = scopeFactory;
        _view.LoadRequested += async (_, _) => await ReportFailureAsync(LoadFailureMessage, LoadAsync);
        _view.AmountChanged += (_, _) => UpdateAmountInWords();
        _view.PostClicked += async (_, _) => await ReportFailureAsync("Không ghi sổ được", PostAsync);
        _view.PrintClicked += async (_, _) => await ReportFailureAsync("Không in được", PrintAsync);
        _view.ResetClicked += async (_, _) => await ReportFailureAsync(LoadFailureMessage, ResetAsync);
    }

    private static DepositReceiptField? ToField(string propertyName) => propertyName switch
    {
        nameof(PostDepositReceiptRequest.InmateId) => DepositReceiptField.Inmate,
        nameof(PostDepositReceiptRequest.TransactionType) => DepositReceiptField.TransactionType,
        nameof(PostDepositReceiptRequest.SenderFullName) => DepositReceiptField.SenderFullName,
        nameof(PostDepositReceiptRequest.Relationship) => DepositReceiptField.Relationship,
        nameof(PostDepositReceiptRequest.PaymentMethod) => DepositReceiptField.PaymentMethod,
        nameof(PostDepositReceiptRequest.SenderAccountNumber) => DepositReceiptField.AccountNumber,
        nameof(PostDepositReceiptRequest.VoucherDate) => DepositReceiptField.VoucherDate,
        nameof(PostDepositReceiptRequest.Description) => DepositReceiptField.Description,
        nameof(PostDepositReceiptRequest.Amount) => DepositReceiptField.Amount,
        _ => null,
    };

    // An unexpected failure (database down, render error) must still reach the user, not the global handler.
    private async Task ReportFailureAsync(string failureMessage, Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            _view.ShowError($"{failureMessage}: {ex.Message}");
        }
    }

    public async Task LoadAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var query = scope.ServiceProvider.GetRequiredService<InmatesInCustodyQuery>();
        _view.Inmates = await query.GetAsync();
    }

    /// <summary>Starts the next receipt; the detainee list is reloaded, since the previous one may be stale.</summary>
    public async Task ResetAsync()
    {
        _voucherId = null;
        _view.Reset();
        await LoadAsync();
    }

    public void UpdateAmountInWords()
    {
        var amount = _view.Amount;
        _view.AmountInWords = amount is > 0 ? AmountInWords.ToWords(amount.Value) : "";
    }

    public async Task PostAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var ledger = scope.ServiceProvider.GetRequiredService<CustodyLedgerService>();

        var request = new PostDepositReceiptRequest
        {
            InmateId = _view.InmateId ?? 0,
            VoucherDate = _view.VoucherDate,
            TransactionType = _view.TransactionType,
            PaymentMethod = _view.PaymentMethod,
            SenderFullName = _view.SenderFullName,
            Relationship = _view.Relationship,
            SenderAccountNumber = _view.SenderAccountNumber,
            Description = _view.Description,
            Amount = _view.Amount ?? 0,
        };

        var result = await ledger.PostDepositReceiptAsync(request);
        if (!result.Succeeded)
        {
            ShowFailure(result);
            return;
        }

        _voucherId = result.Id;
        _view.ShowPosted(result.VoucherNumber, result.BalanceAfter);
    }

    // Validation goes under the fields; the server's re-check (detainee no longer managed, balance) stays a MessageBox.
    private void ShowFailure(PostingResult result)
    {
        var (fieldErrors, otherMessages) = FieldMessages.Split(result.Errors, ToField);
        if (fieldErrors.Count > 0)
            _view.ShowFieldErrors(fieldErrors);
        if (otherMessages.Count > 0)
            _view.ShowError(string.Join('\n', otherMessages));
        else if (fieldErrors.Count == 0)
            _view.ShowError(result.Message ?? "Không ghi sổ được biên nhận.");
    }

    public async Task PrintAsync()
    {
        if (_voucherId is not { } id)
        {
            _view.ShowError("Chưa có chứng từ để in.");
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var query = scope.ServiceProvider.GetRequiredService<DepositReceiptPrintQuery>();
        var model = await query.GetAsync(id);
        if (model is null)
        {
            _view.ShowError("Không tìm thấy chứng từ để in.");
            return;
        }

        var renderer = scope.ServiceProvider.GetRequiredService<IReportRenderer>();
        _view.ShowPrintPreview(renderer.Render(model), $"BienNhanThu-{model.VoucherNumber}");
    }
}
