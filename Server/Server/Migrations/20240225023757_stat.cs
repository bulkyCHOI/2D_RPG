using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class stat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DEX",
                table: "Player");

            migrationBuilder.RenameColumn(
                name: "STR",
                table: "Player",
                newName: "defence");

            migrationBuilder.RenameColumn(
                name: "INT",
                table: "Player",
                newName: "attack");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "defence",
                table: "Player",
                newName: "STR");

            migrationBuilder.RenameColumn(
                name: "attack",
                table: "Player",
                newName: "INT");

            migrationBuilder.AddColumn<int>(
                name: "DEX",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
