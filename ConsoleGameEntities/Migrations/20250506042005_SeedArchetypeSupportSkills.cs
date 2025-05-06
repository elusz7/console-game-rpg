using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedArchetypeSupportSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Skills",
            columns: new[] { "Id", "ArchetypeId", "Name", "Description", "RequiredLevel", "Cost", "Power", "Cooldown", "TargetType", "SkillType", "SkillCategory", "DamageType", "Duration", "StatAffected", "SupportEffect" },
            values: new object[,]
            {
                {12, 1, "Battle Cry", "A fearsome roar that inspires and boosts your Attack.", 3, 3, 2, 3, 0, "SupportSkill", 1, null, 2, 1, 0},
                {14, 2, "Intimidating Roar", "A loud cry that makes enemies hesitate.", 3, 2, 2, 3, 2, "SupportSkill", 1, null, 2, 1, 1},
                {16, 3, "Smoke Bomb", "A cloud of smoke obscures enemy vision.", 3, 3, 2, 3, 2, "SupportSkill", 1, null, 1, 5, 1},
                {18, 4, "Mana Infusion", "The mana in the air is absorbed into your body.", 3, 2, 2, 3, 0, "SupportSkill", 1, null, 2, 2, 0},
                {20, 5, "Blessing of Light", "You are blessed by the heavens.", 3, 3, 4, 3, 0, "SupportSkill", 1, null, 2, 0, 0},
                {23, 1, "Defender's Stance", "Plant your feet like a tree.", 4, 3, 2, 3, 0, "SupportSkill", 1, null, 3, 3, 0},
                {26, 2, "Unstoppable Fury", "Feel emotions. Channel emotions.", 4, 4, 3, 3, 0, "SupportSkill", 1, null, 3, 5, 0},
                {29, 3, "Evasion", "Move your feet faster, don't get hit!", 4, 2, 3, 3, 0, "SupportSkill", 1, null, 3, 3, 0},
                {32, 4, "Arcane Shield", "Magic hardens over you like a second skin.", 4, 3, 3, 3, 0, "SupportSkill", 1, null, 3, 4, 0},
                {35, 5, "Sacred Ward", "Divine light emanates from your skin.", 4, 2, 3, 2, 0, "SupportSkill", 1, null, 2, 3, 0}
            });

            migrationBuilder.Sql("SET IDENTITY_INSERT Skills OFF;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
