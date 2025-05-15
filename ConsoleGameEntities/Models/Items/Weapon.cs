using System.Text;
using ConsoleGameEntities.Exceptions;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Items;

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

        var newStats = _rng.Next(RequiredLevel, RequiredLevel * 2 + 1);
        var baseNewStat = (int)Math.Floor(newStats * 0.33);
        var newAttackPower = _rng.Next(baseNewStat, newStats);

        AttackPower += newAttackPower;

        Value += (price * 0.33M);

        Inventory.Gold -= price;
    }
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
