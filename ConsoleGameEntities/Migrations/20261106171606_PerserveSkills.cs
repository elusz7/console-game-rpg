using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    /// <inheritdoc />
    public partial class PerserveSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills");

            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills",
                column: "ArchetypeId",
                principalTable: "Archetypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills",
                column: "MonsterId",
                principalTable: "Monsters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills");

            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills",
                column: "ArchetypeId",
                principalTable: "Archetypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills",
                column: "MonsterId",
                principalTable: "Monsters",
                principalColumn: "Id");
        }
    }
}
