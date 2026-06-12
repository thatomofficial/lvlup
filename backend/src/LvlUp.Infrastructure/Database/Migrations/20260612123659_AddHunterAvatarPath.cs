using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LvlUp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddHunterAvatarPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_path",
                schema: "lvlup",
                table: "hunters",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_path",
                schema: "lvlup",
                table: "hunters");
        }
    }
}
