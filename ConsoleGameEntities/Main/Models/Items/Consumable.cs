using ConsoleGameEntities.Main.Exceptions;
using ConsoleGameEntities.Main.Interfaces;
using ConsoleGameEntities.Main.Models.Entities;
using static ConsoleGameEntities.Main.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Main.Models.Items;

public class Consumable : Item
{
    public int Power { get; set; }
    public ConsumableType ConsumableType { get; set; }
    public override void Use()
    {
        throw new InvalidTargetException("You need something to use this consumable on.");
    }
    public void UseOn(Item item)
    {
        if (ConsumableType != ConsumableType.Durability)
            throw new InvalidTargetException($"{Name} cannot be used on {item.GetType().Name}.");

        item.RecoverDurability(Power);
        item.Inventory?.Player?.AddActionItem($"{item.Name} has been repaired by {Power} durability!");

        Durability--;
        if (Durability < 1)
        {
            item.Inventory?.Player?.AddActionItem($"{Name} has been used up.");
            Inventory?.RemoveItem(this);
        }
    }
    public void UseOn(Player player)
    {
        switch (ConsumableType)
        {
            case ConsumableType.Health:
                player.Heal(Power);
                break;
            case ConsumableType.Durability:
                throw new InvalidTargetException("You cannot use this consumable on a player.");
            case ConsumableType.Resource:
                player.Archetype.RecoverResource(Power);
                break;
            default:
                throw new InvalidTargetException($"{Name} cannot be used on {player.GetType().Name}.");
        }

        Durability--;

        if (Durability < 1)
        { 
            player.AddActionItem($"{Name} has been used up.");
            Inventory?.RemoveItem(this);
        }
    }

}
