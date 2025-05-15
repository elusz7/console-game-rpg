using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGame.Managers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Exceptions;
using System.Numerics;

namespace ConsoleGame.Helpers.DisplayHelpers;

public static class ColorfulToStringHelper
{
    public static void ColorItemString(Item item, OutputManager _output)
    {
        var color = GetItemColor(item);
        _output.Write($"[{item.Name}] ", color);
        _output.Write(": ");
        _output.Write($"{item.Description}", ConsoleColor.Cyan);
        _output.Write($"\n\tLevel: {item.RequiredLevel}, Value: {item.Value}, Durability: {item.Durability}, Weight: {item.Weight}");

        switch (item)
        {
            case Weapon weapon:
                _output.Write($"\n\tAttack Power: {weapon.AttackPower}, Damage Type: {weapon.DamageType}", ConsoleColor.Red);
                break;
            case Armor armor:
                _output.Write($"\n\tDefense: {armor.DefensePower}, Resistance: {armor.Resistance}", ConsoleColor.Green);
                break;
            case Consumable consumable:
                _output.Write($"\n\tPower: {consumable.Power}, Consumable Type: {consumable.ConsumableType}", ConsoleColor.Yellow);
                break;
        }

        if (item.Inventory?.Player != null)
        {
            _output.Write($"\n\tHeld By: {item.Inventory.Player.Name}", ConsoleColor.Cyan);
        }

        _output.WriteLine();
    }

    public static void ColorPlayerOutput(Player player, OutputManager _output)
    {
        _output.Write($"[{player.Name}] ", ConsoleColor.Green);
        _output.Write($"{player.Archetype.Name}, Level: {player.Level}, Health: {player.MaxHealth}, ");
        _output.WriteLine($"Gold: {player.Inventory.Gold}, Carrying Weight: {player.Inventory.GetCarryingWeight()}/{player.Inventory.Capacity}");
    }

    public static void ColorRoomOutput(Room room, OutputManager _output)
    {
        _output.Write($"[{room.Name}] ", ConsoleColor.DarkYellow);
        _output.WriteLine($"{room.Description}");

        var directions = new List<string>();

        if (room.North != null)
            directions.Add($"North: {room.North.Name}");
        if (room.South != null)
            directions.Add($"South: {room.South.Name}");
        if (room.East != null)
            directions.Add($"East: {room.East.Name}");
        if (room.West != null)
            directions.Add($"West: {room.West.Name}");

        if (directions.Count > 0)
        {
            _output.Write("\tConnections: ", ConsoleColor.DarkGreen);
            _output.WriteLine(string.Join(", ", directions), ConsoleColor.DarkGreen);
        }
    }

    public static void ColorMonsterOutput(Monster monster, OutputManager _output)
    {
        _output.Write($"[{monster.Name}] ", ConsoleColor.DarkMagenta);
        _output.WriteLine($"{monster.Description}");
        _output.Write($"\t{monster.ThreatLevel} ", ConsoleColor.Red);
        _output.Write($"{monster.DamageType}, ", ConsoleColor.Cyan);
        _output.WriteLine($"Level: {monster.Level}, Health: {monster.MaxHealth}");
    }

    public static void ColorSkillOutput(Skill skill, OutputManager _output)
    {
        var color = GetSkillColor(skill);
        _output.Write($"[{skill.Name}] ", color);
        _output.Write($"{skill.Description}");
        _output.Write($"\n\tLevel: {skill.RequiredLevel}, Power: {skill.Power}, Cost: {skill.Cost}, Cooldown: {skill.Cooldown}, Type: {skill.SkillType}");

        if (skill is SupportSkill support)
        {
            _output.Write($", {(SupportEffectType)support.SupportEffect}s {support.StatAffected}");
        }

        _output.WriteLine();
    }

    public static void ColorArchetypeOutput(Archetype archetype, OutputManager _output)
    {
        var color = GetArchetypeColor(archetype);
        _output.Write($"[{archetype.Name}] ", color);
        _output.WriteLine($"{archetype.Description}");
        _output.Write($"\tHealth: {archetype.HealthBase}, Attack: {archetype.AttackBonus}, Magic: {archetype.MagicBonus}, Defense: {archetype.DefenseBonus}, Resistance: {archetype.ResistanceBonus}, Speed: {archetype.Speed}");
    }

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

    public static string ItemStatsString(Item item, decimal? valueMultiplier = null)
    {
        if (valueMultiplier != null) 
            return $"{item.Name}{GetArmorType(item)} - {item.Value * valueMultiplier:0.00} [DUR: {item.Durability}{GetItemStats(item)}]";

        return $"{item.Name}{GetArmorType(item)} [DUR: {item.Durability}{GetItemStats(item)}]";
    }
    public static string SkillStatsString(Player player, Skill skill)
    {
        return $"{skill.Name} [power: {skill.Power}, cost: {skill.Cost}, cooldown: {skill.Cooldown}, {GetTargets(skill)}{GetSupportStat(skill)}]{WaitingStats(player, skill)}";
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
        var parts = new List<string> ();

        var resourceNeeded = SkillResourceNeeded(player, skill);
        if (!string.IsNullOrWhiteSpace(resourceNeeded))
            parts.Add(resourceNeeded);

        var turnsNeeded = SkillTurnsNeeded(skill);
        if (!string.IsNullOrWhiteSpace(turnsNeeded))
            parts.Add(turnsNeeded);        

        return parts.Count == 0 ? "" : string.Join(" ", parts);
    }
}
