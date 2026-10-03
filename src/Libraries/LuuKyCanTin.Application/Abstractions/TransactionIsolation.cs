namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// The isolation level a posting operation asks for when it opens its transaction. Most operations use
/// <see cref="Default"/>; a read-check-write that must not interleave with another workstation's asks for
/// <see cref="Serializable"/>.
/// </summary>
public enum TransactionIsolation : byte
{
    /// <summary>The provider's default (read committed on SQL Server), enough for a single posting.</summary>
    Default = 0,

    /// <summary>Range locks around the whole read-check-write, so two guarded changes cannot both pass the check.</summary>
    Serializable = 1,
}
