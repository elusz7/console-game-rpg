using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ConsoleGameEntities.Main.Interfaces;
using ConsoleGameEntities.Main.Models.Entities;
using ConsoleGameEntities.Main.Models.Monsters;
using ConsoleGameEntities.Main.Exceptions;
using static ConsoleGameEntities.Main.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Main.Models.Items;

public class Item : IItem
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public string Description { get; set; }
    public int Durability { get; set; }
    public decimal Weight { get; set; }
    [NotMapped]
    private bool ItemEquipped { get; set; }
    [NotMapped]
    public bool IsCursed { get; set; }
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
    public virtual void Purify()
    {
        if (!IsCursed)
            throw new ItemPurificationException("Item is not cursed.");

        if (Inventory == null)
            throw new ItemPurificationException("Item is not in an inventory.");

        var price = (int)Math.Floor(Value * 0.5M);

        if (Inventory.Gold < price)
            throw new ItemPurificationException($"You are short by {price - Inventory.Gold} gold.");

        Inventory.Gold -= price;
        IsCursed = false;
    }
    public virtual void Reforge()
    {
        throw new InvalidOperationException("Reforging this item is not allowed.");
    }
    public virtual void Enchant()
    {
        throw new InvalidOperationException("Enchanting this item is not allowed.");
    }
    public virtual void Equip()
    {
        ItemEquipped = true;
    }
    public virtual void Unequip()
    {
        ItemEquipped = false;
    }
    public virtual bool IsEquipped() => ItemEquipped;
    public virtual void RecoverDurability(int power)
    {
        if (Durability + power > 100)
            Durability = 100;
        else
            Durability += power;
    }
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"{Name}: ");
        sb.Append($"{Description}");
        sb.Append($"\n\tValue: {Value}, ");
        sb.Append($"Weight: {Weight} , ");
        sb.Append($"Durability: {Durability}");
        
        return sb.ToString();
    }
    public virtual void CalculateValue(bool calculateValuable)
    {
        decimal baseValue = (decimal)Math.Pow(RequiredLevel, 1.15) * 1.5M;
        baseValue += Durability * 1.5M;

        switch (this)
        {
            case Weapon weapon:
                baseValue += weapon.AttackPower * 1.2M;
                break;
            case Armor armor:
                baseValue += armor.DefensePower * 0.5M;
                baseValue += armor.Resistance * 0.5M;
                break;
            case Consumable consumable:
                baseValue += consumable.Power * 1.8M;
                break;
            case Valuable:
                if (calculateValuable)
                {
                    decimal specialMultiplier = (decimal)(_rng.NextDouble() * 1.2 + 1.7); // Slightly lower
                    baseValue *= specialMultiplier;
                }
                break;
        }

        decimal variance = (decimal)(_rng.NextDouble() * 0.2) + 0.9M; 
        Value = Math.Round(baseValue * variance, 2);
    }
    public virtual void CalculateStatsByLevel()
    {
        Durability = _rng.Next(3, 9);

        switch (this)
        {
            case Weapon weapon:
                weapon.AttackPower = _rng.Next(RequiredLevel * 6, RequiredLevel * 9); 
                break;
            case Armor armor:
                var total = _rng.Next(RequiredLevel * 5, RequiredLevel * 8);
                armor.DefensePower = _rng.Next(0, total + 1); 
                armor.Resistance = total - armor.DefensePower; 
                break;
            case Consumable consumable:
                Durability = 3;
                consumable.Power = consumable.ConsumableType switch
                {
                    ConsumableType.Health => _rng.Next(RequiredLevel * 3, RequiredLevel * 5),
                    ConsumableType.Durability => _rng.Next(RequiredLevel, RequiredLevel * 2),
                    ConsumableType.Resource => _rng.Next(RequiredLevel * 2, RequiredLevel * 4),
                    _ => RequiredLevel * 2,// fallback value
                };
                break;

            default:
                Durability = RequiredLevel % 2 + 1;
                break;
        }
    }
    public virtual void CalculateLevelByStats()
    {
        switch (this)
        {
            case Weapon weapon:
                RequiredLevel = (int)Math.Round(weapon.AttackPower / 9.5M);
                break;

            case Armor armor:
                var total = armor.DefensePower + armor.Resistance;
                RequiredLevel = (int)Math.Round(total / 8.5M);
                break;

            case Consumable consumable:
                RequiredLevel = consumable.ConsumableType switch
                {
                    ConsumableType.Health => (int)Math.Round(consumable.Power / 4M),
                    ConsumableType.Durability => (int)Math.Round(consumable.Power / 1.5M),
                    ConsumableType.Resource => (int)Math.Round(consumable.Power / 3M),
                    _ => 1
                };
                break;
            case Valuable:
                    decimal estimatedBaseValue = Value / 2.0M;
                    decimal durabilityValue = Durability * 2M;

                    RequiredLevel = Math.Max(1, (int)Math.Round((estimatedBaseValue - durabilityValue) / 5M));                    
                break;
            default:
                RequiredLevel = 1;
                break;
        }

        RequiredLevel = Math.Max(1, RequiredLevel); // Safety cap
    }
    public virtual decimal GetBuyPrice() => Value; // Merchant sells at full price
    public virtual decimal GetSellPrice() => Math.Round(Value * 0.75M, 2); // Player sells for 75%
    public virtual decimal GetPurificationPrice() => throw new InvalidOperationException($"Purification not allowed for {Name}");
    public virtual decimal GetReforgePrice() => throw new InvalidOperationException($"Reforge not allowed for {Name}");
    public virtual decimal GetEnchantmentPrice() => throw new InvalidOperationException($"Enchanting not allowed for {Name}");
}
