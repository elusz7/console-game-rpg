using Microsoft.EntityFrameworkCore.Migrations;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class SeedRunes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("SET IDENTITY_INSERT Runes ON");
        migrationBuilder.InsertData(
            table: "Runes",
            columns: new[] { "Id", "Name", "RuneType", "Element", "Rarity", "Tier", "Power", "Duration" },
            values: new object[,]
            {
                { 1, "Ember Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Common, 0, 2, 3 },
                { 2, "Fire Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Uncommon, 0, 3, 3 },
                { 3, "Blaze Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Rare, 0, 4, 3 },
                { 4, "Lava Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Epic, 0, 5, 3 },
                { 5, "Inferno Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Legendary, 0, 7, 3 },
                { 6, "Phoenix Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Mythic, 0, 10, 3 },

                { 7, "Ember Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Common, 0, 2, null },
                { 8, "Fire Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Uncommon, 0, 3, null },
                { 9, "Blaze Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Rare, 0, 4, null },
                { 10, "Lava Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Epic, 0, 5, null },
                { 11, "Inferno Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Legendary, 0, 7, null },
                { 12, "Phoenix Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Mythic, 0, 10, null },

                { 13, "Frost Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Common, 0, 2, 3 },
                { 14, "Ice Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Uncommon, 0, 3, 3 },
                { 15, "Hail Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Rare, 0, 4, 3 },
                { 16, "Glacier Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Epic, 0, 5, 3 },
                { 17, "Blizzard Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Legendary, 0, 7, 3 },
                { 18, "Ymir Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Mythic, 0, 10, 3 },

                { 19, "Frost Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Common, 0, 2, null },
                { 20, "Ice Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Uncommon, 0, 3, null },
                { 21, "Hail Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Rare, 0, 4, null },
                { 22, "Glacier Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Epic, 0, 5, null },
                { 23, "Blizzard Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Legendary, 0, 7, null },
                { 24, "Ymir Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Mythic, 0, 10, null },

                { 25, "Spark Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Common, 0, 2, 3 },
                { 26, "Jolt Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Uncommon, 0, 3, 3 },
                { 27, "Lightning Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Rare, 0, 4, 3 },
                { 28, "Storm Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Epic, 0, 5, 3 },
                { 29, "Tempest Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Legendary, 0, 7, 3 },
                { 30, "Raijin Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Mythic, 0, 10, 3 },

                { 31, "Spark Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Common, 0, 2, null },
                { 32, "Jolt Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Uncommon, 0, 3, null },
                { 33, "Lightning Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Rare, 0, 4, null },
                { 34, "Storm Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Epic, 0, 5, null },
                { 35, "Tempest Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Legendary, 0, 7, null },
                { 36, "Raijin Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Mythic, 0, 10, null },

                { 37, "Seed Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Common, 0, 2, 3 },
                { 38, "Nature Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Uncommon, 0, 3, 3 },
                { 39, "Thorn Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Rare, 0, 4, 3 },
                { 40, "Beast Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Epic, 0, 5, 3 },
                { 41, "Quake Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Legendary, 0, 7, 3 },
                { 42, "Gaia Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Mythic, 0, 10, 3 },

                { 43, "Seed Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Common, 0, 2, null },
                { 44, "Nature Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Uncommon, 0, 3, null },
                { 45, "Thorn Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Rare, 0, 4, null },
                { 46, "Beast Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Epic, 0, 5, null },
                { 47, "Quake Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Legendary, 0, 7, null },
                { 48, "Gaia Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Mythic, 0, 10, null },

                { 49, "Bright Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Common, 0, 2, 3 },
                { 50, "Energy Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Uncommon, 0, 3, 3 },
                { 51, "Radiant Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Rare, 0, 4, 3 },
                { 52, "Solar Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Epic, 0, 5, 3 },
                { 53, "Divine Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Legendary, 0, 7, 3 },
                { 54, "Seraph Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Mythic, 0, 10, 3 },

                { 55, "Bright Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Common, 0, 2, null },
                { 56, "Energy Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Uncommon, 0, 3, null },
                { 57, "Radiant Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Rare, 0, 4, null },
                { 58, "Solar Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Epic, 0, 5, null },
                { 59, "Divine Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Legendary, 0, 7, null },
                { 60, "Seraph Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Mythic, 0, 10, null },

                { 61, "Shadow Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Common, 0, 2, 3 },
                { 62, "Gloom Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Uncommon, 0, 3, 3 },
                { 63, "Abyss Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Rare, 0, 4, 3 },
                { 64, "Void Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Epic, 0, 5, 3 },
                { 65, "Nether Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Legendary, 0, 7, 3 },
                { 66, "Nyx Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Mythic, 0, 10, 3 },

                { 67, "Shadow Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Common, 0, 2, null },
                { 68, "Gloom Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Uncommon, 0, 3, null },
                { 69, "Abyss Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Rare, 0, 4, null },
                { 70, "Void Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Epic, 0, 5, null },
                { 71, "Nether Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Legendary, 0, 7, null },
                { 72, "Nyx Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Mythic, 0, 10, null }
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT Runes OFF");

        migrationBuilder.Sql("SET IDENTITY_INSERT Ingredients ON");
        migrationBuilder.InsertData(
            table: "Ingredients",
            columns: new[] { "Id", "Name", "IngredientType" },
            values: new object[,]
            {
                { 1, "Fading Essence", (int)IngredientType.Essence }, //for runes at raritylevel.common
                { 2, "Stable Essence", (int)IngredientType.Essence }, //uncommon
                { 3, "Potent Essence", (int)IngredientType.Essence }, //rare
                { 4, "Refined Essence", (int)IngredientType.Essence }, //epic
                { 5, "Pristine Essence", (int)IngredientType.Essence }, //legendary
                { 6, "Eternal Essence", (int)IngredientType.Essence }, //mythic

                { 7, "Flame Shard", (int)IngredientType.Core }, //for runes of elementtype.fire
                { 8, "Arctic Shard", (int)IngredientType.Core }, //ice
                { 9, "Plasma Shard", (int)IngredientType.Core }, //lightning
                { 10, "Wild Shard", (int)IngredientType.Core }, //nature
                { 11, "Brilliant Shard", (int)IngredientType.Core }, //radiance
                { 12, "Sinister Shard", (int)IngredientType.Core }, //Abyssal
                //common monster parts
                { 13, "Bone Fragment", (int)IngredientType.MonsterPart },
                { 14, "Cracked Fang", (int)IngredientType.MonsterPart },
                { 15, "Tattered Hide", (int)IngredientType.MonsterPart },
                { 16, "Jagged Scale", (int)IngredientType.MonsterPart },
                { 17, "Brittle Talon", (int)IngredientType.MonsterPart },
                { 18, "Dulled Spike", (int)IngredientType.MonsterPart },
                { 19, "Sticky Ichor", (int)IngredientType.MonsterPart },
                { 20, "Crushed Eye Orb", (int)IngredientType.MonsterPart },
                { 21, "Thin Membrane", (int)IngredientType.MonsterPart },
                { 22, "Shriveled Gland", (int)IngredientType.MonsterPart },
                { 23, "Residual Puff", (int)IngredientType.MonsterPart },
                { 24, "Dull Residue", (int)IngredientType.MonsterPart },
                { 25, "Flickering Tendril", (int)IngredientType.MonsterPart },
                { 26, "Cloth Scrap", (int)IngredientType.MonsterPart },
                { 27, "Old Iron Nail", (int)IngredientType.MonsterPart },
                { 28, "Broken Weapon Bit", (int)IngredientType.MonsterPart },
                { 29, "Fraying Rope Fiber", (int)IngredientType.MonsterPart },
                //uncommon monster parts
                { 30, "Polished Fang", (int)IngredientType.MonsterPart },
                { 31, "Tough Claw", (int)IngredientType.MonsterPart },
                { 32, "Glinting Horn", (int)IngredientType.MonsterPart },
                { 33, "Sharpened Talon", (int)IngredientType.MonsterPart },
                { 34, "Razor Feather", (int)IngredientType.MonsterPart },
                { 35, "Pulsing Tendril", (int)IngredientType.MonsterPart },
                { 36, "Coiled Muscle Fiber", (int)IngredientType.MonsterPart },
                { 37, "Venom Gland", (int)IngredientType.MonsterPart },
                { 38, "Gleaming Husk", (int)IngredientType.MonsterPart },
                { 39, "Sap Cluster", (int)IngredientType.MonsterPart },
                { 40, "Luminous Web Strand", (int)IngredientType.MonsterPart },
                { 41, "Condensed Ichor", (int)IngredientType.MonsterPart },
                { 42, "Wispy Ether Silver", (int)IngredientType.MonsterPart },
                { 43, "Sturdy Hide", (int)IngredientType.MonsterPart },
                { 44, "Chipped Crystal", (int)IngredientType.MonsterPart },
                { 45, "Corrosive Slime Chunk", (int)IngredientType.MonsterPart },
                { 46, "Seared Scale", (int)IngredientType.MonsterPart },
                //rare monster parts
                { 47, "Eldritch Scale", (int)IngredientType.MonsterPart },
                { 48, "Bloodstained Claw", (int)IngredientType.MonsterPart },
                { 49, "Void-Touched Horn", (int)IngredientType.MonsterPart },
                { 50, "Celestial Feather", (int)IngredientType.MonsterPart },
                { 51, "Molten Core Shard", (int)IngredientType.MonsterPart },
                { 52, "Spectral Tendril", (int)IngredientType.MonsterPart },
                { 53, "Dragonbone Splinter", (int)IngredientType.MonsterPart },
                { 54, "Stormforged Spike", (int)IngredientType.MonsterPart },
                { 55, "Runebound Eye Orb", (int)IngredientType.MonsterPart },
                { 56, "Phantom Gland", (int)IngredientType.MonsterPart },
                { 57, "Titanhide Fragment", (int)IngredientType.MonsterPart },
                { 58, "Enchanted Venom Sac", (int)IngredientType.MonsterPart },
                { 59, "Crystalline Heartstone", (int)IngredientType.MonsterPart },
                { 60, "Abyssal Tendril", (int)IngredientType.MonsterPart },
                //epic monster parts
                { 61, "Phoenix Feather", (int)IngredientType.MonsterPart },
                { 62, "Titan’s Fang", (int)IngredientType.MonsterPart },
                { 63, "Ethereal Scale", (int)IngredientType.MonsterPart },
                { 64, "Infernal Claw", (int)IngredientType.MonsterPart },
                { 65, "Celestial Horn", (int)IngredientType.MonsterPart },
                { 66, "Runic Spine", (int)IngredientType.MonsterPart },
                { 67, "Dragonheart Shard", (int)IngredientType.MonsterPart },
                { 68, "Stormcaller’s Talon", (int)IngredientType.MonsterPart },
                { 69, "Void Crystal", (int)IngredientType.MonsterPart },
                { 70, "Soulbound Tendril", (int)IngredientType.MonsterPart },
                { 71, "Eclipse Eye", (int)IngredientType.MonsterPart },
                { 72, "Ancient Gland", (int)IngredientType.MonsterPart },
                { 73, "Primordial Hide", (int)IngredientType.MonsterPart },
                { 74, "Arcane Venom Sac", (int)IngredientType.MonsterPart },
                { 75, "Celestial Heartstone", (int)IngredientType.MonsterPart },
                //legendary monster parts
                { 76, "Eternal Dragon Scale", (int)IngredientType.MonsterPart },
                { 77, "Celestial Phoenix Claw", (int)IngredientType.MonsterPart },
                { 78, "Void Reaper Fang", (int)IngredientType.MonsterPart },
                { 79, "Titanheart Core", (int)IngredientType.MonsterPart },
                { 80, "Stormforged Talon", (int)IngredientType.MonsterPart },
                { 81, "Soulfire Essence", (int)IngredientType.MonsterPart },
                { 82, "Astral Horn Fragment", (int)IngredientType.MonsterPart },
                { 83, "Inferno Heartstone", (int)IngredientType.MonsterPart },
                { 84, "Primordial Bone Shard", (int)IngredientType.MonsterPart },
                { 85, "Voidwalker Tendril", (int)IngredientType.MonsterPart },
                { 86, "Eclipse Crystal Shard", (int)IngredientType.MonsterPart },
                { 87, "Mythic Venom Sac", (int)IngredientType.MonsterPart },
                { 88, "Celestial Heart of Gaia", (int)IngredientType.MonsterPart },
                { 89, "Dragonlord’s Fang", (int)IngredientType.MonsterPart },
                { 90, "Phoenix Soul Shard", (int)IngredientType.MonsterPart },
                //mythic monster parts
                // Fire
                { 91, "Emberwyrm Core", (int)IngredientType.MonsterPart },
                { 92, "Flareheart Shard", (int)IngredientType.MonsterPart },
                // Ice
                { 93, "Frostborn Crystal", (int)IngredientType.MonsterPart },
                { 94, "Glacierfang Fragment", (int)IngredientType.MonsterPart },
                // Lightning
                { 95, "Stormspark Tendril", (int)IngredientType.MonsterPart },
                { 96, "Thunderstrike Claw", (int)IngredientType.MonsterPart },
                // Nature
                { 97, "Verdant Heartwood", (int)IngredientType.MonsterPart },
                { 98, "Wildroot Bark", (int)IngredientType.MonsterPart },
                // Radiance
                { 99, "Solaris Ember", (int)IngredientType.MonsterPart },
                { 100, "Radiant Vein", (int)IngredientType.MonsterPart },
                // Abyssal
                { 101, "Nethercore Essence", (int)IngredientType.MonsterPart },
                { 102, "Shadowvein Tendril", (int)IngredientType.MonsterPart }
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT Ingredients OFF");

        migrationBuilder.Sql("SET IDENTITY_INSERT Recipes ON");
        migrationBuilder.InsertData(
             table: "Recipes",
             columns: new[] { "Id", "Name", "RuneId" },
             values: new object[,]
             {
                { 1, "Ember Rune (Weapon)", 1 },
                { 2, "Ember Rune (Armor)", 7 },
                { 3, "Fire Rune (Weapon)", 2 },
                { 4, "Fire Rune (Armor)", 8 },
                { 5, "Blaze Rune (Weapon)", 3 },
                { 6, "Blaze Rune (Armor)", 9 },
                { 7, "Lava Rune (Weapon)", 4 },
                { 8, "Lava Rune (Armor)", 10 },

                { 9, "Inferno Rune (Weapon)", 5 },
                { 10, "Inferno Rune (Armor)", 11 },

                { 11, "Phoenix Rune (Weapon)", 6 },
                { 12, "Phoenix Rune (Armor)", 12 },

                { 13, "Frost Rune (Weapon)", 13 },
                { 14, "Frost Rune (Armor)", 19 },
                { 15, "Ice Rune (Weapon)", 14 },
                { 16, "Ice Rune (Armor)", 20 },
                { 17, "Hail Rune (Weapon)", 15 },
                { 18, "Hail Rune (Armor)", 21 },
                { 19, "Glacier Rune (Weapon)", 16 },
                { 20, "Glacier Rune (Armor)", 22 },

                { 21, "Blizzard Rune (Weapon)", 17 },
                { 22, "Blizzard Rune (Armor)", 23 },

                { 23, "Ymir Rune (Weapon)", 18 },
                { 24, "Ymir Rune (Armor)", 24 },

                { 25, "Spark Rune (Weapon)", 25 },
                { 26, "Spark Rune (Armor)", 31 },
                { 27, "Jolt Rune (Weapon)", 26 },
                { 28, "Jolt Rune (Armor)", 32 },
                { 29, "Lightning Rune (Weapon)", 27 },
                { 30, "Lightning Rune (Armor)", 33 },
                { 31, "Storm Rune (Weapon)", 28 },
                { 32, "Storm Rune (Armor)", 34 },

                { 33, "Tempest Rune (Weapon)", 29 },
                { 34, "Tempest Rune (Armor)", 35 },

                { 35, "Raijin Rune (Weapon)", 30 },
                { 36, "Raijin Rune (Armor)", 36 },

                { 37, "Seed Rune (Weapon)", 37 },
                { 38, "Seed Rune (Armor)", 43 },
                { 39, "Nature Rune (Weapon)", 38 },
                { 40, "Nature Rune (Armor)", 44 },
                { 41, "Thorn Rune (Weapon)", 39 },
                { 42, "Thorn Rune (Armor)", 45 },
                { 43, "Beast Rune (Weapon)", 40 },
                { 44, "Beast Rune (Armor)", 46 },

                { 45, "Quake Rune (Weapon)", 41 },
                { 46, "Quake Rune (Armor)", 47 },

                { 47, "Gaia Rune (Weapon)", 42 },
                { 48, "Gaia Rune (Armor)", 48 },

                { 49, "Bright Rune (Weapon)", 49 },
                { 50, "Bright Rune (Armor)", 55 },
                { 51, "Energy Rune (Weapon)", 50 },
                { 52, "Energy Rune (Armor)", 56 },
                { 53, "Radiant Rune (Weapon)", 51 },
                { 54, "Radiant Rune (Armor)", 57 },
                { 55, "Solar Rune (Weapon)", 52 },
                { 56, "Solar Rune (Armor)", 58 },

                { 57, "Divine Rune (Weapon)", 53 },
                { 58, "Divine Rune (Armor)", 59 },

                { 59, "Seraph Rune (Weapon)", 54 },
                { 60, "Seraph Rune (Armor)", 60 },

                { 61, "Shadow Rune (Weapon)", 61 },
                { 62, "Shadow Rune (Armor)", 67 },
                { 63, "Gloom Rune (Weapon)", 62 },
                { 64, "Gloom Rune (Armor)", 68 },
                { 65, "Abyss Rune (Weapon)", 63 },
                { 66, "Abyss Rune (Armor)", 69 },
                { 67, "Void Rune (Weapon)", 64 },
                { 68, "Void Rune (Armor)", 70 },

                { 69, "Nether Rune (Weapon)", 65 },
                { 70, "Nether Rune (Armor)", 71 },

                { 71, "Nyx Rune (Weapon)", 66 },
                { 72, "Nyx Rune (Armor)", 72 }
             });
        migrationBuilder.Sql("SET IDENTITY_INSERT Recipes OFF");

        migrationBuilder.Sql("SET IDENTITY_INSERT RecipeIngredients ON");
        migrationBuilder.InsertData(
            table: "RecipeIngredients",
            columns: new[] { "Id", "RecipeId", "IngredientId", "Quantity" },
            values: new object[,]
            {
                // Ember Rune (Weapon)
                { 1, 1, 1, 3 }, // 3 Fading Essence
                { 2, 1, 7, 5 }, // 5 Flame Shard
                { 3, 1, 17, 3 }, // 3 Brittle Talon
                // Ember Rune (Armor)
                { 4, 2, 1, 3 }, // 3 Fading Essence
                { 5, 2, 7, 5 }, //  5 Flame Shard
                { 6, 2, 23, 3 }, // 3 Residual Puff
                //Frost Rune (Weapon)
                { 7, 13, 1, 3 }, // 3 Fading Essence
                { 8, 13, 8, 5 }, // 5 Arctic Shard
                { 9, 13, 19, 3 }, // 3 Thin Membrane
                // Frost Rune (Armor)
                { 10, 14, 1, 3 }, // 3 Fading Essence
                { 11, 14, 8, 5 }, // 5 Arctic Shard
                { 12, 14, 24, 3 }, // 3 Dull Residue
                //Spark Rune (Weapon)
                { 13, 25, 1, 3 }, // 3 Fading Essence
                { 14, 25, 9, 5 }, // 5 Plasma Shard
                { 15, 25, 20, 3 }, // 3 Shriveled Gland
                // Spark Rune (Armor)
                { 16, 26, 1, 3 }, // 3 Fading Essence
                { 17, 26, 9, 5 }, // 5 Plasma Shard
                { 18, 26, 25, 3 }, // 3 Flickering Tendril
                //Seed Rune (Weapon)
                { 19, 37, 1, 3 }, // 3 Fading Essence
                { 20, 37, 10, 5 }, // 5 Wild Shard
                { 21, 37, 26, 3 }, // 3 Dulled Spike
                // Seed Rune (Armor)
                { 22, 38, 1, 3 }, // 3 Fading Essence
                { 23, 38, 10, 5 }, // 5 Wild Shard
                { 24, 38, 29, 3 }, // 3 Fraying Rope Fiber
                //Bright Rune (Weapon)
                { 25, 49, 1, 3 }, // 3 Fading Essence
                { 26, 49, 11, 5 }, // 5 Brilliant Shard
                { 27, 49, 27, 3 }, // 3 Cloth Scrap
                // Bright Rune (Armor)
                { 28, 50, 1, 3 }, // 3 Fading Essence
                { 29, 50, 11, 5 }, // 5 Brilliant Shard
                { 30, 50, 13, 3 }, // 3 Bone Fragment
                //Shadow Rune (Weapon)
                { 31, 61, 1, 3 }, // 3 Fading Essence
                { 32, 61, 12, 5 }, // 5 Sinister Shard
                { 33, 61, 28, 3 }, // 3 Broken Weapon Bit
                //Shadow Rune (Armor)
                { 34, 62, 1, 3 }, // 3 Fading Essence
                { 35, 62, 12, 5 }, // 5 Sinister Shard
                { 36, 62, 16, 3 }, // 3 Jagged Scale

                // Fire Rune (Weapon)
                { 37, 3, 2, 5 }, // Stable Essence
                { 38, 3, 7, 7 }, // Flame Shard
                { 39, 3, 14, 5 }, // Cracked Fang
                { 40, 3, 30, 3 }, // Polished Fang
                // Fire Rune (Armor)
                { 41, 4, 2, 5 },
                { 42, 4, 7, 7 },
                { 43, 4, 13, 5 }, // Bone Fragment
                { 44, 4, 31, 3 }, // Tough Claw
                // Ice Rune (Weapon)
                { 45, 15, 2, 5 },
                { 46, 15, 8, 7 },
                { 47, 15, 16, 5 }, // Jagged Scale
                { 48, 15, 33, 3 }, // Sharpened Talon
                // Ice Rune (Armor)
                { 49, 16, 2, 5 },
                { 50, 16, 8, 7 },
                { 51, 16, 15, 5 }, // Tattered Hide
                { 52, 16, 34, 3 }, // Razor Feather
                // Jolt Rune (Weapon)
                { 53, 27, 2, 5 },
                { 54, 27, 9, 7 },
                { 55, 27, 21, 5 }, // Thin Membrane
                { 56, 27, 35, 3 }, // Pulsing Tendril
                // Jolt Rune (Armor)
                { 57, 28, 2, 5 },
                { 58, 28, 9, 7 },
                { 59, 28, 22, 5 }, // Shriveled Gland
                { 60, 28, 36, 3 }, // Coiled Muscle Fiber
                // Nature Rune (Weapon)
                { 61, 39, 2, 5 },
                { 62, 39, 10, 7 },
                { 63, 39, 18, 5 }, // Dulled Spike
                { 64, 39, 37, 3 }, // Venom Gland
                // Nature Rune (Armor)
                { 65, 40, 2, 5 },
                { 66, 40, 10, 7 },
                { 67, 40, 19, 5 }, // Sticky Ichor
                { 68, 40, 39, 3 }, // Sap Cluster
                // Energy Rune (Weapon)
                { 69, 51, 2, 5 },
                { 70, 51, 11, 7 },
                { 71, 51, 17, 5 }, // Brittle Talon
                { 72, 51, 32, 3 }, // Glinting Horn
                // Energy Rune (Armor)
                { 73, 52, 2, 5 },
                { 74, 52, 11, 7 },
                { 75, 52, 20, 5 }, // Crushed Eye Orb
                { 76, 52, 40, 3 }, // Luminous Web Strand
                // Gloom Rune (Weapon)
                { 77, 63, 2, 5 },
                { 78, 63, 12, 7 },
                { 79, 63, 23, 5 }, // Residual Puff
                { 80, 63, 38, 3 }, // Gleaming Husk
                // Gloom Rune (Armor)
                { 81, 64, 2, 5 },
                { 82, 64, 12, 7 },
                { 83, 64, 24, 5 }, // Dull Residue
                { 84, 64, 41, 3 },  // Condensed Ichor

                // Blaze Rune (Weapon)
                { 85, 5, 3, 7 },   // Potent Essence
                { 86, 5, 7, 9 },   // Flame Shard
                { 87, 5, 30, 5 },  // Polished Fang
                { 88, 5, 51, 3 },  // Molten Core Shard
                { 89, 5, 47, 1 },  // Eldritch Scale
                // Blaze Rune (Armor)
                { 90, 6, 3, 7 },
                { 91, 6, 7, 9 },
                { 92, 6, 31, 5 },  // Tough Claw
                { 93, 6, 55, 3 },  // Runebound Eye Orb
                { 94, 6, 48, 1 },  // Bloodstained Claw
                // Hail Rune (Weapon)
                { 95, 17, 3, 7 },
                { 96, 17, 8, 9 },
                { 97, 17, 33, 5 }, // Sharpened Talon
                { 98, 17, 56, 3 }, // Phantom Gland
                { 99, 17, 48, 1 }, // Bloodstained Claw
                // Hail Rune (Armor)
                { 100, 18, 3, 7 },
                { 101, 18, 8, 9 },
                { 102, 18, 34, 5 }, // Razor Feather
                { 103, 18, 57, 3 }, // Titanhide Fragment
                { 104, 18, 49, 1 }, // Void-Touched Horn
                // Lightning Rune (Weapon)
                { 105, 29, 3, 7 },
                { 106, 29, 9, 9 },
                { 107, 29, 35, 5 }, // Pulsing Tendril
                { 108, 29, 54, 3 }, // Stormforged Spike
                { 109, 29, 49, 1 }, // Void-Touched Horn
                // Lightning Rune (Armor)
                { 110, 30, 3, 7 },
                { 111, 30, 9, 9 },
                { 112, 30, 36, 5 }, // Coiled Muscle Fiber
                { 113, 30, 53, 3 }, // Dragonbone Splinter
                { 114, 30, 50, 1 }, // Celestial Feather
                // Thorn Rune (Weapon)
                { 115, 41, 3, 7 },
                { 116, 41, 10, 9 },
                { 117, 41, 37, 5 }, // Venom Gland
                { 118, 41, 58, 3 }, // Enchanted Venom Sac
                { 119, 41, 57, 1 }, // Titanhide Fragment
                // Thorn Rune (Armor)
                { 120, 42, 3, 7 },
                { 121, 42, 10, 9 },
                { 122, 42, 39, 5 }, // Sap Cluster
                { 123, 42, 52, 3 }, // Spectral Tendril
                { 124, 42, 46, 1 }, // Seared Scale
                // Radiant Rune (Weapon)
                { 125, 53, 3, 7 },
                { 126, 53, 11, 9 },
                { 127, 53, 40, 5 }, // Luminous Web Strand
                { 128, 53, 59, 3 }, // Crystalline Heartstone
                { 129, 53, 50, 1 }, // Celestial Feather
                // Radiant Rune (Armor)
                { 130, 54, 3, 7 },
                { 131, 54, 11, 9 },
                { 132, 54, 42, 5 }, // Wispy Ether Silver
                { 133, 54, 53, 3 }, // Dragonbone Splinter
                { 134, 54, 55, 1 }, // Runebound Eye Orb
                // Abyss Rune (Weapon)
                { 135, 65, 3, 7 },
                { 136, 65, 12, 9 },
                { 137, 65, 41, 5 }, // Condensed Ichor
                { 138, 65, 60, 3 }, // Abyssal Tendril
                { 139, 65, 52, 1 }, // Spectral Tendril
                // Abyss Rune (Armor)
                { 140, 66, 3, 7 },
                { 141, 66, 12, 9 },
                { 142, 66, 43, 5 }, // Sturdy Hide
                { 143, 66, 59, 3 }, // Crystalline Heartstone
                { 144, 66, 54, 1 },  // Stormforged Spike

                // Lava Rune (Weapon: 7)
                { 145, 7, 4, 9 },  // Refined Essence
                { 146, 7, 7, 11 }, // Flame Core
                { 147, 7, 47, 3 }, // Eldritch Scale
                { 148, 7, 51, 7 }, // Molten Core Shard
                { 149, 7, 61, 2 }, // Phoenix Feather
                { 150, 7, 64, 3 }, // Infernal Claw
                // Lava Rune (Armor: 8)
                { 151, 8, 4, 9 },
                { 152, 8, 7, 11 },
                { 153, 8, 48, 3 }, // Bloodstained Claw
                { 154, 8, 53, 7 }, // Dragonbone Splinter
                { 155, 8, 62, 2 }, // Titan’s Fang
                { 156, 8, 67, 3 }, // Dragonheart Shard
                // Glacier Rune (Weapon: 19)
                { 157, 19, 4, 9 },
                { 158, 19, 8, 11 }, // Arctic Core
                { 159, 19, 49, 3 }, // Void-Touched Horn
                { 160, 19, 55, 7 }, // Runebound Eye Orb
                { 161, 19, 63, 2 }, // Ethereal Scale
                { 162, 19, 66, 3 }, // Runic Spine
                // Glacier Rune (Armor: 20)
                { 163, 20, 4, 9 },
                { 164, 20, 8, 11 },
                { 165, 20, 50, 3 }, // Celestial Feather
                { 166, 20, 56, 7 }, // Phantom Gland
                { 167, 20, 65, 2 }, // Celestial Horn
                { 168, 20, 72, 3 }, // Ancient Gland
                // Storm Rune (Weapon: 31)
                { 169, 31, 4, 9 },
                { 170, 31, 9, 11 }, // Plasma Core
                { 171, 31, 52, 3 }, // Spectral Tendril
                { 172, 31, 54, 7 }, // Stormforged Spike
                { 173, 31, 68, 2 }, // Stormcaller’s Talon
                { 174, 31, 70, 3 }, // Soulbound Tendril
                // Storm Rune (Armor: 32)
                { 175, 32, 4, 9 },
                { 176, 32, 9, 11 },
                { 177, 32, 57, 3 }, // Titanhide Fragment
                { 178, 32, 58, 7 }, // Enchanted Venom Sac
                { 179, 32, 69, 2 }, // Void Crystal
                { 180, 32, 73, 3 }, // Primordial Hide
                // Beast Rune (Weapon: 43)
                { 181, 43, 4, 9 },
                { 182, 43, 10, 11 }, // Wild Core
                { 183, 43, 59, 3 }, // Crystalline Heartstone
                { 184, 43, 60, 7 }, // Abyssal Tendril
                { 185, 43, 71, 2 }, // Eclipse Eye
                { 186, 43, 74, 3 }, // Arcane Venom Sac
                // Beast Rune (Armor: 44)
                { 187, 44, 4, 9 },
                { 188, 44, 10, 11 },
                { 189, 44, 47, 3 }, // Eldritch Scale (reused)
                { 190, 44, 48, 7 }, // Bloodstained Claw (reused)
                { 191, 44, 62, 2 }, // Titan’s Fang (reused)
                { 192, 44, 75, 3 }, // Celestial Heartstone
                // Solar Rune (Weapon: 55)
                { 193, 55, 4, 9 },
                { 194, 55, 11, 11 }, // Brilliant Core
                { 195, 55, 49, 3 }, // Void-Touched Horn (reused)
                { 196, 55, 50, 7 }, // Celestial Feather (reused)
                { 197, 55, 63, 2 }, // Ethereal Scale (reused)
                { 198, 55, 61, 3 }, // Phoenix Feather (reused)
                // Solar Rune (Armor: 56)
                { 199, 56, 4, 9 },
                { 200, 56, 11, 11 },
                { 201, 56, 51, 3 }, // Molten Core Shard (reused)
                { 202, 56, 53, 7 }, // Dragonbone Splinter (reused)
                { 203, 56, 67, 2 }, // Dragonheart Shard (reused)
                { 204, 56, 75, 3 }, // Celestial Heartstone (reused)
                // Void Rune (Weapon: 67)
                { 205, 67, 4, 9 },
                { 206, 67, 12, 11 }, // Sinister Core
                { 207, 67, 58, 3 }, // Enchanted Venom Sac (reused)
                { 208, 67, 52, 7 }, // Spectral Tendril (reused)
                { 209, 67, 66, 2 }, // Runic Spine (reused)
                { 210, 67, 69, 3 }, // Void Crystal (reused)
                // Void Rune (Armor: 68)
                { 211, 68, 4, 9 },
                { 212, 68, 12, 11 },
                { 213, 68, 54, 3 }, // Stormforged Spike (reused)
                { 214, 68, 55, 7 }, // Runebound Eye Orb (reused)
                { 215, 68, 70, 2 }, // Soulbound Tendril (reused)
                { 216, 68, 73, 3 },  // Primordial Hide (reused)

                // Inferno Rune
                { 217, 9, 5, 11 },
                { 218, 9, 7, 13 },
                { 219, 9, 76, 1 },
                { 220, 9, 83, 2 },
                { 221, 9, 89, 1 },
                { 222, 10, 5, 11 },
                { 223, 10, 7, 13 },
                { 224, 10, 76, 2 },
                { 225, 10, 83, 1 },
                { 226, 10, 89, 1 },
                // Blizzard Rune
                { 227, 21, 5, 11 },
                { 228, 21, 8, 13 },
                { 229, 21, 77, 1 },
                { 230, 21, 86, 2 },
                { 231, 21, 90, 1 },
                { 232, 22, 5, 11 },
                { 233, 22, 8, 13 },
                { 234, 22, 77, 1 },
                { 235, 22, 86, 2 },
                { 236, 22, 90, 1 },
                // Tempest Rune
                { 237, 33, 5, 11 },
                { 238, 33, 9, 13 },
                { 239, 33, 80, 1 },
                { 240, 33, 81, 1 },
                { 241, 33, 96, 2 },
                { 242, 34, 5, 11 },
                { 243, 34, 9, 13 },
                { 244, 34, 80, 1 },
                { 245, 34, 81, 2 },
                { 246, 34, 96, 1 },
                // Quake Rune
                { 247, 45, 5, 11 },
                { 248, 45, 10, 13 },
                { 249, 45, 84, 1 },
                { 250, 45, 88, 1 },
                { 251, 45, 98, 2 },
                { 252, 46, 5, 11 },
                { 253, 46, 10, 13 },
                { 254, 46, 84, 2 },
                { 255, 46, 88, 1 },
                { 256, 46, 98, 1 },
                // Divine Rune
                { 257, 57, 5, 11 },
                { 258, 57, 11, 13 },
                { 259, 57, 82, 1 },
                { 260, 57, 90, 2 },
                { 261, 57, 75, 1 },
                { 262, 58, 5, 11 },
                { 263, 58, 11, 13 },
                { 264, 58, 82, 1 },
                { 265, 58, 90, 1 },
                { 266, 58, 75, 2 },
                // Nether Rune
                { 267, 69, 5, 11 },
                { 268, 69, 12, 13 },
                { 269, 69, 78, 2 },
                { 270, 69, 85, 1 },
                { 271, 69, 86, 1 },
                { 272, 70, 5, 11 },
                { 273, 70, 12, 13 },
                { 274, 70, 78, 1 },
                { 275, 70, 85, 2 },
                { 276, 70, 86, 1 },

                // Phoenix Rune
                { 277, 11, 6, 15 },
                { 278, 11, 7, 20 },
                { 279, 11, 83, 5 },
                { 280, 11, 89, 5 },
                { 281, 11, 91, 1 }, // Emberwyrm Core (weapon)
                { 282, 12, 6, 15 },
                { 283, 12, 7, 20 },
                { 284, 12, 83, 5 },
                { 285, 12, 89, 5 },
                { 286, 12, 92, 1 }, // Flareheart Shard (armor)
                // Ymir Rune
                { 287, 23, 6, 15 },
                { 288, 23, 8, 20 },
                { 289, 23, 86, 5 },
                { 290, 23, 90, 5 },
                { 291, 23, 93, 1 }, // Frostborn Crystal (weapon)
                { 292, 24, 6, 15 },
                { 293, 24, 8, 20 },
                { 294, 24, 86, 5 },
                { 295, 24, 90, 5 },
                { 296, 24, 94, 1 }, // Glacierfang Fragment (armor)
                // Raijin Rune
                { 297, 35, 6, 15 },
                { 298, 35, 9, 20 },
                { 299, 35, 81, 5 },
                { 300, 35, 96, 5 },
                { 301, 35, 95, 1 }, // Stormspark Tendril (weapon)
                { 302, 36, 6, 15 },
                { 303, 36, 9, 20 },
                { 304, 36, 81, 5 },
                { 305, 36, 96, 5 },
                { 306, 36, 96, 1 }, // Thunderstrike Claw (armor)
                // Gaia Rune
                { 307, 47, 6, 15 },
                { 308, 47, 10, 20 },
                { 309, 47, 84, 5 },
                { 310, 47, 98, 5 },
                { 311, 47, 97, 1 }, // Verdant Heartwood (weapon)
                { 312, 48, 6, 15 },
                { 313, 48, 10, 20 },
                { 314, 48, 84, 5 },
                { 315, 48, 98, 5 },
                { 316, 48, 98, 1 }, // Wildroot Bark (armor)
                // Seraph Rune
                { 317, 59, 6, 15 },
                { 318, 59, 1, 20 },
                { 319, 59, 82, 5 },
                { 320, 59, 90, 5 },
                { 321, 59, 99, 1 }, // Solaris Ember (weapon)
                { 322, 60, 6, 15 },
                { 323, 60, 11, 20 },
                { 324, 60, 82, 5 },
                { 325, 60, 90, 5 },
                { 326, 60, 100, 1 }, // Radiant Vein (armor)
                // Nyx Rune
                { 327, 71, 6, 15 },
                { 328, 71, 12, 20 },
                { 329, 71, 85, 5 },
                { 330, 71, 86, 5 },
                { 331, 71, 101, 1 }, // Nethercore Essence (weapon)
                { 332, 72, 6, 15 },
                { 333, 72, 12, 20 },
                { 334, 72, 85, 5 },
                { 335, 72, 86, 5 },
                { 336, 72, 102, 1 }  // Shadowvein Tendril (armor)
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT RecipeIngredients OFF");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM RecipeIngredients WHERE Id BETWEEN 1 AND 336");
        migrationBuilder.Sql("DELETE FROM Recipes WHERE Id BETWEEN 1 AND 72");
        migrationBuilder.Sql("DELETE FROM Runes WHERE Id BETWEEN 1 AND 72");
        migrationBuilder.Sql("DELETE FROM Ingredients WHERE Id BETWEEN 1 AND 102"); 
    }
}