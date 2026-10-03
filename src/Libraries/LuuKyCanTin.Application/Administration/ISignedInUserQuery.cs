namespace LuuKyCanTin.Application.Administration;

/// <summary>Who is signed in, for the shell's user block, greeting and unit name.</summary>
public interface ISignedInUserQuery
{
    /// <returns>Null when nobody is signed in.</returns>
    Task<SignedInUserDto?> GetAsync(CancellationToken ct = default);
}
