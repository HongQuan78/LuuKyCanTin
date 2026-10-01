using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public class NhatKyThaoTacModelTests
{
    private static readonly DbContextOptions<AppDbContext> Options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    private static IEntityType EntityType()
    {
        using var context = new AppDbContext(Options);
        return context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(NhatKyThaoTac))!;
    }

    [Fact]
    public void Table_IsNhatKyThaoTac()
    {
        EntityType().GetTableName().ShouldBe("NhatKyThaoTac");
    }

    [Theory]
    [InlineData(nameof(NhatKyThaoTac.Id), "bigint", false)]
    [InlineData(nameof(NhatKyThaoTac.ThoiDiem), "datetime2(0)", false)]
    [InlineData(nameof(NhatKyThaoTac.NguoiDungId), "int", true)]
    [InlineData(nameof(NhatKyThaoTac.MayTram), "nvarchar(100)", true)]
    [InlineData(nameof(NhatKyThaoTac.HanhDong), "varchar(20)", false)]
    [InlineData(nameof(NhatKyThaoTac.TenBang), "varchar(50)", true)]
    [InlineData(nameof(NhatKyThaoTac.BanGhiId), "bigint", true)]
    [InlineData(nameof(NhatKyThaoTac.DuLieuCu), "nvarchar(max)", true)]
    [InlineData(nameof(NhatKyThaoTac.DuLieuMoi), "nvarchar(max)", true)]
    public void Column_MatchesTheDatabaseDesign(string name, string columnType, bool nullable)
    {
        var property = EntityType().FindProperty(name)!;

        property.GetColumnType().ShouldBe(columnType);
        property.IsNullable.ShouldBe(nullable);
    }

    [Fact]
    public void Id_IsIdentity()
    {
        EntityType().FindProperty(nameof(NhatKyThaoTac.Id))!.GetValueGenerationStrategy()
            .ShouldBe(SqlServerValueGenerationStrategy.IdentityColumn);
    }

    [Fact]
    public void HanhDong_IsCheckedAgainstTheEnumNames()
    {
        EntityType().GetCheckConstraints().ShouldHaveSingleItem().Sql
            .ShouldBe("[HanhDong] IN ('Them', 'Sua', 'Huy', 'In', 'Duyet', 'DangNhap')");
    }

    [Fact]
    public void Indexes_SupportSearchByRecordAndByTime()
    {
        EntityType().GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name)))
            .ShouldBe(["TenBang,BanGhiId", "ThoiDiem"], ignoreOrder: true);
    }
}
