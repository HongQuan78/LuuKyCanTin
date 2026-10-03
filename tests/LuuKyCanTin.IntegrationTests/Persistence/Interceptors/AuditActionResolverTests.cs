using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Interceptors;

public sealed class AuditActionResolverTests
{
    [Theory]
    [InlineData(EntityState.Added, false, false, AuditAction.Create)]
    [InlineData(EntityState.Added, false, true, AuditAction.Create)]
    [InlineData(EntityState.Modified, false, false, AuditAction.Update)]
    [InlineData(EntityState.Modified, false, true, AuditAction.Cancel)]
    [InlineData(EntityState.Modified, true, true, AuditAction.Update)]
    [InlineData(EntityState.Modified, true, false, AuditAction.Update)]
    public void Resolve_EntryStateAndCancellation_MapsToAuditAction(EntityState state, bool isCancelledBefore, bool isCancelledAfter, AuditAction expected)
    {
        AuditActionResolver.Resolve(state, isCancelledBefore, isCancelledAfter).ShouldBe(expected);
    }

    [Fact]
    public void Resolve_Deleted_ThrowsBecauseVouchersAreCancelledNotDeleted()
    {
        Should.Throw<InvalidOperationException>(() => AuditActionResolver.Resolve(EntityState.Deleted, false, false));
    }

    [Theory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Detached)]
    public void Resolve_StateWithoutAChange_Throws(EntityState state)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => AuditActionResolver.Resolve(state, false, false));
    }
}
