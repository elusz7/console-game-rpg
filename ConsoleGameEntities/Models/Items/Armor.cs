using System.Text;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Items;

public class Armor : Item
{
    private readonly Random _rng = new(Guid.NewGuid().GetHashCode());

    public int DefensePower { get; set; }
    public int Resistance { get; set; }
    public ArmorType ArmorType { get; set; }
    

    public override void Reforge()
    {
        if (Inventory == null)
            throw new ItemReforgeException("Item is not in an inventory.");

        var price = (int)Math.Floor(Value * 0.33M);

        if (Inventory.Gold < price)
            throw new ItemReforgeException($"You are short by {price - Inventory.Gold} gold.");

        double failureChance = Math.Min(0.02 * RequiredLevel, 0.6);
        if (_rng.NextDouble() < failureChance)
        {
            Inventory.Gold -= price;
            throw new ItemReforgeException("Reforge attempt failed.");
        }

        var totalPower = DefensePower + Resistance;

        var newDefensePower = _rng.Next(0, totalPower + 1);
        var newResistance = totalPower - newDefensePower;

        if (newDefensePower == DefensePower && newResistance == Resistance)
            throw new ItemReforgeException("Reforge failed. No changes made.");

        DefensePower = newDefensePower;
        Resistance = newResistance;

        Inventory.Gold -= price;
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

        var newDefensePower = _rng.Next(1, newStats);
        var newResistance = newStats - newDefensePower;

        DefensePower += newDefensePower;
        Resistance += newResistance;

        Inventory.Gold -= price;
    }

    public override string ToString()
    {
        var sb = new StringBuilder(base.ToString());

        sb.Insert(0, "[Armor] ");

        sb.Append($", Defense Power: {DefensePower}, Resistance: {Resistance}");

        if (Inventory != null)
        {
            sb.Append($", Held by: {Inventory.Player.Name}");
        }

        return sb.ToString();
    }
}
