using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class ReSeedMonsters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Monsters");

            migrationBuilder.Sql("SET IDENTITY_INSERT Monsters ON;");
            migrationBuilder.InsertData(
            table: "Monsters",
            columns: new[] { "Id", "Name", "Description", "Level", "MaxHealth", "ThreatLevel", "AggressionLevel", "DefensePower", "AttackPower", "DamageType", "Resistance", "MonsterType" },
            values: new object[,]
            {
                // Level 1 - Low Threat
                {1, "Goblin", "A small, quick creature with a mischievous streak and a crude blade.", 1, 6, 0, 7, 3, 7, 0, 2, "Monster"},
                {2, "Zombie", "A slow-moving undead with an insatiable hunger for flesh.", 1, 12, 0, 3, 2, 5, 0, 3, "Monster"},
                {3, "Skeleton", "A reanimated skeleton warrior wielding old weapons with eerie precision.", 1, 9, 0, 5, 4, 6, 0, 4, "Monster"},

                // Level 1 - Medium Threat
                {4, "Tunnel Spider", "A venomous spider that ambushes prey from the dark tunnels it calls home.", 1, 10, 1, 5, 4, 8, 0, 2, "Monster"},
                {5, "Gnoll Scout", "A hyena-like humanoid that hunts in packs, using sharp weapons and speed.", 1, 8, 1, 6, 3, 9, 1, 3, "Monster"}, 

                // Level 1 - High Threat
                {6, "Dire Wolf", "A large, feral wolf with fangs sharp enough to tear armor apart.", 1, 14, 2, 8, 5, 10, 0, 3, "Monster"}, 

                // Level 4 - Low Threat
                {7, "Swamp Rat", "A giant rat that thrives in filth and swamps, surprisingly aggressive.", 4, 18, 0, 4, 5, 6, 0, 2, "Monster"},
                {8, "Fire Beetle", "A beetle that glows with inner fire, sometimes igniting its foes.", 4, 15, 0, 5, 4, 6, 1, 4, "Monster"},
                {9, "Ghoul", "An undead that paralyzes with its claws and feasts on the living.", 4, 16, 0, 3, 6, 6, 0, 3, "Monster"}, 

                // Level 4 - Medium Threat
                {10, "Banshee", "A wailing spirit that drains life and chills the soul with its voice.", 4, 22, 1, 6, 5, 9, 1, 4, "Monster"}, 

                // Level 4 - High Threat
                {11, "Hellhound", "A blazing canine from the underworld that burns with infernal fire.", 4, 28, 2, 7, 7, 12, 0, 6, "Monster"},

                // Level 7 - Low Threat
                {12, "Harpy", "A winged predator with a hypnotic song and sharp talons.", 7, 32, 0, 4, 6, 9, 1, 5, "Monster"},
                {13, "Orc", "A brutal warrior driven by rage and brute strength.", 7, 28, 0, 5, 5, 10, 0, 3, "Monster"},
                {14, "Animated Armor", "An enchanted suit of armor that patrols ancient ruins.", 7, 30, 0, 7, 8, 11, 1, 5, "Monster"}, 

                // Level 7 - Medium Threat
                {15, "Spectre", "An incorporeal ghost that saps life with every touch.", 7, 35, 1, 7, 6, 14, 1, 4, "Monster"},

                // Level 7 - High Threat
                {16, "Werewolf", "A savage beast cursed to transform under the moon and shred its prey.", 7, 45, 2, 8, 8, 16, 0, 6, "Monster"},

                // Level 1 - Elite
                {17, "Tiger", "A fearsome predator with unmatched agility and power.", 1, 20, 3, 10, 10, 13, 0, 5, "EliteMonster"},
                {18, "Apprentice", "A novice spellcaster with surprising command over magic.", 1, 20, 3, 10, 5, 13, 1, 10, "EliteMonster"},
                {19, "Kappa", "A trickster water spirit known for its martial skill and cunning.", 1, 20, 3, 10, 7, 13, 2, 7, "EliteMonster"},

                // Level 4 - Elite
                {20, "Sabretooth Tiger", "An ancient beast with long fangs and a thunderous roar.", 4, 35, 3, 14, 13, 17, 0, 8, "EliteMonster"},
                {21, "Witch", "A cunning spellcaster with a deep knowledge of curses and illusions.", 4, 35, 3, 14, 8, 17, 1, 13, "EliteMonster"},
                {22, "Tengu", "A mystical bird warrior that blends swordplay and magic.", 4, 35, 3, 14, 10, 17, 2, 10, "EliteMonster"},

                // Level 7 - Elite
                {23, "Weretiger", "A deadly shapeshifter with the strength of a beast and the mind of a hunter.", 7, 60, 3, 16, 25, 21, 0, 16, "EliteMonster"},
                {24, "Coven Leader", "The head of a sinister coven, weaving powerful spells with deadly grace.", 7, 60, 3, 16, 16, 21, 1, 25, "EliteMonster"},
                {25, "Yuki Onna", "A chilling spirit of winter, freezing her victims with a mere glance.", 7, 60, 3, 16, 20, 21, 2, 20, "EliteMonster"},

                // Bosses
                {26, "Shadowed Nogitsune", "A deceitful fox spirit cloaked in shadow and illusion.", 10, 100, 4, 22, 31, 27, 2, 31, "BossMonster" },
                {27, "Ancient Lich", "A powerful undead sorcerer whose dark magic corrupts the air itself.", 10, 100, 4, 22, 28, 27, 1, 35, "BossMonster" },
                {28, "Elder Vampire", "An ancient predator who thrives on blood and rules the night.", 10, 100, 4, 22, 35, 27, 0, 28, "BossMonster" }
            });

            migrationBuilder.Sql("SET IDENTITY_INSERT Monsters OFF;");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Monsters",
                type: "nvarchar(max)",
                nullable: false); //description is required
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}