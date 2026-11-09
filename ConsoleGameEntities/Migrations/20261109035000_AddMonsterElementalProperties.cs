using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    /// <inheritdoc />
    public partial class AddMonsterElementalProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttackElement",
                table: "Monsters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ElementalPower",
                table: "Monsters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Vulnerability",
                table: "Monsters",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttackElement",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "ElementalPower",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "Vulnerability",
                table: "Monsters");
        }
    }
}
