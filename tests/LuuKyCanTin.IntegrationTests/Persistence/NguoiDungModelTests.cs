using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public class NguoiDungModelTests
{
    private static readonly DbContextOptions<AppDbContext> Options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    private static IEntityType EntityType()
    {
        using var context = new AppDbContext(Options);
        return context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(NguoiDung))!;
    }

    [Fact]
    public void CanBoId_IsANullableRestrictedForeignKeyToCanBo()
    {
        var canBoId = EntityType().FindProperty(nameof(NguoiDung.CanBoId))!;
        var foreignKey = EntityType().GetForeignKeys()
            .Single(fk => fk.Properties.Single().Name == nameof(NguoiDung.CanBoId));

        canBoId.IsNullable.ShouldBeTrue();
        canBoId.GetColumnType().ShouldBe("int");
        foreignKey.PrincipalEntityType.ClrType.ShouldBe(typeof(CanBo));
        foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
    }

    [Fact]
    public void OneActiveAccountPerStaffMember_IsAUniqueFilteredIndex()
    {
        var index = EntityType().GetIndexes()
            .Single(i => i.GetDatabaseName() == "UX_NguoiDung_CanBoId_DangHoatDong");

        index.IsUnique.ShouldBeTrue();
        index.Properties.Single().Name.ShouldBe(nameof(NguoiDung.CanBoId));
        index.GetFilter().ShouldBe("[DangHoatDong] = 1 AND [CanBoId] IS NOT NULL");
    }

    [Fact]
    public void OnlyTheBuiltInAdmin_MayHaveNoStaffRecord()
    {
        var check = EntityType().GetCheckConstraints().ShouldHaveSingleItem();

        check.Sql.ShouldBe("[CanBoId] IS NOT NULL OR [TenDangNhap] = 'admin'");
    }
}
