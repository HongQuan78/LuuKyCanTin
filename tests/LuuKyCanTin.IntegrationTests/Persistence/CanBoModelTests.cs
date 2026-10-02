using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public class CanBoModelTests
{
    private static readonly IEntityType CanBo = LoadEntityType();

    private static IEntityType LoadEntityType()
    {
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=unused").Options);
        return context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(CanBo))!;
    }

    [Theory]
    [InlineData(nameof(Domain.DanhMuc.CanBo.MaCanBo), "varchar(20)", false, null)]
    [InlineData(nameof(Domain.DanhMuc.CanBo.HoTen), "nvarchar(100)", false, null)]
    [InlineData(nameof(Domain.DanhMuc.CanBo.ChucVu), "nvarchar(100)", true, null)]
    [InlineData(nameof(Domain.DanhMuc.CanBo.LaQuanGiao), "bit", false, false)]
    [InlineData(nameof(Domain.DanhMuc.CanBo.DangCongTac), "bit", false, true)]
    public void Column_MatchesTheDbDesign(string name, string columnType, bool nullable, object? defaultValue)
    {
        var property = CanBo.FindProperty(name)!;

        property.GetColumnType().ShouldBe(columnType);
        property.IsNullable.ShouldBe(nullable);
        property.GetDefaultValue().ShouldBe(defaultValue);
    }

    [Fact]
    public void HoTen_UsesTheSearchCollation_SoNamesMatchWithoutAnyDiacritics()
    {
        CanBo.FindProperty(nameof(Domain.DanhMuc.CanBo.HoTen))!.GetCollation().ShouldBe(AppDbContext.CollationTimKiem);
    }

    [Fact]
    public void Table_IsCanBo()
    {
        CanBo.GetTableName().ShouldBe("CanBo");
    }

    [Fact]
    public void MaCanBo_IsUnique()
    {
        CanBo.GetIndexes().ShouldContain(i => i.IsUnique && i.Properties.Single().Name == nameof(Domain.DanhMuc.CanBo.MaCanBo));
    }

    [Fact]
    public void HoTen_IsIndexedForSearch()
    {
        CanBo.GetIndexes().ShouldContain(i => i.GetDatabaseName() == "IX_CanBo_HoTen" && !i.IsUnique);
    }
}
