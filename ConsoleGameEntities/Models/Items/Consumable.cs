using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Models.Items;

public class Consumable : Item
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
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
                player.RecoverResource(Power);
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

    public override void CalculateValue()
    {
        var baseValue = (decimal)Math.Pow(RequiredLevel, 1.15) * 1.5M;
        var durabilityValue = Durability * 1.5M;
        var powerValue = Power * 1.8M;

        var variance = (decimal)(_rng.NextDouble() * 0.2) + 0.9M;

        var calculation = (baseValue + durabilityValue + powerValue) * variance;

        Value = Math.Round(calculation, 2);
    }
    public override void CalculateStatsByLevel()
    {
        Durability = 3;
        Power = ConsumableType switch
        {
            ConsumableType.Health => _rng.Next(RequiredLevel * 3, RequiredLevel * 5),
            ConsumableType.Durability => _rng.Next(RequiredLevel, RequiredLevel * 2),
            ConsumableType.Resource => _rng.Next(RequiredLevel * 2, RequiredLevel * 4),
            _ => RequiredLevel * 2,// fallback value
        };
    }
    public override void CalculateLevelByStats()
    {
        var estimatedLevel = ConsumableType switch
        {
            ConsumableType.Health => (int)Math.Round(Power / 4M),
            ConsumableType.Durability => (int)Math.Round(Power / 1.5M),
            ConsumableType.Resource => (int)Math.Round(Power / 3M),
            _ => 1
        };

        RequiredLevel = Math.Max(1, estimatedLevel); // Safety cap
    }
}
