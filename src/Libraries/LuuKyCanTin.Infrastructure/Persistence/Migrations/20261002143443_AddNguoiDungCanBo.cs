using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNguoiDungCanBo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CanBoId",
                table: "NguoiDung",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_NguoiDung_CanBoId_DangHoatDong",
                table: "NguoiDung",
                column: "CanBoId",
                unique: true,
                filter: "[DangHoatDong] = 1 AND [CanBoId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NguoiDung_CanBoId",
                table: "NguoiDung",
                sql: "[CanBoId] IS NOT NULL OR [TenDangNhap] = 'admin'");

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_CanBo_CanBoId",
                table: "NguoiDung",
                column: "CanBoId",
                principalTable: "CanBo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_CanBo_CanBoId",
                table: "NguoiDung");

            migrationBuilder.DropIndex(
                name: "UX_NguoiDung_CanBoId_DangHoatDong",
                table: "NguoiDung");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NguoiDung_CanBoId",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "CanBoId",
                table: "NguoiDung");
        }
    }
}
