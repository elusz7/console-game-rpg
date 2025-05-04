using Microsoft.EntityFrameworkCore.Migrations;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedMonsters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT Archetypes OFF;");

            migrationBuilder.Sql("DELETE FROM Monsters WHERE Id = 1");

            migrationBuilder.Sql("SET IDENTITY_INSERT Monsters ON;");
            migrationBuilder.InsertData(
            table: "Monsters",
            columns: new[] { "Id", "Name", "Level", "MaxHealth", "ThreatLevel", "AggressionLevel", "DefensePower", "AttackPower", "DamageType", "Resistance", "MonsterType" },
            values: new object[,]
            {
                // Level 1 - Low Threat
                {1, "Goblin",   1,  6,  0, 7, 3, 7, 1, 2, "Monster"},
                {2, "Zombie",   1,  12, 0, 3, 2, 5, 1, 3, "Monster" },
                {3, "Skeleton", 1,  9,  0, 5, 4, 6, 1, 4, "Monster" },

                // Level 1 - Medium Threat
                {4, "Tunnel Spider",    1, 10,  1, 5, 4, 8, 1, 2, "Monster"},
                {5, "Gnoll Scout",      1, 8,   1, 6, 3, 9, 2, 3, "Monster"}, 

                // Level 1 - High Threat
                {6, "Dire Wolf", 1, 14, 2, 8, 5, 10, 1, 3, "Monster"}, 

                // Level 4 - Low Threat
                {7, "Swamp Rat",    4, 18, 0, 4, 5, 6, 1, 2, "Monster"}, 
                {8, "Fire Beetle",  4, 15, 0, 5, 4, 6, 2, 4, "Monster"}, 
                {9, "Ghoul",        4, 16, 0, 3, 6, 6, 1, 3, "Monster"}, 

                // Level 4 - Medium Threat
                {10, "Banshee", 4, 22, 1, 6, 5, 9, 2, 4, "Monster"}, 

                // Level 4 - High Threat
                {11, "Hellhound", 4, 28, 2, 7, 7, 12, 1, 6, "Monster"},

                // Level 7 - Low Threat
                {12, "Harpy",           7, 32, 0, 4, 6, 9,  2, 5, "Monster"},
                {13, "Orc",             7, 28, 0, 5, 5, 10, 1, 3, "Monster"}, 
                {14, "Animated Armor",  7, 30, 0, 7, 8, 11, 2, 5, "Monster"}, 

                // Level 7 - Medium Threat
                {15, "Spectre", 7, 35, 1, 7, 6, 14, 2, 4, "Monster"},

                // Level 7 - High Threat
                {16, "Werewolf", 7, 45, 2, 8, 8, 16, 1, 6, "Monster"}
            });

            migrationBuilder.Sql("SET IDENTITY_INSERT Monsters OFF;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
