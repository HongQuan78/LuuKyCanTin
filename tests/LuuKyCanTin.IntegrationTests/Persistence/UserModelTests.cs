using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public class UserModelTests
{
    private static readonly IEntityType User = LoadEntityType();

    private static IEntityType LoadEntityType()
    {
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=unused").Options);
        return context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(LuuKyCanTin.Domain.Administration.User))!;
    }

    [Fact]
    public void OfficerId_IsANullableRestrictedForeignKeyToOfficer()
    {
        var officerId = User.FindProperty(nameof(LuuKyCanTin.Domain.Administration.User.OfficerId))!;
        var foreignKey = User.GetForeignKeys()
            .Single(fk => fk.Properties.Single().Name == nameof(LuuKyCanTin.Domain.Administration.User.OfficerId));

        officerId.IsNullable.ShouldBeTrue();
        officerId.GetColumnType().ShouldBe("int");
        foreignKey.PrincipalEntityType.ClrType.ShouldBe(typeof(Officer));
        foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
    }

    [Fact]
    public void OneActiveAccountPerOfficer_IsAUniqueFilteredIndex()
    {
        var index = User.GetIndexes()
            .Single(i => i.GetDatabaseName() == "UX_User_OfficerId_IsActive");

        index.IsUnique.ShouldBeTrue();
        index.Properties.Single().Name.ShouldBe(nameof(LuuKyCanTin.Domain.Administration.User.OfficerId));
        index.GetFilter().ShouldBe("[IsActive] = 1 AND [OfficerId] IS NOT NULL");
    }

    [Fact]
    public void OnlyTheBuiltInAdmin_MayHaveNoOfficerRecord()
    {
        var check = User.GetCheckConstraints().Single(c => c.ModelName == "CK_User_OfficerId");

        check.Sql.ShouldBe("[OfficerId] IS NOT NULL OR [UserName] = 'admin'");
    }
}
