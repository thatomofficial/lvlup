using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LvlUp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class MoveStatsToOwnTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hunter_stats",
                schema: "lvlup",
                columns: table => new
                {
                    hunter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    strength = table.Column<int>(type: "integer", nullable: false),
                    stamina = table.Column<int>(type: "integer", nullable: false),
                    physique = table.Column<int>(type: "integer", nullable: false),
                    looks = table.Column<int>(type: "integer", nullable: false),
                    well_being = table.Column<int>(type: "integer", nullable: false),
                    intelligence = table.Column<int>(type: "integer", nullable: false),
                    charisma = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hunter_stats", x => x.hunter_id);
                    table.ForeignKey(
                        name: "fk_hunter_stats_hunters_hunter_id",
                        column: x => x.hunter_id,
                        principalSchema: "lvlup",
                        principalTable: "hunters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Move the existing stat values across before the old columns are dropped.
            migrationBuilder.Sql(
                """
                INSERT INTO lvlup.hunter_stats (hunter_id, strength, stamina, physique, looks, well_being, intelligence, charisma)
                SELECT id, strength, stamina, physique, looks, well_being, intelligence, charisma
                FROM lvlup.hunters;
                """);

            migrationBuilder.DropColumn(name: "charisma", schema: "lvlup", table: "hunters");
            migrationBuilder.DropColumn(name: "intelligence", schema: "lvlup", table: "hunters");
            migrationBuilder.DropColumn(name: "looks", schema: "lvlup", table: "hunters");
            migrationBuilder.DropColumn(name: "physique", schema: "lvlup", table: "hunters");
            migrationBuilder.DropColumn(name: "stamina", schema: "lvlup", table: "hunters");
            migrationBuilder.DropColumn(name: "strength", schema: "lvlup", table: "hunters");
            migrationBuilder.DropColumn(name: "well_being", schema: "lvlup", table: "hunters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "charisma", schema: "lvlup", table: "hunters", type: "integer", nullable: false, defaultValue: 10);
            migrationBuilder.AddColumn<int>(name: "intelligence", schema: "lvlup", table: "hunters", type: "integer", nullable: false, defaultValue: 10);
            migrationBuilder.AddColumn<int>(name: "looks", schema: "lvlup", table: "hunters", type: "integer", nullable: false, defaultValue: 10);
            migrationBuilder.AddColumn<int>(name: "physique", schema: "lvlup", table: "hunters", type: "integer", nullable: false, defaultValue: 10);
            migrationBuilder.AddColumn<int>(name: "stamina", schema: "lvlup", table: "hunters", type: "integer", nullable: false, defaultValue: 10);
            migrationBuilder.AddColumn<int>(name: "strength", schema: "lvlup", table: "hunters", type: "integer", nullable: false, defaultValue: 10);
            migrationBuilder.AddColumn<int>(name: "well_being", schema: "lvlup", table: "hunters", type: "integer", nullable: false, defaultValue: 10);

            // Copy the stat values back onto the hunters table before dropping the stats table.
            migrationBuilder.Sql(
                """
                UPDATE lvlup.hunters h
                SET strength = s.strength,
                    stamina = s.stamina,
                    physique = s.physique,
                    looks = s.looks,
                    well_being = s.well_being,
                    intelligence = s.intelligence,
                    charisma = s.charisma
                FROM lvlup.hunter_stats s
                WHERE s.hunter_id = h.id;
                """);

            migrationBuilder.DropTable(
                name: "hunter_stats",
                schema: "lvlup");
        }
    }
}
