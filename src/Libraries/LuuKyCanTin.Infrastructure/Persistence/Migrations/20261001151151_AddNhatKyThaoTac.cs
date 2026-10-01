using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNhatKyThaoTac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NhatKyThaoTac",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThoiDiem = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    NguoiDungId = table.Column<int>(type: "int", nullable: true),
                    MayTram = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HanhDong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenBang = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    BanGhiId = table.Column<long>(type: "bigint", nullable: true),
                    DuLieuCu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuLieuMoi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyThaoTac", x => x.Id);
                    table.CheckConstraint("CK_NhatKyThaoTac_HanhDong", "[HanhDong] IN ('Them', 'Sua', 'Huy', 'In', 'Duyet', 'DangNhap')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyThaoTac_TenBang_BanGhiId",
                table: "NhatKyThaoTac",
                columns: new[] { "TenBang", "BanGhiId" });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyThaoTac_ThoiDiem",
                table: "NhatKyThaoTac",
                column: "ThoiDiem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NhatKyThaoTac");
        }
    }
}
