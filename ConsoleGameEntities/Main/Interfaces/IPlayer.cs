using ConsoleGameEntities.Main.Interfaces.Attributes;
using ConsoleGameEntities.Main.Models.Entities;
using ConsoleGameEntities.Main.Models.Items;

namespace ConsoleGameEntities.Main.Interfaces;

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
    void LevelUp();
}
