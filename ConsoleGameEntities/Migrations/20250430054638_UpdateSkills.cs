using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class UpdateSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttackPower",
                table: "Monsters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefensePower",
                table: "Monsters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Resistance",
                table: "Monsters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Skills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.RenameColumn(
                name: "SkillType",
                table: "Skills",
                newName: "ESkillType");

            migrationBuilder.AddColumn<string>(
                name: "SkillType",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "StatAffected",
                table: "Skills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Effect",
                table: "Skills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttackPower",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "DefensePower",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "Resistance",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "StatAffected",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Effect",
                table: "Skills");
        }
    }
}
