using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Runes;

namespace ConsoleGameEntities.Interfaces;

public interface IPlayer : ITargetable
{
    int Id { get; set; }
    string Name { get; set; }
    int CurrentHealth { get; set; }
    int MaxHealth { get; set; }
    int Level { get; set; }
    Inventory Inventory { get; set; }
    Archetype Archetype { get; set; }
    void Attack(ITargetable target);
    void Equip(Item item);
    void Unequip(Item item);
    void ApplyRune(WeaponRune rune);
    void ApplyRune(ArmorRune rune, Armor armor);
    void LevelUp();
}
