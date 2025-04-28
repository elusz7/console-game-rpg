using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Abilities;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameEntities.Interfaces;

public interface IPlayer
{
    int Id { get; set; }
    string Name { get; set; }
    Inventory Inventory { get; set; }
    void Attack(ITargetable target);
}
