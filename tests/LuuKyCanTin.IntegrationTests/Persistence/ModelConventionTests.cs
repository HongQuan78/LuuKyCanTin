using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

// Model-only checks: building the model needs the SqlServer provider but never opens a connection.
public sealed class ModelConventionTests
{
    private static readonly DbContextOptions<AppDbContext> Options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    private static IModel LayDesignTimeModel(DbContext context) => context.GetService<IDesignTimeModel>().Model;

    private static IProperty LayThuocTinh(string name)
    {
        using var context = new TestAppDbContext(Options);
        return LayDesignTimeModel(context).FindEntityType(typeof(MauChungTu))!.FindProperty(name)!;
    }

    [Fact]
    public void OnModelCreating_AnyModel_UsesVietnameseCaseAndAccentInsensitiveCollation()
    {
        using var context = new AppDbContext(Options);

        LayDesignTimeModel(context).GetCollation().ShouldBe("Vietnamese_CI_AI");
    }

    [Theory]
    [InlineData(nameof(MauChungTu.SoTien), "decimal(18,0)", false)]
    [InlineData(nameof(MauChungTu.NgayChungTu), "date", false)]
    [InlineData(nameof(MauChungTu.ThoiDiemIn), "datetime2(0)", true)]
    [InlineData(nameof(MauChungTu.NoiDung), "nvarchar(max)", false)]
    [InlineData(nameof(MauChungTu.TrangThai), "tinyint", false)]
    [InlineData(nameof(MauChungTu.TrangThaiTruoc), "tinyint", true)]
    [InlineData(nameof(MauChungTu.NgayTao), "datetime2(0)", false)]
    [InlineData(nameof(MauChungTu.NguoiTaoId), "int", false)]
    [InlineData(nameof(MauChungTu.NgaySua), "datetime2(0)", true)]
    [InlineData(nameof(MauChungTu.NguoiSuaId), "int", true)]
    [InlineData(nameof(MauChungTu.RowVer), "rowversion", false)]
    public void ConfigureConventions_SharedPropertyType_HasExpectedColumnType(string name, string columnType, bool nullable)
    {
        var property = LayThuocTinh(name);

        property.GetColumnType().ShouldBe(columnType);
        property.IsNullable.ShouldBe(nullable);
    }

    [Fact]
    public void OnModelCreating_IntKey_IsIdentity()
    {
        LayThuocTinh(nameof(MauChungTu.Id)).GetValueGenerationStrategy().ShouldBe(SqlServerValueGenerationStrategy.IdentityColumn);
    }

    [Fact]
    public void Configure_RowVer_IsConcurrencyToken()
    {
        var rowVer = LayThuocTinh(nameof(MauChungTu.RowVer));

        rowVer.IsConcurrencyToken.ShouldBeTrue();
        rowVer.ValueGenerated.ShouldBe(ValueGenerated.OnAddOrUpdate);
    }

    [Fact]
    public void HasEnumCheck_EnumColumn_EmitsOneInListConstraint()
    {
        using var context = new TestAppDbContext(Options);
        var checks = LayDesignTimeModel(context).FindEntityType(typeof(MauChungTu))!.GetCheckConstraints()
            .ToDictionary(c => c.ModelName, c => c.Sql);

        checks.ShouldBe(new Dictionary<string, string>
        {
            ["CK_MauChungTu_TrangThai"] = "[TrangThai] IN (1, 2, 3)",
            ["CK_MauChungTu_TrangThaiTruoc"] = "[TrangThaiTruoc] IN (1, 2, 3)",
        }, ignoreOrder: true);
    }
}
