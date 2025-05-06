using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedMonsterUltimateSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Skills",
            columns: new[]
            {
                "Id", "Name", "Description", "RequiredLevel", "Cost", "Power", "Cooldown",
                "TargetType", "SkillType", "SkillCategory", "DamageType"
            },
            values: new object[,]
            {
                { 156, "Savage Pounce", "Leap at your target with primal fury.", 4, 1, 50, 6, 1, "UltimateSkill", 2, 2 },
                { 157, "Roar of Ruin", "Unleash a deafening roar that terrifies your enemies.", 4, 1, 45, 6, 2, "UltimateSkill", 2, 2 },
                { 158, "Hexstorm", "Summon a storm of curses that damages all enemies.", 4, 1, 48, 6, 2, "UltimateSkill", 2, 2 },
                { 159, "Witchfire", "Launch a barrage of purple and black flames.", 4, 1, 47, 6, 2, "UltimateSkill", 2, 2 },
                { 160, "Blood Moon Ritual", "The room turns red as blood as it drains the vitality of your enemies.", 4, 1, 44, 6, 2, "UltimateSkill", 2, 2 },
                { 161, "Featherstorm", "Unleash a flurry of razor-sharp feathers in all directions.", 4, 1, 46, 6, 2, "UltimateSkill", 2, 2 },
                { 162, "Gale Evasion", "Vanish into a gust of wind and freely attack.", 4, 1, 0, 6, 3, "UltimateSkill", 2, 2 },
                { 163, "Sky Cleave", "Dive from above with a devastating strike.", 4, 1, 52, 6, 1, "UltimateSkill", 2, 2 },
                { 164, "Ancient Instinct", "Enter a feral trance, unleash your full potential.", 4, 1, 0, 6, 3, "UltimateSkill", 2, 2 },
                { 165, "Dark Pact", "Call upon the greater demon you serve and unleash hell.", 4, 1, 50, 6, 2, "UltimateSkill", 2, 2 }                
            });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
