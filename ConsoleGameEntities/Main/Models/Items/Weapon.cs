using System.Text;
using ConsoleGameEntities.Main.Exceptions;
using static ConsoleGameEntities.Main.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Main.Models.Items;

public class Weapon : Item
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
    public int AttackPower { get; set; }
    public DamageType DamageType { get; set; }
    
    public Weapon() { }
    public Weapon(string name, decimal value, string description, int durability, decimal weight, int attackPower)
    {
        Name = name;
        Value = value;
        Description = description;
        Durability = durability;
        Weight = weight;
        AttackPower = attackPower;
    }
    public override void Enchant()
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

        CalculateValue(false);

        Inventory.Gold -= price;
    }
    public override decimal GetPurificationPrice() => Value * 0.5M;
    public override decimal GetEnchantmentPrice() => Value * 1.5M;
    public override string ToString()
    {
        var sb = new StringBuilder(base.ToString());

        sb.Insert(0, "[Weapon] ");

        sb.Append($", Attack Power: {AttackPower}");

        if (Inventory != null)
        {
            sb.Append($", Held by: {Inventory.Player.Name}");
        }

        return sb.ToString();
    }
}
