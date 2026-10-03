namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// The account just created and its temporary password. The password exists only here: it is never stored in
/// clear and never written to the audit log.
/// </summary>
public sealed record CreateAccountResult(int UserId, string TemporaryPassword);
