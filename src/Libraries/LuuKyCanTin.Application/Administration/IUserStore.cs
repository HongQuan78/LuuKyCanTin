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
}
