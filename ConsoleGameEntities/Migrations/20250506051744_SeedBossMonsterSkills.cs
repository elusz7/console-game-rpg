using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedBossMonsterSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Skills",
            columns: new[]
            {
                "Id", "MonsterId", "Name", "Description", "RequiredLevel", "Cost", "Power", "Cooldown",
                "TargetType", "SkillType", "SkillCategory", "DamageType"
            },
            values: new object[,]
            {
               {
                    188, 26, "Thousand Masks",
                    "Unleash a storm of illusions, each striking with deadly precision.",
                    10, 1, 80, 10, 2, "UltimateSkill", 2, 2
                },
                {
                    189, 27, "Soul Cataclysm",
                    "Rend the battlefield asunder with necrotic force and cursed souls.",
                    10, 1, 85, 10, 2, "UltimateSkill", 2, 2
                },
                {
                    190, 28, "Crimson Requiem",
                    "Drain the blood and vitality of all enemies, healing yourself with their life force.",
                    10, 1, 75, 10, 2, "UltimateSkill", 2, 2
                },
                { 191, 26, "Void Fang", "Sink fangs into the target, draining both health and light.", 10, 1, 18, 3, 1, "MartialSkill", 0, 0 },
                { 192, 26, "Foxfire Burst", "Launch an explosive orb of ghostly blue fire.", 10, 1, 20, 3, 1, "MagicalSkill", 0, 1 },
                { 193, 26, "Shadow Mirage", "Strike from multiple illusory angles at once.", 10, 1, 19, 3, 1, "MartialSkill", 0, 0 },
                { 194, 26, "Night Curse", "Infuse the enemy with cursed magic that saps strength.", 10, 1, 21, 3, 1, "MagicalSkill", 0, 1 },
                { 195, 26, "Howl of Dread", "Let out a piercing cry that disrupts the soul.", 10, 1, 22, 3, 1, "MagicalSkill", 0, 1 },
                { 196, 27, "Necrotic Blast", "Unleash a blast of dark energy that erodes the target's life force.", 10, 1, 22, 3, 1, "MagicalSkill", 0, 1 },
                { 197, 27, "Soul Leech", "Drain the very soul of the target, healing the lich for a portion of the damage dealt.", 10, 1, 20, 3, 1, "MagicalSkill", 0, 1 },
                { 198, 27, "Withering Touch", "Strike with a touch that decays the target's flesh and spirit.", 10, 1, 21, 3, 1, "MartialSkill", 0, 0 },
                { 199, 27, "Death's Embrace", "Summon an ethereal hand to grasp the target, dealing dark damage over time.", 10, 1, 23, 3, 1, "MagicalSkill", 0, 1 },
                { 200, 27, "Dark Convergence", "Gather the energies of the void to collapse on the enemy, dealing massive damage.", 10, 1, 24, 3, 1, "MagicalSkill", 0, 1 },
                { 201, 28, "Blood Feast", "Drain the blood of your target, healing yourself for a portion of the damage dealt.", 10, 1, 22, 3, 1, "MagicalSkill", 0, 1 },
                { 202, 28, "Crimson Fury", "Unleash a flurry of rapid strikes that tear into your opponent's flesh.", 10, 1, 21, 3, 1, "MartialSkill", 0, 0 },
                { 203, 28, "Vampiric Eclipse", "Call upon the darkness to shield you and deal damage to nearby enemies.", 10, 1, 20, 3, 1, "MagicalSkill", 0, 1 },
                { 204, 28, "Sanguine Vortex", "Summon a vortex of blood and dark magic, pulling enemies toward you and dealing damage.", 10, 1, 23, 3, 1, "MagicalSkill", 0, 1 },
                { 205, 28, "Night's Kiss", "Unleash a powerful burst of shadow magic that drains the life force of your target.", 10, 1, 24, 3, 1, "MagicalSkill", 0, 1 }
            });

            migrationBuilder.InsertData(
            table: "Skills",
            columns: new[]
            {
                "Id", "MonsterId", "Name", "Description", "RequiredLevel", "Cost", "Power", "Cooldown",
                "TargetType", "SkillType", "SkillCategory", "Duration", "StatAffected", "SupportEffect"
            },
            values: new object[,]
            {
                // Shadowed Nogitsune (MonsterId: 26)
                { 206, 26, "Eternal Trickery", "Reduce the target's defense, leaving them vulnerable to attacks.", 10, 1, 15, 5, 1, "SupportSkill", 1, 1, 3, 3 },
                { 207, 26, "Illusionary Mist", "Create an illusionary mist that confuses enemies, reducing their accuracy.", 10, 1, 12, 5, 1, "SupportSkill", 1, 3, 5, 1 },
                { 208, 26, "Vengeful Spirit", "Increase your attack power as you harness the energy of vengeful spirits.", 10, 1, 18, 5, 0, "SupportSkill", 1, 3, 1, 0 },

                // Ancient Lich (MonsterId: 27)
                { 209, 27, "Necrotic Surge", "Buff your resistance to magical attacks, making you more durable against magic.", 10, 0, 20, 4, 0, "SupportSkill", 1, 5, 4, 0 },
                { 210, 27, "Curse of Decay", "Inflict a curse on an enemy, reducing their speed over time.", 10, 1, 16, 5, 1, "SupportSkill", 1, 4, 5, 1 },
                { 211, 27, "Soul Drain", "Drain the life force from your enemies, healing yourself while reducing their health.", 10, 1, 18, 5, 1, "SupportSkill", 1, 3, 0, 1 },

                // Elder Vampire (MonsterId: 28)
                { 212, 28, "Blood Shield", "Create a shield that absorbs damage, granting you extra defense.", 10, 1, 20, 5, 0, "SupportSkill", 1, 5, 3, 0 },
                { 213, 28, "Dark Blessing", "Enhance your attack power with the blessing of darkness, increasing damage dealt.", 10, 1, 22, 5, 0, "SupportSkill", 1, 5, 1, 0 },
                { 214, 28, "Siphon Vitality", "Siphon the vitality from a nearby enemy, reducing their strength while restoring your own health.", 10, 1, 18, 5, 1, "SupportSkill", 1, 4, 0, 1 }
            });

            migrationBuilder.Sql("SET IDENTITY_INSERT Skills OFF;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
