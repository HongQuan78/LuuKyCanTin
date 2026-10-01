using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuuKyCanTin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF only applies the model collation when Migrate itself creates the database. An admin may
            // create the empty database beforehand, so set it explicitly while no column depends on it yet.
            migrationBuilder.AlterDatabase(collation: "Vietnamese_CI_AI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The previous collation is unknown; leaving it is harmless because no table exists at this point.
        }
    }
}
