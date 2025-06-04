using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CDAllergies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_cd_character_allergies",
                table: "cd_character_allergies");

            migrationBuilder.DropIndex(
                name: "IX_cd_character_allergies_cdprofile_id",
                table: "cd_character_allergies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cd_character_allergies",
                table: "cd_character_allergies",
                columns: new[] { "cdprofile_id", "allergen" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_cd_character_allergies",
                table: "cd_character_allergies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cd_character_allergies",
                table: "cd_character_allergies",
                column: "allergen");

            migrationBuilder.CreateIndex(
                name: "IX_cd_character_allergies_cdprofile_id",
                table: "cd_character_allergies",
                column: "cdprofile_id");
        }
    }
}
