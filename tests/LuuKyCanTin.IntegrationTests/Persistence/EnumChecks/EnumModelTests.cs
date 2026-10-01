using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

// Model-only: kept out of the SQL Server collection so they run, and pass, without a server.
public class EnumModelTests
{
    private static readonly DbContextOptions<AppDbContext> ModelOnlyOptions = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    [Fact]
    public void EveryEnumColumn_IsTinyint()
    {
        using var db = new AppDbContext(ModelOnlyOptions);

        EnumModel.EnumPropertiesOf(db)
            .Where(p => p.GetColumnType() != "tinyint")
            .Select(p => $"{p.DeclaringType.DisplayName()}.{p.Name} is {p.GetColumnType()}; declare the enum ': byte'")
            .ShouldBeEmpty();
    }

    [Fact]
    public void ModelWalk_FindsNullableAndNonNullableEnumColumns()
    {
        using var db = new TestAppDbContext(ModelOnlyOptions);

        EnumModel.EnumColumnsOf(db).ShouldBe(
        [
            new EnumColumn(EnumModel.DefaultSchema, "MauChungTu", "TrangThai", typeof(MauTrangThai)),
            new EnumColumn(EnumModel.DefaultSchema, "MauChungTu", "TrangThaiTruoc", typeof(MauTrangThai)),
        ], ignoreOrder: true);
    }
}
