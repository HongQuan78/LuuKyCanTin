using LuuKyCanTin.Domain.HeThong;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence.Interceptors;

/// <summary>Which audit action a saved change is.</summary>
internal static class XacDinhHanhDong
{
    public static HanhDong Tu(EntityState state, bool daHuyTruoc, bool daHuySau) => state switch
    {
        EntityState.Added => HanhDong.Them,
        EntityState.Modified => !daHuyTruoc && daHuySau ? HanhDong.Huy : HanhDong.Sua,
        EntityState.Deleted => throw new InvalidOperationException(
            "Audited records are never deleted; cancel the voucher instead."),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Only added or modified entries are audited."),
    };
}
