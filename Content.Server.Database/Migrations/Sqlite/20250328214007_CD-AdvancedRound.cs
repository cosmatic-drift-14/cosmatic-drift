using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CDAdvancedRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "advanced_round",
                columns: table => new
                {
                    advanced_round_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    server_id = table.Column<int>(type: "INTEGER", nullable: false),
                    round_id = table.Column<int>(type: "INTEGER", nullable: false),
                    map = table.Column<string>(type: "TEXT", nullable: false),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advanced_round", x => x.advanced_round_id);
                });

            migrationBuilder.CreateTable(
                name: "advanced_round_player",
                columns: table => new
                {
                    advanced_rounds_id = table.Column<int>(type: "INTEGER", nullable: false),
                    players_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advanced_round_player", x => new { x.advanced_rounds_id, x.players_id });
                    table.ForeignKey(
                        name: "FK_advanced_round_player_advanced_round_advanced_rounds_id",
                        column: x => x.advanced_rounds_id,
                        principalTable: "advanced_round",
                        principalColumn: "advanced_round_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_advanced_round_player_player_players_id",
                        column: x => x.players_id,
                        principalTable: "player",
                        principalColumn: "player_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_advanced_round_start_date",
                table: "advanced_round",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "IX_advanced_round_player_players_id",
                table: "advanced_round_player",
                column: "players_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advanced_round_player");

            migrationBuilder.DropTable(
                name: "advanced_round");
        }
    }
}
