using System.Text;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Items;

public class Weapon : Item, IEquippable, IEnchantable, ICursable
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());

    private bool Cursed { get; set; }
    private bool Equipped { get; set; }
    public int AttackPower { get; set; }
    public DamageType DamageType { get; set; }

    public decimal GetPurificationPrice() => Value * 0.5M;
    public decimal GetEnchantmentPrice() => Value * 1.5M;
    public void Curse() => Cursed = true;
    public void TargetCursed() => Cursed = false;
    public bool IsCursed() => Cursed;
    public void Equip() => Equipped = true;
    public void Unequip() => Equipped = false;
    public bool IsEquipped() => Equipped;

    public void Enchant()
    {
        if (Inventory == null)
            throw new ItemEnchantmentException("Item is not in an inventory.");

        var price = (int)Math.Floor(Value * 1.5M);

        if (Inventory.Gold < price)
            throw new ItemEnchantmentException($"You are short by {price - Inventory.Gold} gold.");

        double failureChance = Math.Min(0.02 * RequiredLevel, 0.6);
        if (_rng.NextDouble() < failureChance)
        {
            Inventory.Gold -= price;
            throw new ItemEnchantmentException("Enchantment attempt failed.");
        }

        RequiredLevel++;

        var newStatMax = _rng.Next(RequiredLevel * 3, RequiredLevel * 6 + 1);
        var newStatMin = (int)Math.Max(1, Math.Floor(newStatMax * 0.33));
        var newAttackPower = _rng.Next(newStatMin, newStatMax);

        AttackPower += newAttackPower;

        CalculateValue();

        Inventory.Gold -= price;
    }  
    public void Purify()
    {
        if (!Cursed)
            throw new ItemPurificationException("Item is not cursed.");

        if (Inventory == null)
            throw new ItemPurificationException("Item is not in an inventory.");

        var price = (int)Math.Floor(Value * 0.5M);

        if (Inventory.Gold < price)
            throw new ItemPurificationException($"You are short by {price - Inventory.Gold} gold.");

        Inventory.Gold -= price;
        Cursed = false;
    }

    public override void CalculateValue()
    {
        var baseValue = (decimal)Math.Pow(RequiredLevel, 1.15) * 1.5M;
        var durabilityValue = Durability * 1.5M;
        var attackValue = AttackPower * 1.2M;

        var variance = (decimal)(_rng.NextDouble() * 0.2) + 0.9M;

        var calculation = (baseValue + durabilityValue + attackValue) * variance;

        Value = Math.Round(calculation, 2);
    }
    public override void CalculateStatsByLevel()
    {
        Durability = _rng.Next(3, 9);

        AttackPower = _rng.Next(RequiredLevel * 6, RequiredLevel * 9);
    }
    public override void CalculateLevelByStats()
    {
        var estimatedLevel = (int)Math.Round(AttackPower / 9.5M);

        RequiredLevel = Math.Max(1, estimatedLevel); // Safety cap
    }
}
