using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Exceptions;

namespace ConsoleGameEntities.Models.Items;

public class Item : IItem
{
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
}
