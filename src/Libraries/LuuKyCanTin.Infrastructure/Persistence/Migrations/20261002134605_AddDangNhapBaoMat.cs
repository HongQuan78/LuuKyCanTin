using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDangNhapBaoMat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PhaiDoiMatKhau",
                table: "NguoiDung",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // The seeded admin must set its own password before reaching the shell (FR1).
            migrationBuilder.Sql("UPDATE NguoiDung SET PhaiDoiMatKhau = 1 WHERE TenDangNhap = 'admin'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhaiDoiMatKhau",
                table: "NguoiDung");
        }
    }
}
