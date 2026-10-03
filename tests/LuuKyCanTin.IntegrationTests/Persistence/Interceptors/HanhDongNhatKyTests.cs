using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Interceptors;

public sealed class HanhDongNhatKyTests
{
    [Theory]
    [InlineData(EntityState.Added, false, false, HanhDong.Them)]
    [InlineData(EntityState.Added, false, true, HanhDong.Them)]
    [InlineData(EntityState.Modified, false, false, HanhDong.Sua)]
    [InlineData(EntityState.Modified, false, true, HanhDong.Huy)]
    [InlineData(EntityState.Modified, true, true, HanhDong.Sua)]
    [InlineData(EntityState.Modified, true, false, HanhDong.Sua)]
    public void XacDinh_EntryStateAndCancellation_MapsToHanhDong(EntityState state, bool daHuyTruoc, bool daHuySau, HanhDong expected)
    {
        HanhDongNhatKy.XacDinh(state, daHuyTruoc, daHuySau).ShouldBe(expected);
    }

    [Fact]
    public void XacDinh_Deleted_ThrowsBecauseVouchersAreCancelledNotDeleted()
    {
        Should.Throw<InvalidOperationException>(() => HanhDongNhatKy.XacDinh(EntityState.Deleted, false, false));
    }

    [Theory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Detached)]
    public void XacDinh_StateWithoutAChange_Throws(EntityState state)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => HanhDongNhatKy.XacDinh(state, false, false));
    }
}
