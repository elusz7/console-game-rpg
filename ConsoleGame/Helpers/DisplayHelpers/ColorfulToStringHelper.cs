using System.Text;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Runes.Recipes;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using Rune = ConsoleGameEntities.Models.Runes.Rune;

namespace ConsoleGame.Helpers.DisplayHelpers;

public static class ColorfulToStringHelper
{
    public static ConsoleColor GetItemColor(Item item) => item switch
    {
        Weapon => ConsoleColor.Red,
        Armor => ConsoleColor.Green,
        Consumable => ConsoleColor.Blue,
        _ => ConsoleColor.Yellow
    };
    public static ConsoleColor GetSkillColor(Skill skill) => skill.SkillCategory switch
    {
        SkillCategory.Basic => ConsoleColor.Red,
        SkillCategory.Support => ConsoleColor.Blue,
        SkillCategory.Ultimate => ConsoleColor.Green,
        _ => ConsoleColor.Yellow
    };
    public static ConsoleColor GetArchetypeColor(Archetype archetype) => archetype.ArchetypeType switch
    {
        ArchetypeType.Martial => ConsoleColor.Red,
        ArchetypeType.Magical => ConsoleColor.Blue,
        _ => ConsoleColor.White
    };
    public static ConsoleColor GetMonsterColor(Monster monster)
    {
        return monster.ThreatLevel switch
        {
            ThreatLevel.Low => ConsoleColor.Gray,
            ThreatLevel.Medium => ConsoleColor.Green,
            ThreatLevel.High => ConsoleColor.Blue,
            ThreatLevel.Elite => ConsoleColor.Yellow,
            _ => ConsoleColor.Red
        };
    }
    public static ConsoleColor GetRarityColor(RarityLevel rarity)
    {
        return rarity switch
        {
            RarityLevel.Common => ConsoleColor.Gray,
            RarityLevel.Uncommon => ConsoleColor.Green,
            RarityLevel.Rare => ConsoleColor.Blue,
            RarityLevel.Epic => ConsoleColor.Magenta,
            RarityLevel.Legendary => ConsoleColor.Yellow,
            RarityLevel.Mythic => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
    }

    public static string ItemStatsString(Item item, decimal? value = null)
    {
        if (value != null)
            return $"{item.Name}{GetArmorType(item)} - {value:0.00} [DUR: {item.Durability}{GetItemStats(item)}]{RuneInfo(item)}";

        return $"{item.Name}{GetArmorType(item)} [DUR: {item.Durability}{GetItemStats(item)}]{RuneInfo(item)}";
    }
    public static string SkillStatsString(Player player, Skill skill)
    {
        return $"{skill.Name} [power: {skill.Power}, cost: {skill.Cost}, cooldown: {skill.Cooldown}, {GetTargets(skill)}{GetSupportStat(skill)}]{WaitingStats(player, skill)}";
    }
    public static string ArchetypeToString(Archetype archetype)
    {
        return $"{archetype.Name} [Health: {archetype.HealthBase}, Attack: {archetype.AttackBonus}, Magic: {archetype.MagicBonus}, Defense: {archetype.DefenseBonus}, Resistance: {archetype.ResistanceBonus}, Speed: {archetype.Speed}]";
    }
    public static string SkillToString(Skill skill)
    {
        return $"{skill.Name} - {skill.Description}\n\t[level: {skill.RequiredLevel}, power: {skill.Power}, cost: {skill.Cost}, cooldown: {skill.Cooldown}, {GetTargets(skill)}{GetSupportStat(skill)}]";
    }
    public static string MonsterToString(Monster monster)
    {
        return $"{monster.Name} - {monster.Description}\n\t[threat level: {monster.ThreatLevel}, damage type: {monster.DamageType}, level: {monster.Level}, health: {monster.MaxHealth}]";
    }
    public static string RoomToString(Room room)
    {
        var sb = new StringBuilder();
        sb.Append($"{room.Name} - {room.Description}");

        var directions = new List<string>();

        if (room.North != null)
            directions.Add($"North: {room.North.Name}");
        if (room.South != null)
            directions.Add($"South: {room.South.Name}");
        if (room.East != null)
            directions.Add($"East: {room.East.Name}");
        if (room.West != null)
            directions.Add($"West: {room.West.Name}");

        sb.Append($"\n\t{string.Join(", ", directions)}");

        return sb.ToString();
    }
    public static string PlayerToString(Player player)
    {
        var sb = new StringBuilder();
        sb.Append($"{player.Name} [Level {player.Level} {player.Archetype.Name}]");
        sb.Append($"\n\t[Health: {player.MaxHealth}, Gold: {player.Inventory.Gold}, Carrying Weight: {player.Inventory.GetCarryingWeight()}/{player.Inventory.Capacity}]");

        return sb.ToString();
    }
    public static string ItemToString(Item item)
    {
        var sb = new StringBuilder();
        sb.Append($"[{item.Name}] : {item.Description}");
        sb.Append($"\n\tLVL: {item.RequiredLevel}, VAL: {item.Value}, DUR: {item.Durability}, WGHT: {item.Weight}");

        switch (item)
        {
            case Weapon weapon:
                sb.Append($"\n\tATK: {weapon.AttackPower}, DMG: {weapon.DamageType}");
                break;
            case Armor armor:
                sb.Append($"\n\tDEF: {armor.DefensePower}, RES: {armor.Resistance}");
                break;
            case Consumable consumable:
                sb.Append($"\n\tPWR: {consumable.Power}, TYPE: {consumable.ConsumableType}");
                break;
        }

        if (item.Inventory?.Player != null)
        {
            sb.Append($"\n\tHeld By: {item.Inventory.Player.Name}");
        }

        return sb.ToString();
    }
    public static string RecipeToString(Recipe recipe, Dictionary<Ingredient, int> availableIngredients)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{recipe.Name}]");
        //sb.AppendLine(string.Join("\n\t", recipe.Ingredients.Select(i => $"{i.Ingredient.Name} x{i.Quantity}")));
        foreach (var recipeIngredient in recipe.Ingredients)
        {
            sb.Append($"\t{recipeIngredient.Ingredient.Name} x{recipeIngredient.Quantity}");
            if (availableIngredients.TryGetValue(recipeIngredient.Ingredient, out var qty))
            {
                sb.Append($" | available: {qty}");
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }
    public static string RuneToString(Rune rune)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{rune.Name}] - {rune.RuneType.Split("Rune")[0]}");
        sb.AppendLine($"\tRarity: {rune.Rarity} | Element: {rune.Element} | Tier: {rune.Tier} | Power: {rune.Power}");
        return sb.ToString();
    }



    public static string GetItemStats(Item item)
    {
        return item switch
        {
            Weapon weapon => $" | ATK: {weapon.AttackPower}",
            Armor armor => $" | DEF: {armor.DefensePower} | RES: {armor.Resistance}",
            Consumable consumable => $" | POW: {consumable.Power} | AFF: {consumable.ConsumableType}",
            _ => ""
        };
    }
    private static string RuneInfo(Item item)
    {
        return item switch
        {
            Armor armor => armor.Rune == null ? "" : $" {armor.Rune.Name}",
            Weapon weapon => weapon.Rune == null ? "" : $" {weapon.Rune.Name}",
            _ => ""
        };
    }
    private static string GetArmorType(Item item)
    {
        return item switch
        {
            Armor armor => $" ({armor.ArmorType})",
            _ => ""
        };
    }
    private static string GetTargets(Skill skill)
    {
        return skill.TargetType switch
        {
            TargetType.Self => "targets yourself",
            TargetType.SingleEnemy => "targets a single enemy",
            TargetType.AllEnemies => "targets all enemies in room",
            _ => throw new InvalidTargetException("Invalid target option on skill")
        };
    }
    private static string GetSupportStat(Skill skill)
    {
        if (skill is SupportSkill supportSkill)
        {
            return $", Affects {supportSkill.StatAffected}";
        }

        return "";
    }
    private static string SkillResourceNeeded(Player player, Skill skill)
    {
        int deficit = skill.Cost - player.Archetype.CurrentResource;

        if (deficit > 0)
        {
            return $" {player.Archetype.ResourceName} NEEDED ({deficit})";
        }

        return "";
    }
    private static string SkillTurnsNeeded(Skill skill)
    {
        int turns = Math.Max(0, skill.Cooldown - skill.ElapsedTime);
        return turns > 0 ? $"({turns} TURNS UNTIL READY)" : "";
    }
    public static string WaitingStats(Player player, Skill skill)
    {
        var parts = new List<string>();

        var resourceNeeded = SkillResourceNeeded(player, skill);
        if (!string.IsNullOrWhiteSpace(resourceNeeded))
            parts.Add(resourceNeeded);

        var turnsNeeded = SkillTurnsNeeded(skill);
        if (!string.IsNullOrWhiteSpace(turnsNeeded))
            parts.Add(turnsNeeded);

        return parts.Count == 0 ? "" : string.Join(" ", parts);
    }
}
