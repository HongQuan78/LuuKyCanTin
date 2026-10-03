using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOfficer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OfficerId",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_User_OfficerId_IsActive",
                table: "User",
                column: "OfficerId",
                unique: true,
                filter: "[IsActive] = 1 AND [OfficerId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_OfficerId",
                table: "User",
                sql: "[OfficerId] IS NOT NULL OR [UserName] = 'admin'");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Officer_OfficerId",
                table: "User",
                column: "OfficerId",
                principalTable: "Officer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Officer_OfficerId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "UX_User_OfficerId_IsActive",
                table: "User");

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_OfficerId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "OfficerId",
                table: "User");
        }
    }
}
