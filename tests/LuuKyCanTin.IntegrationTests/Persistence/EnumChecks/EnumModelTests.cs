using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Domain.LuuKy;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

// Model-only: kept out of the SQL Server collection so they run, and pass, without a server.
public sealed class EnumModelTests
{
    private static readonly DbContextOptions<AppDbContext> ModelOnlyOptions = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    [Fact]
    public void EveryEnum_IsDeclaredByte()
    {
        using var db = new AppDbContext(ModelOnlyOptions);

        EnumModel.LayCotEnum(db)
            .Where(c => Enum.GetUnderlyingType(c.EnumType) != typeof(byte))
            .Select(c => $"{c.EnumType.Name} is not declared ': byte'")
            .ShouldBeEmpty();
    }

    [Fact]
    public void LayCotEnum_NumericEnumColumn_IsTinyint()
    {
        using var db = new AppDbContext(ModelOnlyOptions);

        EnumModel.LayThuocTinhEnum(db)
            .Where(p => !EnumModel.IsStoredAsName(p) && p.GetColumnType() != "tinyint")
            .Select(p => $"{p.DeclaringType.DisplayName()}.{p.Name} is {p.GetColumnType()}; declare the enum ': byte'")
            .ShouldBeEmpty();
    }

    [Fact]
    public void LayThuocTinhEnum_NullableAndNonNullableEnums_FindsBoth()
    {
        using var db = new TestAppDbContext(ModelOnlyOptions);

        EnumModel.LayCotEnum(db).ShouldBe(
        [
            new EnumColumn(EnumModel.DefaultSchema, "MauChungTu", "TrangThai", typeof(MauTrangThai)),
            new EnumColumn(EnumModel.DefaultSchema, "MauChungTu", "TrangThaiTruoc", typeof(MauTrangThai)),
            new EnumColumn(EnumModel.DefaultSchema, "NhatKyThaoTac", "HanhDong", typeof(HanhDong), StoredAsName: true),
            new EnumColumn(EnumModel.DefaultSchema, "DoiTuong", "LoaiDoiTuong", typeof(LoaiDoiTuong)),
            new EnumColumn(EnumModel.DefaultSchema, "DoiTuong", "TrangThai", typeof(TrangThaiDoiTuong)),
            new EnumColumn(EnumModel.DefaultSchema, "ChungTuLuuKy", "LoaiPhieu", typeof(LoaiPhieu)),
            new EnumColumn(EnumModel.DefaultSchema, "ChungTuLuuKy", "NghiepVu", typeof(NghiepVu)),
            new EnumColumn(EnumModel.DefaultSchema, "ChungTuLuuKy", "HinhThuc", typeof(HinhThuc)),
            new EnumColumn(EnumModel.DefaultSchema, "ChungTuLuuKy", "TrangThai", typeof(TrangThaiChungTu)),
            new EnumColumn(EnumModel.DefaultSchema, "ChungTuLuuKy", "LoaiDoiTuong", typeof(LoaiDoiTuong)),
        ], ignoreOrder: true);
    }
}
