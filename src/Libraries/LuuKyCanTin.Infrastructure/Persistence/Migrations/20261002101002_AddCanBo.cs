using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCanBo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CanBo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCanBo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Latin1_General_100_CI_AI"),
                    ChucVu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LaQuanGiao = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DangCongTac = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: false),
                    NgaySua = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NguoiSuaId = table.Column<int>(type: "int", nullable: true),
                    RowVer = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanBo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CanBo_HoTen",
                table: "CanBo",
                column: "HoTen");

            migrationBuilder.CreateIndex(
                name: "IX_CanBo_MaCanBo",
                table: "CanBo",
                column: "MaCanBo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanBo");
        }
    }
}
