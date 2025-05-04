using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Models.Items;

public class Consumable : Item
{
    public int Power { get; set; }
    public ConsumableType ConsumableType { get; set; }
    public override int Use()
    {
        throw new InvalidTargetException("You need something to use this consumable on.");
    }
    public void UseOn(object target)
    {            
        switch (target)
        {
            case IPlayer player when ConsumableType == ConsumableType.Health:
                player.Heal(Power);
                break;
            case IItem item when ConsumableType == ConsumableType.Durability:
                item.Durability += Power;
                break;
            case IPlayer player when ConsumableType == ConsumableType.Resource:
                player.Archetype.RecoverResource(Power);
                break;
            default:
                throw new InvalidTargetException($"{Name} cannot be used on {target.GetType().Name}.");
        }

        Inventory?.RemoveItem(this);
    }
}
