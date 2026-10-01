using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Audit;

public class XacDinhHanhDongTests
{
    [Theory]
    [InlineData(EntityState.Added, false, false, HanhDong.Them)]
    [InlineData(EntityState.Added, false, true, HanhDong.Them)]
    [InlineData(EntityState.Modified, false, false, HanhDong.Sua)]
    [InlineData(EntityState.Modified, false, true, HanhDong.Huy)]
    [InlineData(EntityState.Modified, true, true, HanhDong.Sua)]
    [InlineData(EntityState.Modified, true, false, HanhDong.Sua)]
    public void Tu_MapsEntryStateAndCancellation(EntityState state, bool daHuyTruoc, bool daHuySau, HanhDong expected)
    {
        XacDinhHanhDong.Tu(state, daHuyTruoc, daHuySau).ShouldBe(expected);
    }

    [Fact]
    public void Tu_Deleted_Throws_BecauseVouchersAreCancelledNotDeleted()
    {
        Should.Throw<InvalidOperationException>(() => XacDinhHanhDong.Tu(EntityState.Deleted, false, false));
    }

    [Theory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Detached)]
    public void Tu_StateWithoutAChange_Throws(EntityState state)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => XacDinhHanhDong.Tu(state, false, false));
    }
}
