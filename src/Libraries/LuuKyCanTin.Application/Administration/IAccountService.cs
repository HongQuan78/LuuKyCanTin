using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// Account administration (HT-02): create a staff account with a temporary password, change its roles,
/// deactivate/reactivate it, unlock it and reset its password. Accounts are never hard-deleted.
/// </summary>
public interface IAccountService
{
    Task<IReadOnlyList<AccountDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>The active staff members who have no active account yet, for the create dialog.</summary>
    Task<IReadOnlyList<OfficerDto>> GetOfficersForAccountCreationAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates the account with a generated temporary password and forces a change at first sign-in.
    /// </summary>
    /// <exception cref="BusinessRuleException">Invalid input, an unknown staff member or role, or the person already has an active account.</exception>
    /// <exception cref="PermissionDeniedException">The current user lacks HT.Sua.</exception>
    Task<CreateAccountResult> CreateAsync(CreateAccountRequest request, CancellationToken ct = default);

    /// <summary>Replaces the account's roles and writes one audit row with the before/after role codes.</summary>
    /// <exception cref="BusinessRuleException">The account or a role doesn't exist, or the change would leave no administrator.</exception>
    /// <exception cref="PermissionDeniedException">The current user lacks HT.Sua.</exception>
    Task UpdateRolesAsync(int userId, IReadOnlyCollection<int> roleIds, CancellationToken ct = default);

    /// <exception cref="BusinessRuleException">The account doesn't exist, is the current user's own, or is the last administrator.</exception>
    /// <exception cref="PermissionDeniedException">The current user lacks HT.Sua.</exception>
    Task DeactivateAsync(int userId, CancellationToken ct = default);

    /// <exception cref="BusinessRuleException">The account doesn't exist, or its staff member already has an active account.</exception>
    /// <exception cref="PermissionDeniedException">The current user lacks HT.Sua.</exception>
    Task ReactivateAsync(int userId, CancellationToken ct = default);

    /// <exception cref="BusinessRuleException">The account doesn't exist.</exception>
    /// <exception cref="PermissionDeniedException">The current user lacks HT.Sua.</exception>
    Task UnlockAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Sets a new temporary password, forces a change at next sign-in and clears the lock.
    /// </summary>
    /// <returns>The temporary password, to show once; it is never stored in clear.</returns>
    /// <exception cref="BusinessRuleException">The account doesn't exist.</exception>
    /// <exception cref="PermissionDeniedException">The current user lacks HT.Sua.</exception>
    Task<string> ResetPasswordAsync(int userId, CancellationToken ct = default);
}
