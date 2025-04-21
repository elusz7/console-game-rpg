using ConsoleGameEntities.Models.Attributes;
using ConsoleGameEntities.Models.Characters;

namespace ConsoleGameEntities.Models.Abilities.PlayerAbilities;

public interface IAbility
{
    int Id { get; set; }
    string Name { get; set; }
    IEnumerable<Player> Players { get; set; }

    void Activate(IPlayer user, ITargetable target);
}