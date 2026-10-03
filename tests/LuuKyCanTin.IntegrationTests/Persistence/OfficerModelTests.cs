using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public class OfficerModelTests
{
    private static readonly IEntityType Officer = LoadEntityType();

    private static IEntityType LoadEntityType()
    {
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=unused").Options);
        return context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Officer))!;
    }

    [Theory]
    [InlineData(nameof(Domain.MasterData.Officer.OfficerCode), "varchar(20)", false, null)]
    [InlineData(nameof(Domain.MasterData.Officer.FullName), "nvarchar(100)", false, null)]
    [InlineData(nameof(Domain.MasterData.Officer.Position), "nvarchar(100)", true, null)]
    [InlineData(nameof(Domain.MasterData.Officer.IsSupervisingOfficer), "bit", false, false)]
    [InlineData(nameof(Domain.MasterData.Officer.IsActive), "bit", false, true)]
    public void Column_MatchesTheDbDesign(string name, string columnType, bool nullable, object? defaultValue)
    {
        var property = Officer.FindProperty(name)!;

        property.GetColumnType().ShouldBe(columnType);
        property.IsNullable.ShouldBe(nullable);
        property.GetDefaultValue().ShouldBe(defaultValue);
    }

    [Fact]
    public void FullName_UsesTheSearchCollation_SoNamesMatchWithoutAnyDiacritics()
    {
        Officer.FindProperty(nameof(Domain.MasterData.Officer.FullName))!.GetCollation().ShouldBe(AppDbContext.SearchCollation);
    }

    [Fact]
    public void Table_IsOfficer()
    {
        Officer.GetTableName().ShouldBe("Officer");
    }

    [Fact]
    public void OfficerCode_IsUnique()
    {
        Officer.GetIndexes().ShouldContain(i => i.IsUnique && i.Properties.Single().Name == nameof(Domain.MasterData.Officer.OfficerCode));
    }

    [Fact]
    public void FullName_IsIndexedForSearch()
    {
        Officer.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_Officer_FullName" && !i.IsUnique);
    }
}
