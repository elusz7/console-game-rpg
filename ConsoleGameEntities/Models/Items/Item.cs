using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Exceptions;

namespace ConsoleGameEntities.Models.Items;

public abstract class Item : IItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public string Description { get; set; }
    public int Durability { get; set; }
    public decimal Weight { get; set; }
    [NotMapped]
    public bool IsEquipped { get; set; }
    public bool IsCursed { get; set; }
    public int RequiredLevel { get; set; }
    public string ItemType { get; set; }

    public int? InventoryId { get; set; }
    public virtual Inventory? Inventory { get; set; }
    public int? MonsterId { get; set; }
    public Monster? Monster { get; set; }

    public abstract int Use();
    public virtual void Purify()
    {
        if (!IsCursed)
            throw new ItemPurificationException("Item is not cursed.");

        if (Inventory == null)
            throw new ItemPurificationException("Item is not in an inventory.");

        var price = (int)Math.Floor(Value * 0.5M);

        if (Inventory.Gold < price)
            throw new ItemPurificationException($"{price - Inventory.Gold}");

        Inventory.Gold -= price;
        IsCursed = false;
    }
    public virtual void Sell()
    {
        if (IsEquipped)
            throw new InvalidOperationException("Cannot sell an equipped item.");

        if (Inventory != null)
            Inventory.Gold += (int)Math.Floor(Value * 0.75M);

        InventoryId = null;
        Inventory = null;
    }

    public virtual void Buy(IPlayer player) {
        var price = (int)Math.Floor(Value * 1.25M);

        if (player.Inventory.Gold < price)
            throw new ItemPurchaseException($"{price - player.Inventory.Gold}");

        try
        {
            player.Inventory.AddItem(this);
        }
        catch (InventoryException ex) {
            throw new ItemPurchaseException(ex.Message);
        }

        player.Inventory.Gold -= price;
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
