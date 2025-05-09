using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers;

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

    private static ConsoleColor GetItemColor(Item item) => item switch
    {
        Weapon => ConsoleColor.Red,
        Armor => ConsoleColor.Green,
        Consumable => ConsoleColor.Yellow,
        _ => ConsoleColor.White
    };

    private static ConsoleColor GetSkillColor(Skill skill) => skill.SkillType switch
    {
        "MartialSkill" => ConsoleColor.Red,
        "MagicSkill" => ConsoleColor.Blue,
        "SupportSkill" => ConsoleColor.Green,
        "UltimateSkill" => ConsoleColor.Magenta,
        _ => ConsoleColor.Yellow
    };

    private static ConsoleColor GetArchetypeColor(Archetype archetype) => archetype.ArchetypeType switch
    {
        ArchetypeType.Martial => ConsoleColor.Red,
        ArchetypeType.Magical => ConsoleColor.Blue,
        _ => ConsoleColor.White
    };
}
