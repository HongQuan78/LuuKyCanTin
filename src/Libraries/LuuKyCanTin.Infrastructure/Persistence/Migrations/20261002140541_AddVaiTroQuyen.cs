using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVaiTroQuyen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quyen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Ma = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Module = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quyen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: false),
                    NgaySua = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NguoiSuaId = table.Column<int>(type: "int", nullable: true),
                    RowVer = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungVaiTro",
                columns: table => new
                {
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    VaiTroId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungVaiTro", x => new { x.NguoiDungId, x.VaiTroId });
                    table.ForeignKey(
                        name: "FK_NguoiDungVaiTro_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NguoiDungVaiTro_VaiTro_VaiTroId",
                        column: x => x.VaiTroId,
                        principalTable: "VaiTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VaiTroQuyen",
                columns: table => new
                {
                    VaiTroId = table.Column<int>(type: "int", nullable: false),
                    QuyenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTroQuyen", x => new { x.VaiTroId, x.QuyenId });
                    table.ForeignKey(
                        name: "FK_VaiTroQuyen_Quyen_QuyenId",
                        column: x => x.QuyenId,
                        principalTable: "Quyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VaiTroQuyen_VaiTro_VaiTroId",
                        column: x => x.VaiTroId,
                        principalTable: "VaiTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Quyen",
                columns: new[] { "Id", "Ma", "Module", "Ten" },
                values: new object[,]
                {
                    { 1, "HT.Xem", "HT", "Xem — Hệ thống" },
                    { 2, "HT.Them", "HT", "Them — Hệ thống" },
                    { 3, "HT.Sua", "HT", "Sua — Hệ thống" },
                    { 4, "HT.Huy", "HT", "Huy — Hệ thống" },
                    { 5, "HT.In", "HT", "In — Hệ thống" },
                    { 6, "HT.Duyet", "HT", "Duyet — Hệ thống" },
                    { 7, "DM.Xem", "DM", "Xem — Danh mục" },
                    { 8, "DM.Them", "DM", "Them — Danh mục" },
                    { 9, "DM.Sua", "DM", "Sua — Danh mục" },
                    { 10, "DM.Huy", "DM", "Huy — Danh mục" },
                    { 11, "DM.In", "DM", "In — Danh mục" },
                    { 12, "DM.Duyet", "DM", "Duyet — Danh mục" },
                    { 13, "LK-T.Xem", "LK-T", "Xem — Tăng tiền lưu ký" },
                    { 14, "LK-T.Them", "LK-T", "Them — Tăng tiền lưu ký" },
                    { 15, "LK-T.Sua", "LK-T", "Sua — Tăng tiền lưu ký" },
                    { 16, "LK-T.Huy", "LK-T", "Huy — Tăng tiền lưu ký" },
                    { 17, "LK-T.In", "LK-T", "In — Tăng tiền lưu ký" },
                    { 18, "LK-T.Duyet", "LK-T", "Duyet — Tăng tiền lưu ký" },
                    { 19, "LK-C.Xem", "LK-C", "Xem — Giảm tiền lưu ký" },
                    { 20, "LK-C.Them", "LK-C", "Them — Giảm tiền lưu ký" },
                    { 21, "LK-C.Sua", "LK-C", "Sua — Giảm tiền lưu ký" },
                    { 22, "LK-C.Huy", "LK-C", "Huy — Giảm tiền lưu ký" },
                    { 23, "LK-C.In", "LK-C", "In — Giảm tiền lưu ký" },
                    { 24, "LK-C.Duyet", "LK-C", "Duyet — Giảm tiền lưu ký" },
                    { 25, "LK-BC.Xem", "LK-BC", "Xem — Báo cáo lưu ký" },
                    { 26, "LK-BC.Them", "LK-BC", "Them — Báo cáo lưu ký" },
                    { 27, "LK-BC.Sua", "LK-BC", "Sua — Báo cáo lưu ký" },
                    { 28, "LK-BC.Huy", "LK-BC", "Huy — Báo cáo lưu ký" },
                    { 29, "LK-BC.In", "LK-BC", "In — Báo cáo lưu ký" },
                    { 30, "LK-BC.Duyet", "LK-BC", "Duyet — Báo cáo lưu ký" },
                    { 31, "NH.Xem", "NH", "Xem — Nhập hàng" },
                    { 32, "NH.Them", "NH", "Them — Nhập hàng" },
                    { 33, "NH.Sua", "NH", "Sua — Nhập hàng" },
                    { 34, "NH.Huy", "NH", "Huy — Nhập hàng" },
                    { 35, "NH.In", "NH", "In — Nhập hàng" },
                    { 36, "NH.Duyet", "NH", "Duyet — Nhập hàng" },
                    { 37, "BH.Xem", "BH", "Xem — Bán hàng" },
                    { 38, "BH.Them", "BH", "Them — Bán hàng" },
                    { 39, "BH.Sua", "BH", "Sua — Bán hàng" },
                    { 40, "BH.Huy", "BH", "Huy — Bán hàng" },
                    { 41, "BH.In", "BH", "In — Bán hàng" },
                    { 42, "BH.Duyet", "BH", "Duyet — Bán hàng" },
                    { 43, "HH-BC.Xem", "HH-BC", "Xem — Báo cáo hàng hóa" },
                    { 44, "HH-BC.Them", "HH-BC", "Them — Báo cáo hàng hóa" },
                    { 45, "HH-BC.Sua", "HH-BC", "Sua — Báo cáo hàng hóa" },
                    { 46, "HH-BC.Huy", "HH-BC", "Huy — Báo cáo hàng hóa" },
                    { 47, "HH-BC.In", "HH-BC", "In — Báo cáo hàng hóa" },
                    { 48, "HH-BC.Duyet", "HH-BC", "Duyet — Báo cáo hàng hóa" }
                });

            migrationBuilder.InsertData(
                table: "VaiTro",
                columns: new[] { "Id", "Ma", "NgaySua", "NgayTao", "NguoiSuaId", "NguoiTaoId", "Ten" },
                values: new object[,]
                {
                    { 1, "QUAN_TRI", null, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, "Quản trị hệ thống" },
                    { 2, "LUU_KY", null, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, "Cán bộ theo dõi tiền lưu ký" },
                    { 3, "CAN_TIN", null, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, "Cán bộ căn tin / bán hàng" },
                    { 4, "QUAN_GIAO", null, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, "Cán bộ quản giáo" },
                    { 5, "LANH_DAO", null, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, "Chỉ huy phụ trách / Lãnh đạo đơn vị" },
                    { 6, "KE_TOAN", null, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0, "Kế toán đơn vị" }
                });

            migrationBuilder.InsertData(
                table: "VaiTroQuyen",
                columns: new[] { "QuyenId", "VaiTroId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 11, 1 },
                    { 12, 1 },
                    { 7, 2 },
                    { 13, 2 },
                    { 14, 2 },
                    { 15, 2 },
                    { 16, 2 },
                    { 17, 2 },
                    { 19, 2 },
                    { 20, 2 },
                    { 21, 2 },
                    { 22, 2 },
                    { 23, 2 },
                    { 25, 2 },
                    { 26, 2 },
                    { 27, 2 },
                    { 28, 2 },
                    { 29, 2 },
                    { 25, 3 },
                    { 31, 3 },
                    { 32, 3 },
                    { 33, 3 },
                    { 34, 3 },
                    { 35, 3 },
                    { 36, 3 },
                    { 37, 3 },
                    { 38, 3 },
                    { 39, 3 },
                    { 40, 3 },
                    { 41, 3 },
                    { 42, 3 },
                    { 43, 3 },
                    { 44, 3 },
                    { 45, 3 },
                    { 46, 3 },
                    { 47, 3 },
                    { 48, 3 },
                    { 25, 4 },
                    { 1, 5 },
                    { 18, 5 },
                    { 24, 5 },
                    { 25, 5 },
                    { 36, 5 },
                    { 43, 5 },
                    { 25, 6 },
                    { 43, 6 }
                });

            // The admin seeded by AddWalkingSkeletonTables (id 1) administers the system from day one. Written
            // here, not as HasData, because only migrations seed that account (Story 2.3 T2).
            migrationBuilder.InsertData(
                table: "NguoiDungVaiTro",
                columns: new[] { "NguoiDungId", "VaiTroId" },
                values: new object[] { 1, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungVaiTro_VaiTroId",
                table: "NguoiDungVaiTro",
                column: "VaiTroId");

            migrationBuilder.CreateIndex(
                name: "IX_Quyen_Ma",
                table: "Quyen",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_Ma",
                table: "VaiTro",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaiTroQuyen_QuyenId",
                table: "VaiTroQuyen",
                column: "QuyenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NguoiDungVaiTro");

            migrationBuilder.DropTable(
                name: "VaiTroQuyen");

            migrationBuilder.DropTable(
                name: "Quyen");

            migrationBuilder.DropTable(
                name: "VaiTro");
        }
    }
}
