using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Models.Abilities.PlayerAbilities;
using ConsoleGameEntities.Models.Attributes;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Models.Characters;

public interface IPlayer
{
    int Id { get; set; }
    string Name { get; set; }

    ICollection<Ability> Abilities { get; set; }

    Inventory Inventory { get; set; }

    void Attack(ITargetable target);
    void UseAbility(IAbility ability, ITargetable target);


}
