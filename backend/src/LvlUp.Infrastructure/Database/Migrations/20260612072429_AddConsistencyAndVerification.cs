using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LvlUp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddConsistencyAndVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "verification",
                schema: "lvlup",
                table: "quests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "note",
                schema: "lvlup",
                table: "quest_completions",
                type: "character varying(280)",
                maxLength: 280,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "git_hub_username",
                schema: "lvlup",
                table: "hunters",
                type: "character varying(39)",
                maxLength: 39,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "verification",
                schema: "lvlup",
                table: "quests");

            migrationBuilder.DropColumn(
                name: "note",
                schema: "lvlup",
                table: "quest_completions");

            migrationBuilder.DropColumn(
                name: "git_hub_username",
                schema: "lvlup",
                table: "hunters");
        }
    }
}
