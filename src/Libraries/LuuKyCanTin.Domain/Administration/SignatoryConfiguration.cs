namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// One signer line of a print template: the title printed over the signature column and, optionally, the staff
/// member whose name is printed under it. <see cref="Ordinal"/> is the left-to-right position, starting at 1.
/// </summary>
/// <remarks>
/// Deliberately not <c>IAuditable</c> and without audit columns: a save replaces a template's whole row set, so the
/// interceptor would see only deletes and adds. <c>SignatoryConfigurationService</c> writes one explicit audit row
/// with the before and after lists instead.
/// </remarks>
public sealed class SignatoryConfiguration
{
    public int Id { get; set; }

    public string TemplateCode { get; set; } = "";

    public byte Ordinal { get; set; }

    public string Title { get; set; } = "";

    /// <summary>Null prints a blank name line (a signer role that is not a staff member).</summary>
    public int? OfficerId { get; set; }
}
