using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Administration;

public interface IUserStore
{
    Task<User?> FindByUserNameAsync(string userName, CancellationToken ct = default);

    Task<User?> FindByIdAsync(int userId, CancellationToken ct = default);

    /// <summary>Saves the account through the context that translates provider errors and writes the audit row.</summary>
    Task SaveAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Refreshes a tracked account after a row-version conflict, so the caller can re-apply its change to the
    /// values another workstation wrote in the meantime.
    /// </summary>
    Task ReloadAsync(User user, CancellationToken ct = default);

    /// <summary>Adds a new account and saves it, so the database's own constraints decide any conflict.</summary>
    Task AddAsync(User user, CancellationToken ct = default);

    /// <summary>Every account with its officer's full name and roles, ordered by sign-in name.</summary>
    Task<IReadOnlyList<AccountRow>> GetAllWithRolesAsync(CancellationToken ct = default);

    /// <summary>True when the officer already has another active account.</summary>
    /// <param name="excludedUserId">The account being reactivated, which must not count as the other one.</param>
    Task<bool> HasActiveAccountAsync(int officerId, int? excludedUserId = null, CancellationToken ct = default);

    /// <summary>The officer's full name, loaded at sign-in for the session.</summary>
    Task<string?> GetOfficerFullNameAsync(int officerId, CancellationToken ct = default);
}
