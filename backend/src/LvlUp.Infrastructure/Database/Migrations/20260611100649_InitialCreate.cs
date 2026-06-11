using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LvlUp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "lvlup");

            migrationBuilder.CreateTable(
                name: "hunters",
                schema: "lvlup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    current_xp = table.Column<int>(type: "integer", nullable: false),
                    total_xp = table.Column<int>(type: "integer", nullable: false),
                    strength = table.Column<int>(type: "integer", nullable: false),
                    stamina = table.Column<int>(type: "integer", nullable: false),
                    physique = table.Column<int>(type: "integer", nullable: false),
                    looks = table.Column<int>(type: "integer", nullable: false),
                    well_being = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hunters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quest_completions",
                schema: "lvlup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hunter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    xp_awarded = table.Column<int>(type: "integer", nullable: false),
                    stat_awarded = table.Column<int>(type: "integer", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quest_completions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quests",
                schema: "lvlup",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hunter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quests", x => x.id);
                    table.ForeignKey(
                        name: "fk_quests_hunters_hunter_id",
                        column: x => x.hunter_id,
                        principalSchema: "lvlup",
                        principalTable: "hunters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hunters_email",
                schema: "lvlup",
                table: "hunters",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quest_completions_hunter_id",
                schema: "lvlup",
                table: "quest_completions",
                column: "hunter_id");

            migrationBuilder.CreateIndex(
                name: "ix_quests_hunter_id",
                schema: "lvlup",
                table: "quests",
                column: "hunter_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quest_completions",
                schema: "lvlup");

            migrationBuilder.DropTable(
                name: "quests",
                schema: "lvlup");

            migrationBuilder.DropTable(
                name: "hunters",
                schema: "lvlup");
        }
    }
}
