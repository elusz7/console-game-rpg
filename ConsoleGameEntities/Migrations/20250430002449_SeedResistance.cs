using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedResistance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills");

            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills");

            migrationBuilder.AlterColumn<int>(
                name: "MonsterId",
                table: "Skills",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ArchetypeId",
                table: "Skills",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Skill_OnlyOneOwner",
                table: "Skills",
                sql: "((\"ArchetypeId\" IS NOT NULL AND \"MonsterId\" IS NULL) OR (\"ArchetypeId\" IS NULL AND \"MonsterId\" IS NOT NULL))");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills",
                column: "ArchetypeId",
                principalTable: "Archetypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills",
                column: "MonsterId",
                principalTable: "Monsters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills");

            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Skill_OnlyOneOwner",
                table: "Skills");

            migrationBuilder.AlterColumn<int>(
                name: "MonsterId",
                table: "Skills",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ArchetypeId",
                table: "Skills",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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
                principalColumn: "Id");
        }
    }
}
