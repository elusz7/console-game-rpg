using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedArchetypeBaseSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT Skills ON;");
            migrationBuilder.InsertData(
            table: "Skills",
            columns: new[] { "Id", "ArchetypeId", "Name", "Description", "RequiredLevel", "Cost", "Power", "Cooldown", "TargetType", "SkillType", "SkillCategory", "DamageType" },
            values: new object[,]
            {
                {1, 1, "Power Slash", "A heavy swing aimed at breaking defenses.", 1, 2, 7, 0, 1, "MartialSkill", 0, 0},
                {2, 1, "Staggering Blow", "A powerful hit that leaves enemies reeling.", 2, 3, 9, 2, 1, "MartialSkill", 0, 0},
                {3, 2, "Gut Punch", "A brutal strike to wind the enemy.", 1, 2, 6, 1, 1, "MartialSkill", 0, 0},
                {4, 2, "Rampage Swing", "Wild, reckless overhead strike.", 2, 3, 11, 2, 1, "MartialSkill", 0, 0},
                {5, 3, "Quick Stab", "A lightning-fast strike aimed at weak points.", 1, 2, 8, 0, 1, "MartialSkill", 0, 0},
                {6, 3, "Smoke Blade", "Strike and vanish in the same breath.", 2, 3, 10, 2, 1, "MartialSkill", 0, 0},
                {7, 4, "Ember Spark", "A focused bolt of flame.", 1, 2, 6, 0, 1, "MagicalSkill", 0, 1},
                {8, 4, "Ice Lance", "A chilling projectile of ice.", 2, 3, 11, 2, 1, "MagicalSkill", 0, 1},
                {9, 5, "Radiant Bolt", "A divine light strikes the foe.", 1, 2, 9, 1, 1, "MagicalSkill", 0, 1},
                {10, 5, "Thunder Hymn", "A burst of holy thunder.", 2, 3, 13, 2, 1, "MagicalSkill", 0, 1},
                {11, 1, "Whirlwind Strike", "A spinning attack that clears a path through enemies.", 3, 6, 15, 6, 2, "UltimateSkill", 2, 2},
                {13, 2, "Earthshatter Slam", "Uses sheer force to disrupt the battlefield.", 3, 6, 15, 6, 2, "UltimateSkill", 2, 2},
                {15, 3, "Shadow Dance", "A blur of movement and blades.", 3, 6, 15, 6, 2, "UltimateSkill", 2, 2},
                {17, 4, "Elemental Surge", "The elements leap to your call", 3, 6, 15, 6, 2, "UltimateSkill", 2, 2},
                {19, 5, "Divine Judgment", "Summon the Wrath of the heavens.", 3, 6, 15, 6, 2, "UltimateSkill", 2, 2},
                {21, 1, "Slashing Wind", "The sword cuts through the wind and strikes all enemies.", 4, 4, 24, 2, 2, "MartialSkill", 0, 0},
                {22, 1, "Cleaving Strike", "Wind up and swing. You'll hit somebody.", 4, 3, 32, 4, 1, "MartialSkill", 0, 0},
                {24, 2, "Ground Slam", "Stomp your foot and shake the ground", 4, 5, 26, 4, 2, "MartialSkill", 0, 0},
                {25, 2, "Rampaging Charge", "Wind up, close your eyes, and charge.", 4, 4, 29, 3, 1, "MartialSkill", 0, 0},
                {27, 3, "Backstab", "Slip past your enemy's defenses and stab!", 4, 3, 27, 3, 1, "MartialSkill", 0, 0},
                {28, 3, "Shadowstrike", "Strike with the power of the shadows.", 4, 3, 31, 2, 1, "MartialSkill", 0, 0},
                {30, 4, "Flame Wave", "Launch a wave of flame at your enemies", 4, 4, 28, 3, 2, "MagicalSkill", 0, 1},
                {31, 4, "Arcane Missile", "Shoot forth a misslile of magic power.", 4, 2, 34, 2, 1, "MagicalSkill", 0, 1},
                {33, 5, "Divine Smite", "Call down your god to vanquish a foe", 4, 3, 30, 2, 1, "MagicalSkill", 0, 1},
                {34, 5, "Holy Light", "A bright light bursts forth from your hands", 4, 4, 25, 3, 2, "MagicalSkill", 0, 1},
                {36, 1, "Iron Tempest", "Your blades move fast like a tempest.", 5, 7, 40, 5, 2, "MartialSkill", 0, 0},
                {37, 1, "Blade Rush", "Dash through enemies, slicing each in your path", 5, 6, 50, 6, 2, "MartialSkill", 0, 0},
                {38, 1, "Punishing Blow", "A crushing attack", 5, 6, 45, 4, 1, "MartialSkill", 0, 0},
                {39, 2, "Savage Uppercut", "A brutal upward punch", 5, 5, 45, 3, 1, "MartialSkill", 0, 0},
                {40, 2, "Brutal Lunge", "Charge at a foe with reckless abandon.", 5, 6, 49, 4, 1, "MartialSkill", 0, 0},
                {41, 2, "Bonecrusher", "A heavy strike aimed at breaking bones and armor alike.", 5, 8, 54, 5, 1, "MartialSkill", 0, 0},
                {42, 3, "Fan of Knives", "Throw of flurry of knives in all directions. Where did they even come from?", 5, 5, 40, 4, 2, "MartialSkill", 0, 0},
                {43, 3, "Bleeding Strike", "Target a spot that's going to hurt. A lot.", 5, 4, 45, 5, 1, "MartialSkill", 0, 0},
                {44, 3, "Ambush", "Leap with surprising agility to surprise your foes.", 5, 6, 55, 5, 1, "MartialSkill", 0, 0},
                {45, 4, "Chain Lightning", "Lightning arcs through all enemies", 5, 6, 50, 5, 2, "MagicalSkill", 0, 1},
                {46, 4, "Frostbite", "Ice creeps over an enemy", 5, 5, 40, 3, 1, "MagicalSkill", 0, 1},
                {47, 4, "Arcane Detonation", "Create an arcane explosion", 5, 6, 45, 4, 2, "MagicalSkill", 0, 1},
                {48, 5, "Judgment Flame", "A holy fire that burns all enemies", 5, 6, 45, 4, 2, "MagicalSkill", 0, 1},
                {49, 5, "Heaven's Hammer", "A divine hammer to crush a foe", 5, 8, 55, 6, 1, "MagicalSkill", 0, 1},
                {50, 5, "Blinding Radiance", "Divine light blinds the room. Wear sunglasses next time.", 5, 7, 50, 5, 2, "MagicalSkill", 0, 1},
                {51, 1, "Titan Cleave", "Deliver a devastating wide swing that shatters defenses.", 6, 9, 70, 7, 2, "MartialSkill", 0, 0},
                {52, 1, "Executioner’s Strike", "A brutal blow aimed to finish weakened foes.", 6, 8, 75, 8, 1, "MartialSkill", 0, 0},
                {55, 2, "Bonequake Slam", "Crush the ground and crack bones with raw force.", 6, 10, 75, 9, 2, "MartialSkill", 0, 0},
                {56, 2, "Fury Breaker", "Focus all your strength into a single armor-breaking punch.", 6, 7, 65, 6, 1, "MartialSkill", 0, 0},
                {59, 3, "Death Blossom", "Spin through enemies, blades flying like petals.", 6, 8, 60, 7, 2, "MartialSkill", 0, 0},
                {60, 3, "Throatpiercer", "A ruthless critical strike aimed at vital spots.", 6, 7, 70, 6, 1, "MartialSkill", 0, 0},
                {63, 4, "Cataclysm Ray", "Unleash a focused magical beam of destruction.", 6, 9, 70, 8, 1, "MagicalSkill", 0, 1},
                {64, 4, "Voidstorm", "Summon a chaotic storm of arcane energy.", 6, 10, 65, 9, 2, "MagicalSkill", 0, 1},
                {67, 5, "Divine Lance", "Launch a radiant spear that sears with holy fire.", 6, 8, 65, 7, 1, "MagicalSkill", 0, 1},
                {68, 5, "Radiant Pulse", "Send a blinding wave of light through the battlefield.", 6, 9, 60, 6, 2, "MagicalSkill", 0, 1},
                {71, 1, "Crushing Arc", "A heavy swing aimed to break multiple enemies at once.", 7, 9, 70, 8, 2, "MartialSkill", 0, 0},
                {74, 2, "Juggernaut Slam", "A powerful, ground-shaking charge that flattens enemies.", 7, 10, 75, 9, 2, "MartialSkill", 0, 0},
                {77, 3, "Blackout Strike", "A quick blow to a pressure point, aimed to disorient.", 7, 8, 65, 7, 1, "MartialSkill", 0, 0},
                {80, 4, "Comet Shard", "Drop a magical meteor chunk onto a foe with crushing force.", 7, 9, 70, 9, 1, "MagicalSkill", 0, 1},
                {83, 5, "Purifying Flame", "Send out cleansing fire that damages the impure.", 7, 8, 65, 7, 2, "MagicalSkill", 0, 1},
                {86, 1, "Thunder Cleave", "A massive sweeping strike infused with thunderous force.", 8, 10, 75, 9, 2, "MartialSkill", 0, 0},
                {87, 1, "Precision Slam", "A perfectly timed hit that bypasses some enemy defense.", 8, 9, 70, 8, 1, "MartialSkill", 0, 0},
                {89, 2, "Skullbreaker", "A bone-crunching blow aimed at disabling the enemy.", 8, 10, 75, 9, 1, "MartialSkill", 0, 0},
                {90, 2, "Spiked Collision", "Slam enemies with brutal force, causing splash damage.", 8, 9, 70, 8, 2, "MartialSkill", 0, 0},
                {92, 3, "Blade Fan", "A flurry of blades hits multiple targets.", 8, 9, 70, 8, 2, "MartialSkill", 0, 0},
                {93, 3, "Eviscerate", "A swift strike to vital organs for major damage.", 8, 10, 75, 9, 1, "MartialSkill", 0, 0},
                {95, 4, "Arc Lightning", "Chain lightning arcs between enemies.", 8, 9, 70, 8, 2, "MagicalSkill", 0, 1},
                {96, 4, "Void Pulse", "A pulse of chaotic magic that damages and confuses.", 8, 10, 75, 9, 1, "MagicalSkill", 0, 1},
                {98, 5, "Sacred Eruption", "Burst of divine light harms all foes.", 8, 9, 70, 8, 2, "MagicalSkill", 0, 1},
                {99, 5, "Judgement Spear", "Hurl a holy spear that pierces defenses.", 8, 10, 75, 9, 1, "MagicalSkill", 0, 1},
                {101, 1, "Meteor Crash", "Leap and strike the ground with explosive force.", 9, 11, 85, 9, 2, "MartialSkill", 0, 0},
                {102, 2, "Earthshatter", "A ground-shaking slam that damages and dazes.", 9, 11, 85, 9, 2, "MartialSkill", 0, 0},
                {103, 3, "Death Lotus", "A deadly spinning flurry of blades.", 9, 11, 85, 9, 2, "MartialSkill", 0, 0},
                {104, 4, "Rift Spear", "Summon a spear from another plane to skewer your foe.", 9, 11, 85, 9, 1, "MagicalSkill", 0, 1},
                {105, 5, "Beacon Burst", "Radiate divine energy that burns evil.", 9, 11, 85, 9, 2, "MagicalSkill", 0, 1},
                {106, 1, "Dragon’s End", "Channel all strength into a devastating final blow.", 10, 12, 100, 10, 1, "MartialSkill", 0, 0},
                {107, 2, "Colossus Crush", "Slam with titanic force, obliterating armor and flesh.", 10, 12, 100, 10, 2, "MartialSkill", 0, 0},
                {108, 3, "Night Reaper", "Unleash a precise, lethal combo on a single target.", 10, 12, 100, 10, 1, "MartialSkill", 0, 0},
                {109, 4, "Starfall", "Bring down stars to scorch all enemies in radiant fire.", 10, 12, 100, 10, 2, "MagicalSkill", 0, 1},
                {110, 5, "Divine Wrath", "Summon the full might of your god to smite the wicked.", 10, 12, 100, 10, 2, "MagicalSkill", 0, 1}
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
