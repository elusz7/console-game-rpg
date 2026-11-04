using Microsoft.EntityFrameworkCore.Migrations;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class AddRuneAndIngredientSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Seed Runes table
        migrationBuilder.Sql("SET IDENTITY_INSERT Runes ON");
        migrationBuilder.InsertData(
            table: "Runes",
            columns: new[] { "Id", "Name", "RuneType", "Element", "Rarity", "Power", "Duration" },
            values: new object[,]
            {
                // Fire Weapon Runes
                { 1, "Ember Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Common, 2, 3 },
                { 2, "Fire Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Uncommon, 3, 3 },
                { 3, "Blaze Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Rare, 4, 3 },
                { 4, "Lava Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Epic, 5, 3 },
                { 5, "Inferno Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Legendary, 7, 3 },
                { 6, "Phoenix Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Mythic, 10, 3 },

                // Fire Armor Runes
                { 7, "Ember Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Common, 2, null },
                { 8, "Fire Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Uncommon, 3, null },
                { 9, "Blaze Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Rare, 4, null },
                { 10, "Lava Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Epic, 5, null },
                { 11, "Inferno Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Legendary, 7, null },
                { 12, "Phoenix Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Mythic, 10, null },

                // Ice Weapon Runes
                { 13, "Frost Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Common, 2, 3 },
                { 14, "Ice Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Uncommon, 3, 3 },
                { 15, "Hail Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Rare, 4, 3 },
                { 16, "Glacier Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Epic, 5, 3 },
                { 17, "Blizzard Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Legendary, 7, 3 },
                { 18, "Ymir Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Mythic, 10, 3 },

                // Ice Armor Runes
                { 19, "Frost Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Common, 2, null },
                { 20, "Ice Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Uncommon, 3, null },
                { 21, "Hail Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Rare, 4, null },
                { 22, "Glacier Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Epic, 5, null },
                { 23, "Blizzard Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Legendary, 7, null },
                { 24, "Ymir Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Mythic, 10, null },

                // Lightning Weapon Runes
                { 25, "Spark Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Common, 2, 3 },
                { 26, "Jolt Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Uncommon, 3, 3 },
                { 27, "Lightning Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Rare, 4, 3 },
                { 28, "Storm Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Epic, 5, 3 },
                { 29, "Tempest Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Legendary, 7, 3 },
                { 30, "Raijin Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Mythic, 10, 3 },

                // Lightning Armor Runes
                { 31, "Spark Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Common, 2, null },
                { 32, "Jolt Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Uncommon, 3, null },
                { 33, "Lightning Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Rare, 4, null },
                { 34, "Storm Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Epic, 5, null },
                { 35, "Tempest Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Legendary, 7, null },
                { 36, "Raijin Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Mythic, 10, null },

                // Nature Weapon Runes
                { 37, "Seed Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Common, 2, 3 },
                { 38, "Nature Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Uncommon, 3, 3 },
                { 39, "Thorn Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Rare, 4, 3 },
                { 40, "Beast Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Epic, 5, 3 },
                { 41, "Quake Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Legendary, 7, 3 },
                { 42, "Gaia Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Mythic, 10, 3 },

                // Nature Armor Runes
                { 43, "Seed Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Common, 2, null },
                { 44, "Nature Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Uncommon, 3, null },
                { 45, "Thorn Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Rare, 4, null },
                { 46, "Beast Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Epic, 5, null },
                { 47, "Quake Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Legendary, 7, null },
                { 48, "Gaia Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Mythic, 10, null },

                // Radiance Weapon Runes
                { 49, "Bright Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Common, 2, 3 },
                { 50, "Energy Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Uncommon, 3, 3 },
                { 51, "Radiant Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Rare, 4, 3 },
                { 52, "Solar Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Epic, 5, 3 },
                { 53, "Divine Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Legendary, 7, 3 },
                { 54, "Seraph Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Mythic, 10, 3 },

                // Radiance Armor Runes
                { 55, "Bright Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Common, 2, null },
                { 56, "Energy Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Uncommon, 3, null },
                { 57, "Radiant Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Rare, 4, null },
                { 58, "Solar Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Epic, 5, null },
                { 59, "Divine Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Legendary, 7, null },
                { 60, "Seraph Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Mythic, 10, null },

                // Abyssal Weapon Runes
                { 61, "Shadow Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Common, 2, 3 },
                { 62, "Gloom Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Uncommon, 3, 3 },
                { 63, "Abyss Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Rare, 4, 3 },
                { 64, "Void Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Epic, 5, 3 },
                { 65, "Nether Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Legendary, 7, 3 },
                { 66, "Nyx Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Mythic, 10, 3 },

                // Abyssal Armor Runes
                { 67, "Shadow Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Common, 2, null },
                { 68, "Gloom Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Uncommon, 3, null },
                { 69, "Abyss Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Rare, 4, null },
                { 70, "Void Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Epic, 5, null },
                { 71, "Nether Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Legendary, 7, null },
                { 72, "Nyx Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Mythic, 10, null }
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT Runes OFF");

        // Seed Ingredients table
        migrationBuilder.Sql("SET IDENTITY_INSERT Ingredients ON");
        migrationBuilder.InsertData(
            table: "Ingredients",
            columns: new[] { "Id", "Name", "ComponentType" },
            values: new object[,]
            {
                // Essences (for transmutation)
                { 1, "Fading Essence", (int)ComponentType.Essence },
                { 2, "Stable Essence", (int)ComponentType.Essence },
                { 3, "Potent Essence", (int)ComponentType.Essence },
                { 4, "Refined Essence", (int)ComponentType.Essence },
                { 5, "Pristine Essence", (int)ComponentType.Essence },
                { 6, "Eternal Essence", (int)ComponentType.Essence },

                // Element Cores
                { 7, "Flame Shard", (int)ComponentType.Core },
                { 8, "Arctic Shard", (int)ComponentType.Core },
                { 9, "Plasma Shard", (int)ComponentType.Core },
                { 10, "Wild Shard", (int)ComponentType.Core },
                { 11, "Brilliant Shard", (int)ComponentType.Core },
                { 12, "Sinister Shard", (int)ComponentType.Core },

                // Common Monster Parts
                { 13, "Bone Fragment", (int)ComponentType.MonsterPart },
                { 14, "Cracked Fang", (int)ComponentType.MonsterPart },
                { 15, "Tattered Hide", (int)ComponentType.MonsterPart },
                { 16, "Jagged Scale", (int)ComponentType.MonsterPart },
                { 17, "Brittle Talon", (int)ComponentType.MonsterPart },
                { 18, "Dulled Spike", (int)ComponentType.MonsterPart },
                { 19, "Sticky Ichor", (int)ComponentType.MonsterPart },
                { 20, "Crushed Eye Orb", (int)ComponentType.MonsterPart },
                { 21, "Thin Membrane", (int)ComponentType.MonsterPart },
                { 22, "Shriveled Gland", (int)ComponentType.MonsterPart },
                { 23, "Residual Puff", (int)ComponentType.MonsterPart },
                { 24, "Dull Residue", (int)ComponentType.MonsterPart },
                { 25, "Flickering Tendril", (int)ComponentType.MonsterPart },
                { 26, "Cloth Scrap", (int)ComponentType.MonsterPart },
                { 27, "Old Iron Nail", (int)ComponentType.MonsterPart },
                { 28, "Broken Weapon Bit", (int)ComponentType.MonsterPart },
                { 29, "Fraying Rope Fiber", (int)ComponentType.MonsterPart },

                // Uncommon Monster Parts
                { 30, "Polished Fang", (int)ComponentType.MonsterPart },
                { 31, "Tough Claw", (int)ComponentType.MonsterPart },
                { 32, "Glinting Horn", (int)ComponentType.MonsterPart },
                { 33, "Sharpened Talon", (int)ComponentType.MonsterPart },
                { 34, "Razor Feather", (int)ComponentType.MonsterPart },
                { 35, "Pulsing Tendril", (int)ComponentType.MonsterPart },
                { 36, "Coiled Muscle Fiber", (int)ComponentType.MonsterPart },
                { 37, "Venom Gland", (int)ComponentType.MonsterPart },
                { 38, "Gleaming Husk", (int)ComponentType.MonsterPart },
                { 39, "Sap Cluster", (int)ComponentType.MonsterPart },
                { 40, "Luminous Web Strand", (int)ComponentType.MonsterPart },
                { 41, "Condensed Ichor", (int)ComponentType.MonsterPart },
                { 42, "Wispy Ether Silver", (int)ComponentType.MonsterPart },
                { 43, "Sturdy Hide", (int)ComponentType.MonsterPart },
                { 44, "Chipped Crystal", (int)ComponentType.MonsterPart },
                { 45, "Corrosive Slime Chunk", (int)ComponentType.MonsterPart },
                { 46, "Seared Scale", (int)ComponentType.MonsterPart },

                // Rare Monster Parts
                { 47, "Eldritch Scale", (int)ComponentType.MonsterPart },
                { 48, "Bloodstained Claw", (int)ComponentType.MonsterPart },
                { 49, "Void-Touched Horn", (int)ComponentType.MonsterPart },
                { 50, "Celestial Feather", (int)ComponentType.MonsterPart },
                { 51, "Molten Core Shard", (int)ComponentType.MonsterPart },
                { 52, "Spectral Tendril", (int)ComponentType.MonsterPart },
                { 53, "Dragonbone Splinter", (int)ComponentType.MonsterPart },
                { 54, "Stormforged Spike", (int)ComponentType.MonsterPart },
                { 55, "Runebound Eye Orb", (int)ComponentType.MonsterPart },
                { 56, "Phantom Gland", (int)ComponentType.MonsterPart },
                { 57, "Titanhide Fragment", (int)ComponentType.MonsterPart },
                { 58, "Enchanted Venom Sac", (int)ComponentType.MonsterPart },
                { 59, "Crystalline Heartstone", (int)ComponentType.MonsterPart },
                { 60, "Abyssal Tendril", (int)ComponentType.MonsterPart },

                // Epic Monster Parts
                { 61, "Phoenix Feather", (int)ComponentType.MonsterPart },
                { 62, "Titan's Fang", (int)ComponentType.MonsterPart },
                { 63, "Ethereal Scale", (int)ComponentType.MonsterPart },
                { 64, "Infernal Claw", (int)ComponentType.MonsterPart },
                { 65, "Celestial Horn", (int)ComponentType.MonsterPart },
                { 66, "Runic Spine", (int)ComponentType.MonsterPart },
                { 67, "Dragonheart Shard", (int)ComponentType.MonsterPart },
                { 68, "Stormcaller's Talon", (int)ComponentType.MonsterPart },
                { 69, "Void Crystal", (int)ComponentType.MonsterPart },
                { 70, "Soulbound Tendril", (int)ComponentType.MonsterPart },
                { 71, "Eclipse Eye", (int)ComponentType.MonsterPart },
                { 72, "Ancient Gland", (int)ComponentType.MonsterPart },
                { 73, "Primordial Hide", (int)ComponentType.MonsterPart },
                { 74, "Arcane Venom Sac", (int)ComponentType.MonsterPart },
                { 75, "Celestial Heartstone", (int)ComponentType.MonsterPart },

                // Legendary Monster Parts
                { 76, "Eternal Dragon Scale", (int)ComponentType.MonsterPart },
                { 77, "Celestial Phoenix Claw", (int)ComponentType.MonsterPart },
                { 78, "Void Reaper Fang", (int)ComponentType.MonsterPart },
                { 79, "Titanheart Core", (int)ComponentType.MonsterPart },
                { 80, "Stormforged Talon", (int)ComponentType.MonsterPart },
                { 81, "Soulfire Essence", (int)ComponentType.MonsterPart },
                { 82, "Astral Horn Fragment", (int)ComponentType.MonsterPart },
                { 83, "Inferno Heartstone", (int)ComponentType.MonsterPart },
                { 84, "Primordial Bone Shard", (int)ComponentType.MonsterPart },
                { 85, "Voidwalker Tendril", (int)ComponentType.MonsterPart },
                { 86, "Eclipse Crystal Shard", (int)ComponentType.MonsterPart },
                { 87, "Mythic Venom Sac", (int)ComponentType.MonsterPart },
                { 88, "Celestial Heart of Gaia", (int)ComponentType.MonsterPart },
                { 89, "Dragonlord's Fang", (int)ComponentType.MonsterPart },
                { 90, "Phoenix Soul Shard", (int)ComponentType.MonsterPart },

                // Mythic Monster Parts
                { 91, "Emberwyrm Core", (int)ComponentType.MonsterPart },
                { 92, "Flareheart Shard", (int)ComponentType.MonsterPart },
                { 93, "Frostborn Crystal", (int)ComponentType.MonsterPart },
                { 94, "Glacierfang Fragment", (int)ComponentType.MonsterPart },
                { 95, "Stormspark Tendril", (int)ComponentType.MonsterPart },
                { 96, "Thunderstrike Claw", (int)ComponentType.MonsterPart },
                { 97, "Verdant Heartwood", (int)ComponentType.MonsterPart },
                { 98, "Wildroot Bark", (int)ComponentType.MonsterPart },
                { 99, "Solaris Ember", (int)ComponentType.MonsterPart },
                { 100, "Radiant Vein", (int)ComponentType.MonsterPart },
                { 101, "Nethercore Essence", (int)ComponentType.MonsterPart },
                { 102, "Shadowvein Tendril", (int)ComponentType.MonsterPart }
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT Ingredients OFF");

        // Seed Recipes table
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

        // Seed RecipeIngredients table
        migrationBuilder.Sql("SET IDENTITY_INSERT RecipeIngredients ON");
        migrationBuilder.InsertData(
            table: "RecipeIngredients",
            columns: new[] { "Id", "RecipeId", "IngredientId", "Quantity" },
            values: new object[,]
            {
                // Common runes - Ember (Weapon)
                { 1, 1, 1, 3 }, { 2, 1, 7, 1 }, { 3, 1, 17, 3 },
                // Common runes - Ember (Armor)
                { 4, 2, 1, 3 }, { 5, 2, 7, 1 }, { 6, 2, 23, 3 },
                // Common runes - Frost (Weapon)
                { 7, 13, 1, 3 }, { 8, 13, 8, 1 }, { 9, 13, 19, 3 },
                // Common runes - Frost (Armor)
                { 10, 14, 1, 3 }, { 11, 14, 8, 1 }, { 12, 14, 24, 3 },
                // Common runes - Spark (Weapon)
                { 13, 25, 1, 3 }, { 14, 25, 9, 1 }, { 15, 25, 20, 3 },
                // Common runes - Spark (Armor)
                { 16, 26, 1, 3 }, { 17, 26, 9, 1 }, { 18, 26, 25, 3 },
                // Common runes - Seed (Weapon)
                { 19, 37, 1, 3 }, { 20, 37, 10, 1 }, { 21, 37, 26, 3 },
                // Common runes - Seed (Armor)
                { 22, 38, 1, 3 }, { 23, 38, 10, 1 }, { 24, 38, 29, 3 },
                // Common runes - Bright (Weapon)
                { 25, 49, 1, 3 }, { 26, 49, 11, 1 }, { 27, 49, 27, 3 },
                // Common runes - Bright (Armor)
                { 28, 50, 1, 3 }, { 29, 50, 11, 1 }, { 30, 50, 13, 3 },
                // Common runes - Shadow (Weapon)
                { 31, 61, 1, 3 }, { 32, 61, 12, 1 }, { 33, 61, 28, 3 },
                // Common runes - Shadow (Armor)
                { 34, 62, 1, 3 }, { 35, 62, 12, 1 }, { 36, 62, 16, 3 },

                // Uncommon runes - Fire (Weapon)
                { 37, 3, 2, 5 }, { 38, 3, 7, 3 }, { 39, 3, 14, 5 }, { 40, 3, 30, 3 },
                // Uncommon runes - Fire (Armor)
                { 41, 4, 2, 5 }, { 42, 4, 7, 3 }, { 43, 4, 13, 5 }, { 44, 4, 31, 3 },
                // Uncommon runes - Ice (Weapon)
                { 45, 15, 2, 5 }, { 46, 15, 8, 3 }, { 47, 15, 16, 5 }, { 48, 15, 33, 3 },
                // Uncommon runes - Ice (Armor)
                { 49, 16, 2, 5 }, { 50, 16, 8, 3 }, { 51, 16, 15, 5 }, { 52, 16, 34, 3 },
                // Uncommon runes - Jolt (Weapon)
                { 53, 27, 2, 5 }, { 54, 27, 9, 3 }, { 55, 27, 21, 5 }, { 56, 27, 35, 3 },
                // Uncommon runes - Jolt (Armor)
                { 57, 28, 2, 5 }, { 58, 28, 9, 3 }, { 59, 28, 22, 5 }, { 60, 28, 36, 3 },
                // Uncommon runes - Nature (Weapon)
                { 61, 39, 2, 5 }, { 62, 39, 10, 3 }, { 63, 39, 18, 5 }, { 64, 39, 37, 3 },
                // Uncommon runes - Nature (Armor)
                { 65, 40, 2, 5 }, { 66, 40, 10, 3 }, { 67, 40, 19, 5 }, { 68, 40, 39, 3 },
                // Uncommon runes - Energy (Weapon)
                { 69, 51, 2, 5 }, { 70, 51, 11, 3 }, { 71, 51, 17, 5 }, { 72, 51, 32, 3 },
                // Uncommon runes - Energy (Armor)
                { 73, 52, 2, 5 }, { 74, 52, 11, 3 }, { 75, 52, 20, 5 }, { 76, 52, 40, 3 },
                // Uncommon runes - Gloom (Weapon)
                { 77, 63, 2, 5 }, { 78, 63, 12, 3 }, { 79, 63, 23, 5 }, { 80, 63, 38, 3 },
                // Uncommon runes - Gloom (Armor)
                { 81, 64, 2, 5 }, { 82, 64, 12, 3 }, { 83, 64, 24, 5 }, { 84, 64, 41, 3 },

                // Rare runes - Blaze (Weapon)
                { 85, 5, 3, 7 }, { 86, 5, 7, 5 }, { 87, 5, 30, 5 }, { 88, 5, 51, 3 }, { 89, 5, 47, 1 },
                // Rare runes - Blaze (Armor)
                { 90, 6, 3, 7 }, { 91, 6, 7, 5 }, { 92, 6, 31, 5 }, { 93, 6, 55, 3 }, { 94, 6, 48, 1 },
                // Rare runes - Hail (Weapon)
                { 95, 17, 3, 7 }, { 96, 17, 8, 5 }, { 97, 17, 33, 5 }, { 98, 17, 56, 3 }, { 99, 17, 48, 1 },
                // Rare runes - Hail (Armor)
                { 100, 18, 3, 7 }, { 101, 18, 8, 5 }, { 102, 18, 34, 5 }, { 103, 18, 57, 3 }, { 104, 18, 49, 1 },
                // Rare runes - Lightning (Weapon)
                { 105, 29, 3, 7 }, { 106, 29, 9, 5 }, { 107, 29, 35, 5 }, { 108, 29, 54, 3 }, { 109, 29, 49, 1 },
                // Rare runes - Lightning (Armor)
                { 110, 30, 3, 7 }, { 111, 30, 9, 5 }, { 112, 30, 36, 5 }, { 113, 30, 53, 3 }, { 114, 30, 50, 1 },
                // Rare runes - Thorn (Weapon)
                { 115, 41, 3, 7 }, { 116, 41, 10, 5 }, { 117, 41, 37, 5 }, { 118, 41, 58, 3 }, { 119, 41, 57, 1 },
                // Rare runes - Thorn (Armor)
                { 120, 42, 3, 7 }, { 121, 42, 10, 5 }, { 122, 42, 39, 5 }, { 123, 42, 52, 3 }, { 124, 42, 46, 1 },
                // Rare runes - Radiant (Weapon)
                { 125, 53, 3, 7 }, { 126, 53, 11, 5 }, { 127, 53, 40, 5 }, { 128, 53, 59, 3 }, { 129, 53, 50, 1 },
                // Rare runes - Radiant (Armor)
                { 130, 54, 3, 7 }, { 131, 54, 11, 5 }, { 132, 54, 42, 5 }, { 133, 54, 53, 3 }, { 134, 54, 55, 1 },
                // Rare runes - Abyss (Weapon)
                { 135, 65, 3, 7 }, { 136, 65, 12, 5 }, { 137, 65, 41, 5 }, { 138, 65, 60, 3 }, { 139, 65, 52, 1 },
                // Rare runes - Abyss (Armor)
                { 140, 66, 3, 7 }, { 141, 66, 12, 5 }, { 142, 66, 43, 5 }, { 143, 66, 59, 3 }, { 144, 66, 54, 1 },

                // Epic runes - Lava (Weapon)
                { 145, 7, 4, 9 }, { 146, 7, 7, 7 }, { 147, 7, 47, 3 }, { 148, 7, 51, 7 }, { 149, 7, 61, 2 }, { 150, 7, 64, 3 },
                // Epic runes - Lava (Armor)
                { 151, 8, 4, 9 }, { 152, 8, 7, 7 }, { 153, 8, 48, 3 }, { 154, 8, 53, 7 }, { 155, 8, 62, 2 }, { 156, 8, 67, 3 },
                // Epic runes - Glacier (Weapon)
                { 157, 19, 4, 9 }, { 158, 19, 8, 7 }, { 159, 19, 49, 3 }, { 160, 19, 55, 7 }, { 161, 19, 63, 2 }, { 162, 19, 66, 3 },
                // Epic runes - Glacier (Armor)
                { 163, 20, 4, 9 }, { 164, 20, 8, 7 }, { 165, 20, 50, 3 }, { 166, 20, 56, 7 }, { 167, 20, 65, 2 }, { 168, 20, 72, 3 },
                // Epic runes - Storm (Weapon)
                { 169, 31, 4, 9 }, { 170, 31, 9, 7 }, { 171, 31, 52, 3 }, { 172, 31, 54, 7 }, { 173, 31, 68, 2 }, { 174, 31, 70, 3 },
                // Epic runes - Storm (Armor)
                { 175, 32, 4, 9 }, { 176, 32, 9, 7 }, { 177, 32, 57, 3 }, { 178, 32, 58, 7 }, { 179, 32, 69, 2 }, { 180, 32, 73, 3 },
                // Epic runes - Beast (Weapon)
                { 181, 43, 4, 9 }, { 182, 43, 10, 7 }, { 183, 43, 59, 3 }, { 184, 43, 60, 7 }, { 185, 43, 71, 2 }, { 186, 43, 74, 3 },
                // Epic runes - Beast (Armor)
                { 187, 44, 4, 9 }, { 188, 44, 10, 7 }, { 189, 44, 47, 3 }, { 190, 44, 48, 7 }, { 191, 44, 62, 2 }, { 192, 44, 75, 3 },
                // Epic runes - Solar (Weapon)
                { 193, 55, 4, 9 }, { 194, 55, 11, 7 }, { 195, 55, 49, 3 }, { 196, 55, 50, 7 }, { 197, 55, 63, 2 }, { 198, 55, 61, 3 },
                // Epic runes - Solar (Armor)
                { 199, 56, 4, 9 }, { 200, 56, 11, 7 }, { 201, 56, 51, 3 }, { 202, 56, 53, 7 }, { 203, 56, 67, 2 }, { 204, 56, 75, 3 },
                // Epic runes - Void (Weapon)
                { 205, 67, 4, 9 }, { 206, 67, 12, 7 }, { 207, 67, 58, 3 }, { 208, 67, 52, 7 }, { 209, 67, 66, 2 }, { 210, 67, 69, 3 },
                // Epic runes - Void (Armor)
                { 211, 68, 4, 9 }, { 212, 68, 12, 7 }, { 213, 68, 54, 3 }, { 214, 68, 55, 7 }, { 215, 68, 70, 2 }, { 216, 68, 73, 3 },

                // Legendary runes - Inferno (Weapon & Armor)
                { 217, 9, 5, 11 }, { 218, 9, 7, 9 }, { 219, 9, 76, 1 }, { 220, 9, 83, 2 }, { 221, 9, 89, 1 },
                { 222, 10, 5, 11 }, { 223, 10, 7, 9 }, { 224, 10, 76, 2 }, { 225, 10, 83, 1 }, { 226, 10, 89, 1 },
                // Legendary runes - Blizzard (Weapon & Armor)
                { 227, 21, 5, 11 }, { 228, 21, 8, 9 }, { 229, 21, 77, 1 }, { 230, 21, 86, 2 }, { 231, 21, 90, 1 },
                { 232, 22, 5, 11 }, { 233, 22, 8, 9 }, { 234, 22, 77, 1 }, { 235, 22, 86, 2 }, { 236, 22, 90, 1 },
                // Legendary runes - Tempest (Weapon & Armor)
                { 237, 33, 5, 11 }, { 238, 33, 9, 9 }, { 239, 33, 80, 1 }, { 240, 33, 81, 1 }, { 241, 33, 96, 2 },
                { 242, 34, 5, 11 }, { 243, 34, 9, 9 }, { 244, 34, 80, 1 }, { 245, 34, 81, 2 }, { 246, 34, 96, 1 },
                // Legendary runes - Quake (Weapon & Armor)
                { 247, 45, 5, 11 }, { 248, 45, 10, 9 }, { 249, 45, 84, 1 }, { 250, 45, 88, 1 }, { 251, 45, 98, 2 },
                { 252, 46, 5, 11 }, { 253, 46, 10, 9 }, { 254, 46, 84, 2 }, { 255, 46, 88, 1 }, { 256, 46, 98, 1 },
                // Legendary runes - Divine (Weapon & Armor)
                { 257, 57, 5, 11 }, { 258, 57, 11, 9 }, { 259, 57, 82, 1 }, { 260, 57, 90, 2 }, { 261, 57, 75, 1 },
                { 262, 58, 5, 11 }, { 263, 58, 11, 9 }, { 264, 58, 82, 1 }, { 265, 58, 90, 1 }, { 266, 58, 75, 2 },
                // Legendary runes - Nether (Weapon & Armor)
                { 267, 69, 5, 11 }, { 268, 69, 12, 9 }, { 269, 69, 78, 2 }, { 270, 69, 85, 1 }, { 271, 69, 86, 1 },
                { 272, 70, 5, 11 }, { 273, 70, 12, 9 }, { 274, 70, 78, 1 }, { 275, 70, 85, 2 }, { 276, 70, 86, 1 },

                // Mythic runes - Phoenix (Weapon & Armor)
                { 277, 11, 6, 15 }, { 278, 11, 7, 15 }, { 279, 11, 83, 5 }, { 280, 11, 89, 5 }, { 281, 11, 91, 1 },
                { 282, 12, 6, 15 }, { 283, 12, 7, 15 }, { 284, 12, 83, 5 }, { 285, 12, 89, 5 }, { 286, 12, 92, 1 },
                // Mythic runes - Ymir (Weapon & Armor)
                { 287, 23, 6, 15 }, { 288, 23, 8, 15 }, { 289, 23, 86, 5 }, { 290, 23, 90, 5 }, { 291, 23, 93, 1 },
                { 292, 24, 6, 15 }, { 293, 24, 8, 15 }, { 294, 24, 86, 5 }, { 295, 24, 90, 5 }, { 296, 24, 94, 1 },
                // Mythic runes - Raijin (Weapon & Armor)
                { 297, 35, 6, 15 }, { 298, 35, 9, 15 }, { 299, 35, 81, 5 }, { 300, 35, 96, 5 }, { 301, 35, 95, 1 },
                { 302, 36, 6, 15 }, { 303, 36, 9, 15 }, { 304, 36, 81, 5 }, { 305, 36, 96, 5 }, { 306, 36, 95, 1 },
                // Mythic runes - Gaia (Weapon & Armor)
                { 307, 47, 6, 15 }, { 308, 47, 10, 15 }, { 309, 47, 84, 5 }, { 310, 47, 98, 5 }, { 311, 47, 97, 1 },
                { 312, 48, 6, 15 }, { 313, 48, 10, 15 }, { 314, 48, 84, 5 }, { 315, 48, 98, 5 }, { 316, 48, 97, 1 },
                // Mythic runes - Seraph (Weapon & Armor)
                { 317, 59, 6, 15 }, { 318, 59, 11, 15 }, { 319, 59, 82, 5 }, { 320, 59, 90, 5 }, { 321, 59, 99, 1 },
                { 322, 60, 6, 15 }, { 323, 60, 11, 15 }, { 324, 60, 82, 5 }, { 325, 60, 90, 5 }, { 326, 60, 100, 1 },
                // Mythic runes - Nyx (Weapon & Armor)
                { 327, 71, 6, 15 }, { 328, 71, 12, 15 }, { 329, 71, 85, 5 }, { 330, 71, 86, 5 }, { 331, 71, 101, 1 },
                { 332, 72, 6, 15 }, { 333, 72, 12, 15 }, { 334, 72, 85, 5 }, { 335, 72, 86, 5 }, { 336, 72, 102, 1 }
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT RecipeIngredients OFF");

        // Seed MonsterDrops table
        SeedMonsterDrops(migrationBuilder);
    }

    private static void SeedMonsterDrops(MigrationBuilder migrationBuilder)
    {
        // Essence drops: all 6 elements, all 5 threat levels
        var essenceRates = new[]
        {
            // ThreatLevel 0: Fading 30%, Stable 15%, Potent 5%
            new { Threat = 0, Drops = new (int Id, decimal Rate)[] { (1, 30.0M), (2, 15.0M), (3, 5.0M) } },
            // ThreatLevel 1: Fading 40%, Stable 30%, Potent 15%, Refined 5%
            new { Threat = 1, Drops = new (int Id, decimal Rate)[] { (1, 40.0M), (2, 30.0M), (3, 15.0M), (4, 5.0M) } },
            // ThreatLevel 2: Fading 50%, Stable 40%, Potent 30%, Refined 15%, Pristine 5%
            new { Threat = 2, Drops = new (int Id, decimal Rate)[] { (1, 50.0M), (2, 40.0M), (3, 30.0M), (4, 15.0M), (5, 5.0M) } },
            // ThreatLevel 3: Fading 60%, Stable 50%, Potent 40%, Refined 30%, Pristine 15%, Eternal 5%
            new { Threat = 3, Drops = new (int Id, decimal Rate)[] { (1, 60.0M), (2, 50.0M), (3, 40.0M), (4, 30.0M), (5, 15.0M), (6, 5.0M) } },
            // ThreatLevel 4: Fading 75%, Stable 60%, Potent 50%, Refined 40%, Pristine 30%, Eternal 15%
            new { Threat = 4, Drops = new (int Id, decimal Rate)[] { (1, 75.0M), (2, 60.0M), (3, 50.0M), (4, 40.0M), (5, 30.0M), (6, 15.0M) } },
        };

        foreach (var level in essenceRates)
        {
            foreach (var element in Enumerable.Range(0, 6))
            {
                foreach (var (id, rate) in level.Drops)
                {
                    migrationBuilder.InsertData(
                        table: "MonsterDrops",
                        columns: new[] { "Element", "ThreatLevel", "IngredientId", "DropRate" },
                        values: new object[] { element, level.Threat, id, rate });
                }
            }
        }

        // Core drops: element-specific core at each threat level
        var coreRates = new[] { 25.0M, 35.0M, 40.0M, 50.0M, 60.0M };
        for (int threat = 0; threat < 5; threat++)
        {
            for (int element = 0; element < 6; element++)
            {
                migrationBuilder.InsertData(
                    table: "MonsterDrops",
                    columns: new[] { "Element", "ThreatLevel", "IngredientId", "DropRate" },
                    values: new object[] { element, threat, 7 + element, coreRates[threat] });
            }
        }

        // Monster part drops: each part drops from 3 elements per threat level
        var partTiers = new[]
        {
            // (startId, endId)
            (13, 29),  // Common
            (30, 46),  // Uncommon
            (47, 60),  // Rare
            (61, 75),  // Epic
            (76, 90),  // Legendary
            (91, 102), // Mythic
        };

        var partRatesByThreat = new[]
        {
            // ThreatLevel 0: Common 40%, Uncommon 20%, Rare 5%
            new { Threat = 0, Tiers = new (int TierIndex, decimal Rate)[] { (0, 40.0M), (1, 20.0M), (2, 5.0M) } },
            // ThreatLevel 1: Common 50%, Uncommon 40%, Rare 20%, Epic 5%
            new { Threat = 1, Tiers = new (int TierIndex, decimal Rate)[] { (0, 50.0M), (1, 40.0M), (2, 20.0M), (3, 5.0M) } },
            // ThreatLevel 2: Common 60%, Uncommon 50%, Rare 40%, Epic 20%, Legendary 5%
            new { Threat = 2, Tiers = new (int TierIndex, decimal Rate)[] { (0, 60.0M), (1, 50.0M), (2, 40.0M), (3, 20.0M), (4, 5.0M) } },
            // ThreatLevel 3: Common 75%, Uncommon 60%, Rare 50%, Epic 40%, Legendary 20%, Mythic 5%
            new { Threat = 3, Tiers = new (int TierIndex, decimal Rate)[] { (0, 75.0M), (1, 60.0M), (2, 50.0M), (3, 40.0M), (4, 20.0M), (5, 5.0M) } },
            // ThreatLevel 4: Common 90%, Uncommon 75%, Rare 60%, Epic 50%, Legendary 40%, Mythic 20%
            new { Threat = 4, Tiers = new (int TierIndex, decimal Rate)[] { (0, 90.0M), (1, 75.0M), (2, 60.0M), (3, 50.0M), (4, 40.0M), (5, 20.0M) } },
        };

        foreach (var level in partRatesByThreat)
        {
            foreach (var (tierIndex, rate) in level.Tiers)
            {
                var (startId, endId) = partTiers[tierIndex];
                // Each part drops from 3 elements: (tierIndex*3 + threat) % 6, +1, +2
                int baseElement = (tierIndex * 3 + level.Threat) % 6;
                var elements = new[] { baseElement, (baseElement + 1) % 6, (baseElement + 2) % 6 };

                for (int id = startId; id <= endId; id++)
                {
                    foreach (var element in elements)
                    {
                        migrationBuilder.InsertData(
                            table: "MonsterDrops",
                            columns: new[] { "Element", "ThreatLevel", "IngredientId", "DropRate" },
                            values: new object[] { element, level.Threat, id, rate });
                    }
                }
            }
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
{
    // Delete RecipeIngredients first because they reference Recipes and Ingredients.
    for (int id = 1; id <= 336; id++)
    {
        migrationBuilder.DeleteData(
            table: "RecipeIngredients",
            keyColumn: "Id",
            keyValue: id);
    }

    // Recipes reference Runes, so delete them before Runes.
    for (int id = 1; id <= 72; id++)
    {
        migrationBuilder.DeleteData(
            table: "Recipes",
            keyColumn: "Id",
            keyValue: id);
    }

    // Delete only the MonsterDrops rows inserted by this migration.
    DeleteSeededMonsterDrops(migrationBuilder);

    // Delete Runes after their Recipes have been deleted.
    for (int id = 1; id <= 72; id++)
    {
        migrationBuilder.DeleteData(
            table: "Runes",
            keyColumn: "Id",
            keyValue: id);
    }

    // Ingredients are deleted last because other seeded tables reference them.
    for (int id = 1; id <= 102; id++)
    {
        migrationBuilder.DeleteData(
            table: "Ingredients",
            keyColumn: "Id",
            keyValue: id);
    }
}

private static void DeleteSeededMonsterDrops(MigrationBuilder migrationBuilder)
{
    // Essence drops
    var essenceDrops = new[]
    {
        new { Threat = 0, Drops = new[] { 1, 2, 3 } },
        new { Threat = 1, Drops = new[] { 1, 2, 3, 4 } },
        new { Threat = 2, Drops = new[] { 1, 2, 3, 4, 5 } },
        new { Threat = 3, Drops = new[] { 1, 2, 3, 4, 5, 6 } },
        new { Threat = 4, Drops = new[] { 1, 2, 3, 4, 5, 6 } }
    };

    foreach (var level in essenceDrops)
    {
        foreach (var element in Enumerable.Range(0, 6))
        {
            foreach (var ingredientId in level.Drops)
            {
                DeleteMonsterDrop(
                    migrationBuilder,
                    element,
                    level.Threat,
                    ingredientId);
            }
        }
    }

    // Element-specific core drops
    for (int threat = 0; threat < 5; threat++)
    {
        for (int element = 0; element < 6; element++)
        {
            DeleteMonsterDrop(
                migrationBuilder,
                element,
                threat,
                7 + element);
        }
    }

    // Monster part drops
    var partTiers = new[]
    {
        (StartId: 13, EndId: 29),
        (StartId: 30, EndId: 46),
        (StartId: 47, EndId: 60),
        (StartId: 61, EndId: 75),
        (StartId: 76, EndId: 90),
        (StartId: 91, EndId: 102)
    };

    var partTiersByThreat = new[]
    {
        new { Threat = 0, Tiers = new[] { 0, 1, 2 } },
        new { Threat = 1, Tiers = new[] { 0, 1, 2, 3 } },
        new { Threat = 2, Tiers = new[] { 0, 1, 2, 3, 4 } },
        new { Threat = 3, Tiers = new[] { 0, 1, 2, 3, 4, 5 } },
        new { Threat = 4, Tiers = new[] { 0, 1, 2, 3, 4, 5 } }
    };

    foreach (var level in partTiersByThreat)
    {
        foreach (var tierIndex in level.Tiers)
        {
            var (startId, endId) = partTiers[tierIndex];

            int baseElement = (tierIndex * 3 + level.Threat) % 6;

            var elements = new[]
            {
                baseElement,
                (baseElement + 1) % 6,
                (baseElement + 2) % 6
            };

            for (int ingredientId = startId; ingredientId <= endId; ingredientId++)
            {
                foreach (var element in elements)
                {
                    DeleteMonsterDrop(
                        migrationBuilder,
                        element,
                        level.Threat,
                        ingredientId);
                }
            }
        }
    }
}

    private static void DeleteMonsterDrop(
        MigrationBuilder migrationBuilder,
        int element,
        int threatLevel,
        int ingredientId)
    {
        migrationBuilder.DeleteData(
            table: "MonsterDrops",
            keyColumns: new[]
            {
                "Element",
                "ThreatLevel",
                "IngredientId"
            },
            keyValues: new object[]
            {
                element,
                threatLevel,
                ingredientId
            });
    }
    
}