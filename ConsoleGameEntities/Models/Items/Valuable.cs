using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Items;

public class Valuable : Item
{
    public Valuable() : this(new RandomProvider())
    {
    }

    public Valuable(IRandomProvider random) : base(random)
    {
    }
    
    public override void Use() =>
        throw new InvalidOperationException("A valuable cannot be used."); //A valuable cannot be used

    public override void RecoverDurability(int power) =>
        throw new InvalidOperationException("A valuable's durability cannot be recovered");

    public override decimal GetSellPrice() => Value; // values sell for full price

    public override void CalculateStatsByLevel()
    {
        Durability = RequiredLevel % 2 + 1;
    }

    public override void CalculateLevelByStats()
    {
        decimal durabilityValue = Durability * 1.5M;
        decimal estimatedBaseValue = Value - durabilityValue;

        if (estimatedBaseValue <= 0)
        {
            RequiredLevel = 1;
            return;
        }

        decimal rawLevelEstimate = (decimal)Math.Pow((double)(estimatedBaseValue / 1.5M), 1.0 / 1.15);

        RequiredLevel = Math.Max(1, (int)Math.Round(rawLevelEstimate));
    }
}