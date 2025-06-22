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
            migrationBuilder.DropIndex(
                name: "IX_cd_character_allergies_allergen",
                table: "cd_character_allergies");

            migrationBuilder.CreateIndex(
                name: "IX_cd_character_allergies_cdprofile_id_allergen",
                table: "cd_character_allergies",
                columns: new[] { "cdprofile_id", "allergen" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cd_character_allergies_cdprofile_id_allergen",
                table: "cd_character_allergies");

            migrationBuilder.CreateIndex(
                name: "IX_cd_character_allergies_allergen",
                table: "cd_character_allergies",
                column: "allergen");
        }
    }
}
