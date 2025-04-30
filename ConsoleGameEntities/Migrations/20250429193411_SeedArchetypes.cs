using Microsoft.EntityFrameworkCore.Migrations;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedArchetypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT Archetypes ON;");
            migrationBuilder.InsertData(
                table: "Archetypes",
                columns: new[] { "Id", "Name", "Description", "ArchetypeType", "HealthBase", "AttackBonus", "AttackMultiplier", "MagicBonus", "MagicMultiplier", "DefenseBonus", "DefenseMultiplier", "Speed", "SpeedMultiplier", "ResourceName", "CurrentResource", "MaxResource", "ResourceMultiplier", "RecoveryRate", "RecoveryGrowth" },
                values: new object[,]
                {
                  //   Id  Name       Description                                                                                      ArchetypeType         Health  AtkB  AtkM   MagB  MagM   DefB  DefM   Spd  SpdM   Resource   CurrRes  MaxRes  ResM   RecRate  RecGrow
                    { 1,  "Warrior",  "The human equivalent of a Swiss Army knife—predictable, dependable, mildly threatening.",      ArchetypeType.Martial,  10,     3,    0.4M,  0,    0M,    2,    0.6M,  2,   0.2M,  "Stamina",   5,       5,      0.3M,  2,        2 },
                    { 2,  "Barbarian","Muscle-first problem-solver. Brain sold separately.",                                          ArchetypeType.Martial,  12,     3,    0.6M,  0,    0M,    2,    0.2M,  1,   0.4M,  "Stamina",   4,       4,      0.4M,  1,        2 },
                    { 3,  "Rogue",    "Fast, sneaky, and slightly allergic to fair fights.",                                          ArchetypeType.Martial,   8,     2,    0.2M,  0,    0M,    3,    0.4M,  3,   0.6M,  "Stamina",   8,       8,      0.5M,  1,        1 },
                    { 4,  "Mage",     "Soft as a marshmallow, but throws lightning like Zeus with a grudge.",                        ArchetypeType.Magical,   6,     0,    0M,    3,    0.5M,  2,    0.2M,  3,   0.4M,  "Mana",      8,       8,      0.5M,  1,        1 },
                    { 5,  "Cleric",   "Can smite, heal, and guilt-trip—truly holy triple threat.",                                   ArchetypeType.Magical,   8,     0,    0M,    2,    0.3M,  3,    0.6M,  1,   0.2M,  "Mana",      5,       5,      0.3M,  2,        2 }
                });


            migrationBuilder.Sql("SET IDENTITY_INSERT Rooms OFF;");

            migrationBuilder.Sql("UPDATE Players SET ArchetypeId = 1 WHERE ArchetypeId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "ArchetypeId",
                table: "Players",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ArchetypeId",
                table: "Players",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.DeleteData(
                table: "Archetypes",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5 });
        }

    }
}
