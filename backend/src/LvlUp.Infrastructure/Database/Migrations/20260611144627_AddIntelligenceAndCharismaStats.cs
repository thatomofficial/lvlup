using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LvlUp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIntelligenceAndCharismaStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Default 10 so existing hunters start at the base stat value.
            migrationBuilder.AddColumn<int>(
                name: "charisma",
                schema: "lvlup",
                table: "hunters",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "intelligence",
                schema: "lvlup",
                table: "hunters",
                type: "integer",
                nullable: false,
                defaultValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "charisma",
                schema: "lvlup",
                table: "hunters");

            migrationBuilder.DropColumn(
                name: "intelligence",
                schema: "lvlup",
                table: "hunters");
        }
    }
}
