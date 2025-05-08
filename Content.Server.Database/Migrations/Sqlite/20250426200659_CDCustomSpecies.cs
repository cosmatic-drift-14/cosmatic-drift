﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CDCustomSpecies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "custom_species_name",
                table: "cdprofile",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "custom_species_name",
                table: "cdprofile");
        }
    }
}
