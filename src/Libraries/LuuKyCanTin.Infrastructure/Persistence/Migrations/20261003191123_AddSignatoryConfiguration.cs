using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSignatoryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignatoryConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Ordinal = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OfficerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignatoryConfiguration", x => x.Id);
                    table.CheckConstraint("CK_SignatoryConfiguration_Ordinal", "[Ordinal] >= 1");
                    table.CheckConstraint("CK_SignatoryConfiguration_TemplateCode", "[TemplateCode] IN ('BIEN_NHAN_THU', 'PHIEU_CHI', 'BANG_KE_CA_NHAN', 'SO_THEO_DOI', 'BANG_KE_NOP', 'PHIEU_NHAP', 'PHIEU_MUA_HANG', 'BAO_CAO_NXT', 'BAO_CAO_DOANH_THU', 'BANG_NIEM_YET_GIA', 'THEO_DOI_MUA_HANG', 'DANH_MUC_HANG_HOA')");
                    table.ForeignKey(
                        name: "FK_SignatoryConfiguration_Officer_OfficerId",
                        column: x => x.OfficerId,
                        principalTable: "Officer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SignatoryConfiguration",
                columns: new[] { "Id", "OfficerId", "Ordinal", "TemplateCode", "Title" },
                values: new object[,]
                {
                    { 1, null, (byte)1, "BIEN_NHAN_THU", "Người gửi" },
                    { 2, null, (byte)2, "BIEN_NHAN_THU", "Người nhận" },
                    { 3, null, (byte)3, "BIEN_NHAN_THU", "Lãnh đạo đơn vị" },
                    { 4, null, (byte)1, "PHIEU_CHI", "Cán bộ theo dõi tiền lưu ký" },
                    { 5, null, (byte)2, "PHIEU_CHI", "Người bị tạm giữ, tạm giam/phạm nhân xác nhận" },
                    { 6, null, (byte)3, "PHIEU_CHI", "Cán bộ quản giáo xác nhận" },
                    { 7, null, (byte)4, "PHIEU_CHI", "Lãnh đạo đơn vị xác nhận" },
                    { 8, null, (byte)1, "BANG_KE_CA_NHAN", "Cán bộ căn tin" },
                    { 9, null, (byte)2, "BANG_KE_CA_NHAN", "Cán bộ quản giáo" },
                    { 10, null, (byte)3, "BANG_KE_CA_NHAN", "Người bị tạm giữ, tạm giam/phạm nhân" },
                    { 11, null, (byte)4, "BANG_KE_CA_NHAN", "Thủ trưởng đơn vị" },
                    { 12, null, (byte)1, "SO_THEO_DOI", "Cán bộ căn tin" },
                    { 13, null, (byte)2, "SO_THEO_DOI", "Chỉ huy phụ trách" },
                    { 14, null, (byte)3, "SO_THEO_DOI", "Kế toán đơn vị" },
                    { 15, null, (byte)4, "SO_THEO_DOI", "Thủ trưởng đơn vị" },
                    { 16, null, (byte)1, "BANG_KE_NOP", "Người nộp" },
                    { 17, null, (byte)2, "BANG_KE_NOP", "Chỉ huy phụ trách" },
                    { 18, null, (byte)3, "BANG_KE_NOP", "Thủ trưởng đơn vị" },
                    { 19, null, (byte)1, "PHIEU_NHAP", "Người giao" },
                    { 20, null, (byte)2, "PHIEU_NHAP", "Người nhận" },
                    { 21, null, (byte)3, "PHIEU_NHAP", "Chỉ huy đội" },
                    { 22, null, (byte)4, "PHIEU_NHAP", "Lãnh đạo đơn vị" },
                    { 23, null, (byte)1, "PHIEU_MUA_HANG", "Người mua hàng" },
                    { 24, null, (byte)2, "PHIEU_MUA_HANG", "Cán bộ căn tin" },
                    { 25, null, (byte)3, "PHIEU_MUA_HANG", "Lãnh đạo đơn vị" },
                    { 26, null, (byte)1, "BAO_CAO_NXT", "Cán bộ bán hàng" },
                    { 27, null, (byte)2, "BAO_CAO_NXT", "Chỉ huy phụ trách" },
                    { 28, null, (byte)3, "BAO_CAO_NXT", "Lãnh đạo đơn vị" },
                    { 29, null, (byte)1, "BAO_CAO_DOANH_THU", "Cán bộ căn tin" },
                    { 30, null, (byte)2, "BAO_CAO_DOANH_THU", "Chỉ huy phụ trách" },
                    { 31, null, (byte)3, "BAO_CAO_DOANH_THU", "Lãnh đạo đơn vị" },
                    { 32, null, (byte)1, "BANG_NIEM_YET_GIA", "Cán bộ căn tin" },
                    { 33, null, (byte)2, "BANG_NIEM_YET_GIA", "Lãnh đạo đơn vị" },
                    { 34, null, (byte)1, "THEO_DOI_MUA_HANG", "Cán bộ căn tin" },
                    { 35, null, (byte)2, "THEO_DOI_MUA_HANG", "Chỉ huy phụ trách" },
                    { 36, null, (byte)3, "THEO_DOI_MUA_HANG", "Lãnh đạo đơn vị" },
                    { 37, null, (byte)1, "DANH_MUC_HANG_HOA", "Cán bộ căn tin" },
                    { 38, null, (byte)2, "DANH_MUC_HANG_HOA", "Chỉ huy phụ trách" },
                    { 39, null, (byte)3, "DANH_MUC_HANG_HOA", "Lãnh đạo đơn vị" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignatoryConfiguration_OfficerId",
                table: "SignatoryConfiguration",
                column: "OfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_SignatoryConfiguration_TemplateCode_Ordinal",
                table: "SignatoryConfiguration",
                columns: new[] { "TemplateCode", "Ordinal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignatoryConfiguration");
        }
    }
}
