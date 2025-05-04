using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class MonsterAndSkillUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills");

            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Sneakiness",
                table: "Monsters");

            migrationBuilder.RenameColumn(
                name: "ESkillType",
                table: "Skills",
                newName: "SkillCategory");

            migrationBuilder.RenameColumn(
                name: "MartialBonus",
                table: "Monsters",
                newName: "ThreatLevel");

            migrationBuilder.RenameColumn(
                name: "MagicBonus",
                table: "Monsters",
                newName: "DamageType");

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

            migrationBuilder.AddColumn<int>(
                name: "DamageType",
                table: "Skills",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills");

            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Monsters_MonsterId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "DamageType",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "DamageType",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "DesiredHitsToBeKilledByPlayer",
                table: "Monsters");

            migrationBuilder.RenameColumn(
                name: "SkillCategory",
                table: "Skills",
                newName: "ESkillType");

            migrationBuilder.RenameColumn(
                name: "ThreatLevel",
                table: "Monsters",
                newName: "MartialBonus");

            migrationBuilder.RenameColumn(
                name: "DesiredHitsToKillPlayer",
                table: "Monsters",
                newName: "MagicBonus");

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

            migrationBuilder.AddColumn<int>(
                name: "Sneakiness",
                table: "Monsters",
                type: "int",
                nullable: true);

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
    }
}
