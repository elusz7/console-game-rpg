using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Abilities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Interfaces;

public interface IPlayer
{
    int Id { get; set; }
    string Name { get; set; }

    ICollection<Ability> Abilities { get; set; }

    Inventory Inventory { get; set; }

    void Attack(ITargetable target);
    void UseAbility(IAbility ability, ITargetable target);


}
