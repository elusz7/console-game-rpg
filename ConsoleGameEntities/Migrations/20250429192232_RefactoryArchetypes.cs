using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class RefactoryArchetypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills");

            migrationBuilder.AlterColumn<int>(
                name: "ArchetypeType",
                table: "Archetypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AttackBonus",
                table: "Archetypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AttackMultiplier",
                table: "Archetypes",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DefenseMultiplier",
                table: "Archetypes",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "MagicBonus",
                table: "Archetypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MagicMultiplier",
                table: "Archetypes",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RecoveryGrowth",
                table: "Archetypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ResourceMultiplier",
                table: "Archetypes",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ResourceName",
                table: "Archetypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SpeedMultiplier",
                table: "Archetypes",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills",
                column: "ArchetypeId",
                principalTable: "Archetypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("UPDATE Players SET Level = 1 WHERE Id = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "AttackBonus",
                table: "Archetypes");

            migrationBuilder.DropColumn(
                name: "AttackMultiplier",
                table: "Archetypes");

            migrationBuilder.DropColumn(
                name: "DefenseMultiplier",
                table: "Archetypes");

            migrationBuilder.DropColumn(
                name: "MagicBonus",
                table: "Archetypes");

            migrationBuilder.DropColumn(
                name: "MagicMultiplier",
                table: "Archetypes");

            migrationBuilder.DropColumn(
                name: "RecoveryGrowth",
                table: "Archetypes");

            migrationBuilder.DropColumn(
                name: "ResourceMultiplier",
                table: "Archetypes");

            migrationBuilder.DropColumn(
                name: "ResourceName",
                table: "Archetypes");

            migrationBuilder.DropColumn(
                name: "SpeedMultiplier",
                table: "Archetypes");

            migrationBuilder.AlterColumn<int>(
                name: "ArchetypeId",
                table: "Skills",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ArchetypeType",
                table: "Archetypes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Archetypes_ArchetypeId",
                table: "Skills",
                column: "ArchetypeId",
                principalTable: "Archetypes",
                principalColumn: "Id");
        }
    }
}
