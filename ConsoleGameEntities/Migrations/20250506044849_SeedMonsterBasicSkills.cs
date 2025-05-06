using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedMonsterBasicSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT Skills ON;");
            migrationBuilder.InsertData(
            table: "Skills",
            columns: new[]
            {
                "Id", "Name", "Description", "RequiredLevel", "Cost", "Power", "Cooldown",
                "TargetType", "SkillType", "SkillCategory", "DamageType"
            },
            values: new object[,]
            {
                { 111, "Swift Strike", "A quick strike that catches the enemy off guard.", 2, 1, 8, 1, 1, "MartialSkill", 0, 0 },
                { 112, "Flame Dart", "A small burst of fire aimed at the enemy.", 2, 1, 9, 1, 1, "MagicalSkill", 0, 1 },
                { 113, "Shield Bash", "A defensive bash with your shield to stagger the enemy.", 2, 1, 10, 1, 1, "MartialSkill", 0, 0 },
                { 114, "Frostbite", "A blast of cold energy to freeze the enemy in place.", 2, 1, 11, 2, 1, "MagicalSkill", 0, 1 },
                { 115, "Wind Slash", "A slash of wind that cuts through enemies.", 2, 1, 12, 2, 1, "MartialSkill", 0, 0 },
                { 116, "Arcane Bolt", "A bolt of pure arcane energy shot from your hand.", 2, 1, 13, 2, 1, "MagicalSkill", 0, 1 },
                { 117, "Power Strike", "A heavy strike that deals massive damage.", 2, 1, 13, 2, 1, "MartialSkill", 0, 0 },
                { 118, "Elemental Blast", "A surge of elemental energy strikes the target.", 2, 1, 12, 2, 1, "MagicalSkill", 0, 1 },
                { 119, "Slash", "A quick, basic attack dealing moderate damage.", 2, 1, 10, 1, 1, "MartialSkill", 0, 0 },
                { 120, "Fireball", "A small fireball that explodes on impact.", 2, 1, 11, 2, 1, "MagicalSkill", 0, 1 },
                { 121, "Brutal Uppercut", "A savage uppercut that knocks foes off their footing.", 4, 1, 25, 2, 1, "MartialSkill", 0, 0 },
                { 122, "Shadow Lance", "A piercing spear of dark energy hurled with malice.", 4, 1, 27, 2, 1, "MagicalSkill", 0, 1 },
                { 123, "Bone Crusher", "A bone-shattering blow aimed at limbs.", 4, 1, 28, 3, 1, "MartialSkill", 0, 0 },
                { 124, "Inferno Shot", "A searing blaze of fire races toward the target.", 4, 1, 30, 3, 1, "MagicalSkill", 0, 1 },
                { 125, "Iron Claw", "A crushing claw attack that rends armor.", 4, 1, 26, 2, 1, "MartialSkill", 0, 0 },
                { 126, "Ice Spike", "A jagged spike of ice erupts under the foe.", 4, 1, 24, 2, 1, "MagicalSkill", 0, 1 },
                { 127, "Skull Bash", "A brutal headbutt aimed at disorientation.", 4, 1, 23, 2, 1, "MartialSkill", 0, 0 },
                { 128, "Thunder Coil", "Electricity coils around the enemy and discharges violently.", 4, 1, 29, 3, 1, "MagicalSkill", 0, 1 },
                { 129, "Savage Rend", "Tears into the target with wild abandon.", 4, 1, 27, 2, 1, "MartialSkill", 0, 0 },
                { 130, "Void Pulse", "A blast of void energy disrupts the target's body and mind.", 4, 1, 28, 3, 1, "MagicalSkill", 0, 1 },
                { 131, "Titan's Fist", "A mighty punch that crushes through enemies with overwhelming force.", 6, 1, 55, 4, 1, "MartialSkill", 0, 0 },
                { 132, "Arcane Fury", "Unleashes a flurry of destructive arcane energy in all directions.", 6, 1, 58, 4, 1, "MagicalSkill", 0, 1 },
                { 133, "Dragon's Roar", "A deafening roar that sends shockwaves through enemies.", 6, 1, 62, 5, 1, "MartialSkill", 0, 0 },
                { 134, "Lightning Surge", "A surge of lightning that strikes multiple enemies at once.", 6, 1, 54, 3, 1, "MagicalSkill", 0, 1 },
                { 135, "Earthquake Stomp", "A powerful stomp that causes the ground to tremble and shake.", 6, 1, 60, 4, 1, "MartialSkill", 0, 0 },
                { 136, "Frost Nova", "A burst of freezing energy radiates outward, slowing and damaging enemies.", 6, 1, 56, 4, 1, "MagicalSkill", 0, 1 },
                { 137, "Hellfire Blast", "Summons a massive explosion of fire and brimstone.", 6, 1, 64, 5, 1, "MagicalSkill", 0, 1 },
                { 138, "Stone Crush", "Crushes the enemy with a giant boulder summoned from the earth.", 6, 1, 63, 5, 1, "MartialSkill", 0, 0 },
                { 139, "Tornado Strike", "Creates a small tornado that sweeps through enemies, causing massive damage.", 6, 1, 61, 4, 1, "MartialSkill", 0, 0 },
                { 140, "Solar Flare", "A concentrated burst of solar energy blinds and burns enemies.", 6, 1, 59, 4, 1, "MagicalSkill", 0, 1 },
                { 141, "Phoenix's Wrath", "Unleashes the fiery fury of the Phoenix, burning all enemies in range.", 8, 1, 85, 6, 1, "MagicalSkill", 0, 1 },
                { 142, "Titan's Fury", "Summons the might of the Titans to deal colossal damage to enemies.", 8, 1, 90, 7, 1, "MartialSkill", 0, 0 },
                { 143, "Meteor Storm", "Summons a storm of meteors that rains down fiery destruction on enemies.", 8, 1, 95, 8, 1, "MagicalSkill", 0, 1 },
                { 144, "Blizzard Gale", "A powerful storm of ice and wind that freezes and damages enemies in its path.", 8, 1, 87, 6, 1, "MagicalSkill", 0, 1 },
                { 145, "Vortex Slash", "A swirling slash of wind and energy that cuts through all foes in its path.", 8, 1, 92, 7, 1, "MartialSkill", 0, 0 },
                { 146, "Divine Smite", "A mighty strike from the heavens that delivers devastating holy damage.", 8, 1, 80, 6, 1, "MartialSkill", 0, 0 },
                { 147, "Void Blast", "A burst of dark energy that tears apart the fabric of space, damaging enemies.", 8, 1, 93, 8, 1, "MagicalSkill", 0, 1 },
                { 148, "Thunderclap", "A massive thunderous shockwave that stuns and damages enemies.", 8, 1, 88, 6, 1, "MartialSkill", 0, 0 },
                { 149, "Arcane Barrage", "Fires a barrage of magical bolts, each dealing substantial damage.", 8, 1, 94, 7, 1, "MagicalSkill", 0, 1 },
                { 150, "Dragon's Might", "The power of a dragon surges through the user, causing massive damage to enemies.", 8, 1, 99, 8, 1, "MartialSkill", 0, 0 },
                { 151, "Eclipse Beam", "Fires a concentrated beam of pure darkness that obliterates enemies in its path.", 10, 1, 130, 9, 1, "MagicalSkill", 0, 1 },
                { 152, "Cataclysm", "A devastating attack that causes the earth to crack open and crush enemies.", 10, 1, 145, 10, 1, "MartialSkill", 0, 0 },
                { 153, "Heaven's Wrath", "A divine strike from the heavens that deals massive holy damage to all foes.", 10, 1, 140, 10, 1, "MartialSkill", 0, 0 },
                { 154, "Arcane Fury", "Unleashes a powerful burst of raw magical energy, causing massive area damage.", 10, 1, 135, 8, 1, "MagicalSkill", 0, 1 },
                { 155, "Endless Storm", "Summons an unstoppable storm that causes continuous damage to all nearby enemies.", 10, 1, 120, 11, 1, "MagicalSkill", 0, 1 }
            });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}