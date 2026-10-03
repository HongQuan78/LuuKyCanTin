using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.MasterData;

/// <summary>A detainee whose custodial balance the canteen and the ledger work against.</summary>
public sealed class Inmate : AuditableEntity
{
    public int Id { get; set; }

    public string InmateCode { get; set; } = "";

    public string FullName { get; set; } = "";

    public short? BirthYear { get; set; }

    public InmateType InmateType { get; set; }

    public DateOnly AdmissionDate { get; set; }

    public string? Cell { get; set; }

    public InmateStatus Status { get; set; } = InmateStatus.InCustody;

    public DateOnly? ReleaseDate { get; set; }

    /// <summary>
    /// Only the ledger engine changes a balance, with a conditional UPDATE under a row lock. No C# code
    /// sets this, and EF is configured to never write the column, so a save can't change it by accident.
    /// </summary>
    public decimal CustodyBalance { get; private set; }
}
