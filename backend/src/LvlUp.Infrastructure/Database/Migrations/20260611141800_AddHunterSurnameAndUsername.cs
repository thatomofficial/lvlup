using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LvlUp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddHunterSurnameAndUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "display_name_preference",
                schema: "lvlup",
                table: "hunters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "surname",
                schema: "lvlup",
                table: "hunters",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "username",
                schema: "lvlup",
                table: "hunters",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            // Backfill existing hunters with a unique username derived from their email
            // before the unique index is created.
            migrationBuilder.Sql(
                """
                UPDATE lvlup.hunters
                SET username = left(regexp_replace(split_part(email, '@', 1), '[^a-zA-Z0-9_]', '_', 'g'), 21)
                               || '_' || left(replace(id::text, '-', ''), 8)
                WHERE username = '';
                """);

            migrationBuilder.CreateIndex(
                name: "ix_hunters_username",
                schema: "lvlup",
                table: "hunters",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_hunters_username",
                schema: "lvlup",
                table: "hunters");

            migrationBuilder.DropColumn(
                name: "display_name_preference",
                schema: "lvlup",
                table: "hunters");

            migrationBuilder.DropColumn(
                name: "surname",
                schema: "lvlup",
                table: "hunters");

            migrationBuilder.DropColumn(
                name: "username",
                schema: "lvlup",
                table: "hunters");
        }
    }
}
