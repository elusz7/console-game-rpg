using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameEntities.Interfaces;

public interface IAbility
{
    int Id { get; set; }
    string Name { get; set; }
    void Activate(IPlayer user, ITargetable target);
}