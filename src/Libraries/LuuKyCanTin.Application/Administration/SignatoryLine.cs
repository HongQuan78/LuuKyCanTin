namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// One edited signer line of a template. The order in the list is the printed left-to-right order; the service
/// assigns <c>Ordinal</c> 1..n. A null <paramref name="OfficerId"/> prints a blank name line.
/// </summary>
public sealed record SignatoryLine(string Title, int? OfficerId = null);
