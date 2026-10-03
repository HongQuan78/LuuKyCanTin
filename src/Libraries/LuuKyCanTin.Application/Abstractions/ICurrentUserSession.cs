namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// The signed-in user, plus the two operations that change the session. Read-only clients depend on
/// <see cref="ICurrentUser"/>; only sign-in and sign-out need this wider contract.
/// </summary>
public interface ICurrentUserSession : ICurrentUser
{
    /// <param name="canBoId">The staff member behind the account, or null for the built-in admin.</param>
    /// <param name="hoTen">The staff name; null falls back to the sign-in name.</param>
    void DangNhap(int nguoiDungId, string tenDangNhap, int? canBoId, string? hoTen);

    void DangXuat();
}
