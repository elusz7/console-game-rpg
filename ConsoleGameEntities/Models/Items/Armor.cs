using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Items;

public class Armor : Item, IEquippable, IEnchantable, IReforgable, ICursable
{
    private readonly Random _rng = Random.Shared;

    private bool Cursed { get; set; }
    private bool Equipped { get; set; }

    public int DefensePower { get; set; }
    public int Resistance { get; set; }
    public ArmorType ArmorType { get; set; }

    public int? RuneId { get; set; }
    public virtual ArmorRune? Rune { get; set; }

    public decimal GetPurificationPrice() => Value * 0.5M;
    public decimal GetReforgePrice() => Value * 0.33M;
    public decimal GetEnchantmentPrice() => Value * 1.5M;

    public void Curse() => Cursed = true;
    public void TargetCursed() => Cursed = false;
    public bool IsCursed() => Cursed;
    public void Equip() => Equipped = true;
    public void Unequip() => Equipped = false;
    public bool IsEquipped() => Equipped;

    public void Reforge()
    {
        if (Inventory == null)
            throw new ItemReforgeException("Item is not in an inventory.");

        var price = (int)Math.Floor(Value * 0.33M);

        if (Inventory.Gold < price)
            throw new ItemReforgeException($"You are short by {price - Inventory.Gold} gold.");

        double failureChance = Math.Min(0.01 * RequiredLevel, 0.6);
        if (_rng.NextDouble() < failureChance)
        {
            Inventory.Gold -= price;
            throw new ItemReforgeException("Reforge attempt failed.");
        }

        var totalPower = DefensePower + Resistance;

        var newDefensePower = _rng.Next(totalPower);
        var newResistance = totalPower - newDefensePower;

        if (newDefensePower == DefensePower && newResistance == Resistance)
            throw new ItemReforgeException("Reforge failed. No gold taken. No changes made.");

        DefensePower = newDefensePower;
        Resistance = newResistance;

        //Value += Math.Round(totalPower * RequiredLevel * 0.5M, 2);
        //CalculateValue();

        Inventory.Gold -= price;
    }
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

        var newStats = _rng.Next(RequiredLevel, RequiredLevel * 2 + 1);

        var newDefensePower = _rng.Next(1, newStats);
        var newResistance = newStats - newDefensePower;

        DefensePower += newDefensePower;
        Resistance += newResistance;

        base.CalculateValue();

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
        var armorValue = DefensePower * 0.5M + Resistance * 0.5M;

        var variance = (decimal)(_rng.NextDouble() * 0.2) + 0.9M;

        var calculation = (baseValue + durabilityValue + armorValue) * variance;

        Value = Math.Round(calculation, 2);
    }
    public override void CalculateStatsByLevel()
    {
        Durability = _rng.Next(3, 9);

        var total = _rng.Next(RequiredLevel * 5, RequiredLevel * 8);
        DefensePower = _rng.Next(0, total + 1);
        Resistance = total - DefensePower;
    }
    public override void CalculateLevelByStats()
    {
        var total = DefensePower + Resistance;
        var estimatedLevel = (int)Math.Round(total / 8.5M);

        RequiredLevel = Math.Max(1, estimatedLevel); // Safety cap
    }
}
