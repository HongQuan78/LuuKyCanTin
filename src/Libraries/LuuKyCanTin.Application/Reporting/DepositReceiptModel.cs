using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;

namespace LuuKyCanTin.Application.Reporting;

/// <summary>
/// Everything the skeleton's receipt print shows. Values come from the stored document and the unit row, so
/// the print matches the posting even years later.
/// </summary>
public sealed class DepositReceiptModel : IReportModel
{
    public const string PrintTemplate = "BIEN_NHAN_THU";

    public string TemplateCode => PrintTemplate;

    public string? ParentAgencyName { get; init; }

    public string FacilityName { get; init; } = "";

    public string Address { get; init; } = "";

    public string VoucherNumber { get; init; } = "";

    public DateOnly VoucherDate { get; init; }

    public string InmateFullName { get; init; } = "";

    public InmateType InmateType { get; init; }

    public string? SenderFullName { get; init; }

    public string? Relationship { get; init; }

    public PaymentMethod PaymentMethod { get; init; }

    public string? SenderAccountNumber { get; init; }

    public string? Description { get; init; }

    public decimal Amount { get; init; }

    public string AmountInWords { get; init; } = "";
}
