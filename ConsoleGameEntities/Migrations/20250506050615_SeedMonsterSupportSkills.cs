using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedMonsterSupportSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Skills",
            columns: new[] { "Id", "Name", "Description", "RequiredLevel", "Cost", "Power", "Cooldown", "TargetType", "SkillType", "SkillCategory", "Duration", "StatAffected", "SupportEffect" },
            values: new object[,]
            {
                // Level 3
                { 168, "Adrenal Boost", "Momentarily increases your attack.", 3, 1, 2, 2, 0, "SupportSkill", 1, 3, 1, 0 },
                { 169, "Ironhide", "Temporarily hardens skin to increase defense.", 3, 1, 2, 2, 0, "SupportSkill", 1, 3, 3, 0 },
                { 170, "Focused Mind", "Raises magic power through deep concentration.", 3, 1, 2, 2, 0, "SupportSkill", 1, 3, 2, 0 },
                { 171, "Shield Break", "Reduces the target's physical defense.", 3, 1, 2, 2, 1, "SupportSkill", 1, 3, 3, 1 },
                { 172, "Arcane Drain", "Weakens the target’s magical resistance.", 3, 1, 2, 2, 1, "SupportSkill", 1, 3, 4, 1 },

                // Level 5
                { 173, "Vigor Surge", "Surge of health energizes the body.", 5, 1, 3, 2, 0, "SupportSkill", 1, 3, 0, 0 },
                { 174, "Sharpen Claws", "Attack power increases with honed edges.", 5, 1, 3, 2, 0, "SupportSkill", 1, 3, 1, 0 },
                { 175, "Mental Shackles", "Suppresses enemy magic.", 5, 1, 3, 2, 1, "SupportSkill", 1, 3, 2, 1 },
                { 176, "Stone Skin", "Physical defense greatly improves.", 5, 1, 3, 2, 0, "SupportSkill", 1, 3, 3, 0 },
                { 177, "Mind Fog", "Reduces target’s magic resistance.", 5, 1, 3, 2, 1, "SupportSkill", 1, 3, 4, 1 },

                // Level 7
                { 178, "Overcharge", "Greatly boosts magic temporarily.", 7, 1, 4, 2, 0, "SupportSkill", 1, 3, 2, 0 },
                { 179, "Rage Howl", "Raises attack power and intimidates foes.", 7, 1, 4, 2, 0, "SupportSkill", 1, 3, 1, 0 },
                { 180, "Armor Melt", "Reduces enemy defense significantly.", 7, 1, 4, 2, 1, "SupportSkill", 1, 3, 3, 1 },
                { 181, "Phase Step", "Boosts speed with temporal magic.", 7, 1, 4, 2, 0, "SupportSkill", 1, 3, 5, 0 },
                { 182, "Chill Veil", "Slows enemy movements.", 7, 1, 4, 2, 1, "SupportSkill", 1, 3, 5, 1 },

                // Level 9
                { 183, "Lifebloom", "Health regenerates for a short time.", 9, 1, 5, 2, 0, "SupportSkill", 1, 3, 0, 0 },
                { 184, "Desolation Mark", "Reduces enemy attack slightly.", 9, 1, 5, 2, 1, "SupportSkill", 1, 3, 1, 1 },
                { 185, "Titan Skin", "Massively boosts defense.", 9, 1, 5, 2, 0, "SupportSkill", 1, 3, 3, 0 },
                { 186, "Stasis Bind", "Greatly slows the enemy.", 9, 1, 5, 2, 1, "SupportSkill", 1, 3, 5, 1 },
                { 187, "Spellflux", "Reduces enemy magic output significantly.", 9, 1, 5, 2, 1, "SupportSkill", 1, 3, 2, 1 }
            });

            


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
