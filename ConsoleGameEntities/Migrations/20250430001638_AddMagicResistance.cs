using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class AddMagicResistance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Health",
                table: "Players",
                newName: "MaxHealth");

            migrationBuilder.RenameColumn(
                name: "Health",
                table: "Monsters",
                newName: "MaxHealth");

            migrationBuilder.RenameColumn(
                name: "CurrentResource",
                table: "Archetypes",
                newName: "ResistanceBonus");

            migrationBuilder.AddColumn<int>(
                name: "MonsterId",
                table: "Skills",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Monsters",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Resistance",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ResistanceMultiplier",
                table: "Archetypes",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_MonsterId",
                table: "Skills",
                column: "MonsterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills",
                column: "MonsterId",
                principalTable: "Monsters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_MonsterId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "MonsterId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "Resistance",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ResistanceMultiplier",
                table: "Archetypes");

            migrationBuilder.RenameColumn(
                name: "MaxHealth",
                table: "Players",
                newName: "Health");

            migrationBuilder.RenameColumn(
                name: "MaxHealth",
                table: "Monsters",
                newName: "Health");

            migrationBuilder.RenameColumn(
                name: "ResistanceBonus",
                table: "Archetypes",
                newName: "CurrentResource");
        }
    }
}
