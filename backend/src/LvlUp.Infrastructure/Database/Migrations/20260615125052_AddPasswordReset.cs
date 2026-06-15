using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LvlUp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "password_reset_code_expires_at_utc",
                schema: "lvlup",
                table: "hunters",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_reset_code_hash",
                schema: "lvlup",
                table: "hunters",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "password_reset_failed_attempts",
                schema: "lvlup",
                table: "hunters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password_reset_code_expires_at_utc",
                schema: "lvlup",
                table: "hunters");

            migrationBuilder.DropColumn(
                name: "password_reset_code_hash",
                schema: "lvlup",
                table: "hunters");

            migrationBuilder.DropColumn(
                name: "password_reset_failed_attempts",
                schema: "lvlup",
                table: "hunters");
        }
    }
}
