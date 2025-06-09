using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGameEntities.Models.Items;

public class Item : IItem
{
    private static readonly Random _rng = Random.Shared;
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public string Description { get; set; }
    public int Durability { get; set; }
    public decimal Weight { get; set; }
    public int RequiredLevel { get; set; }
    public string ItemType { get; set; }

    public int? InventoryId { get; set; }
    public virtual Inventory? Inventory { get; set; }
    public int? MonsterId { get; set; }
    public virtual Monster? Monster { get; set; }

    public virtual void Use()
    {
        Durability--;
        if (Durability < 1)
            throw new ItemDurabilityException("This item is broken and can't be used");
    }
    public virtual void RecoverDurability(int power)
    {
        if (Durability + power > 100)
            Durability = 100;
        else
            Durability += power;
    }

    public virtual void CalculateValue()
    {
        var baseValue = (decimal)Math.Pow(RequiredLevel, 1.15) * 1.5M;
        var durabilityValue = Durability * 1.5M;

        var variance = (decimal)(_rng.NextDouble() * 0.2) + 0.9M;

        var calculation = (baseValue + durabilityValue) * variance;

        Value = Math.Round(calculation, 2);
    }
    public virtual void CalculateStatsByLevel()
    {
        Durability = _rng.Next(3, 9);
    }
    public virtual void CalculateLevelByStats()
    {
        var durabilityValue = Durability * 1.5M;
        var estimatedBaseValue = Value - durabilityValue;

        if (estimatedBaseValue <= 0)
        {
            RequiredLevel = 1;
            return;
        }

        var rawLevelEstimate = (decimal)Math.Pow((double)(estimatedBaseValue / 1.5M), 1.0 / 1.15);

        RequiredLevel = Math.Max(1, (int)Math.Round(rawLevelEstimate));
    }

    public virtual decimal GetBuyPrice() => Value; // Merchant sells at full price
    public virtual decimal GetSellPrice() => Math.Round(Value * 0.75M, 2); // Player sells for 75%
}
