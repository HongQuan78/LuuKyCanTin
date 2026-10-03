using LuuKyCanTin.Domain.Administration;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence.Interceptors;

/// <summary>Which audit action a saved change is.</summary>
internal static class AuditActionResolver
{
    public static AuditAction Resolve(EntityState state, bool isCancelledBefore, bool isCancelledAfter) => state switch
    {
        EntityState.Added => AuditAction.Create,
        EntityState.Modified => !isCancelledBefore && isCancelledAfter ? AuditAction.Cancel : AuditAction.Update,
        EntityState.Deleted => throw new InvalidOperationException(
            "Audited records are never deleted; cancel the voucher instead."),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Only added or modified entries are audited."),
    };
}
