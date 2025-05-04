using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameEntities.Interfaces;

public interface IPlayer : ITargetable
{
    int Id { get; set; }
    string Name { get; set; }
    int CurrentHealth { get; set; }
    int MaxHealth { get; set; }
    int Experience { get; set; }
    int Level { get; set; }
    Inventory Inventory { get; set; }
    Archetype Archetype { get; set; }
    void Attack(ITargetable target);
    void LevelUp();
}
