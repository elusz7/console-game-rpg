using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Characters;

namespace ConsoleGameEntities.Interfaces;

public interface IAbility
{
    int Id { get; set; }
    string Name { get; set; }
    IEnumerable<Player> Players { get; set; }

    void Activate(IPlayer user, ITargetable target);
}