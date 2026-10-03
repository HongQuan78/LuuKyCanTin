using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.DanhMuc;

internal sealed class DoiTuongConfiguration : AuditableEntityConfiguration<DoiTuong>
{
    protected override void CauHinhRieng(EntityTypeBuilder<DoiTuong> builder)
    {
        builder.ToTable("DoiTuong", t =>
        {
            t.HasCheckConstraint("CK_DoiTuong_SoDuLuuKy", "[SoDuLuuKy] >= 0");
            t.HasCheckConstraint("CK_DoiTuong_NgayRa", "[TrangThai] = 1 OR [NgayRa] IS NOT NULL");
        });

        builder.Property(e => e.MaSo).HasMaxLength(30).IsUnicode(false).IsRequired();
        builder.HasIndex(e => e.MaSo).IsUnique();

        builder.Property(e => e.HoTen).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.HoTen);

        builder.Property(e => e.NamSinh).HasColumnType("smallint");
        builder.Property(e => e.BuongGiam).HasMaxLength(50);

        // The sentinel is the default itself; a new detainee explicitly set to DangQuanLy still stores 1.
        builder.Property(e => e.TrangThai).HasDefaultValue(TrangThaiDoiTuong.DangQuanLy).HasSentinel(TrangThaiDoiTuong.DangQuanLy);
        builder.HasEnumCheck(e => e.TrangThai);
        builder.HasEnumCheck(e => e.LoaiDoiTuong);

        // The balance is only ever written by the ledger engine's raw conditional UPDATE. Ignoring the
        // property on both saves makes that rule structural: EF never emits this column in an INSERT/UPDATE.
        var soDu = builder.Property(e => e.SoDuLuuKy).HasPrecision(18, 0).HasDefaultValue(0m);
        soDu.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        soDu.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
