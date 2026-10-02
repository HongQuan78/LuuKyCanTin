namespace LuuKyCanTin.Application.Abstractions;

/// <summary>Hashes and verifies passwords. The stored string is self-describing so iterations and salt can change later.</summary>
public interface IMatKhauHasher
{
    string Hash(string matKhau);

    /// <summary>False for a wrong password and for a malformed stored hash; never throws on bad input.</summary>
    bool Verify(string matKhau, string hash);
}
