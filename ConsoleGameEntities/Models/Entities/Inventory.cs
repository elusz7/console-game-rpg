using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Models.Entities;

public class Inventory : IInventory
{
    public int Id { get; set; }
    public int Gold { get; set; }
    public decimal Capacity { get; set; }
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; }
    public virtual List<Item> Items { get; set; } = new();

    public virtual void AddItem(Item item)
    {
        if (item is Consumable)
        {
            AddConsumable(item);
            return;
        }

        if (item is Valuable)
        {
            Items.Add(item);
            item.InventoryId = Id;
            item.Inventory = this;
            return;
        }

        decimal wiggleRoom = Capacity - GetCarryingWeight();

        if (Items.Contains(item))
        {
            throw new DuplicateItemException($"You already have {item.Name} in your inventory.");
        }
        else if (item.Weight <= wiggleRoom)
        {
            Items.Add(item);
            item.InventoryId = Id;
            item.Inventory = this;
        }
        else if (item.Weight > wiggleRoom)
        {
            throw new OverweightException($"You cannot add {item.Name} to your inventory. Will exceed carrying capacity.");
        }
        else
        {
            throw new InventoryException($"An error has occured trying to add {item.Name} to your inventory.");
        }
    }
    public virtual void RemoveItem(Item item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
        }
        else
        {
            throw new ItemNotFoundException($"You do not have {item.Name} in your inventory.");
        }
    }
    private void AddConsumable(Item item)
    {
        var cap = Math.Max(3m, Capacity * 0.10m);
        int consumableCap = (int)Math.Floor(cap);


        if (item is Consumable consumable)
        {
            var currentCount = Items.OfType<Consumable>()
                .Count(c => c.ConsumableType == consumable.ConsumableType);

            if (currentCount < consumableCap)
            {
                Items.Add(consumable);
                item.InventoryId = Id;
                item.Inventory = this;
            }
            else
            {
                throw new InventoryException($"You cannot carry more than {consumableCap} {consumable.ConsumableType} consumables.");
            }
        }
    }
    public decimal GetCarryingWeight()
    {
        return Items
            .Where(i => i is Weapon || i is Armor)
            .Sum(i => i.Weight);
    }
    public bool ContainsConsumables()
    {
        return Items
            .OfType<Consumable>()
            .Any();
    }
    public void Sell(Item item)
    {
        Gold += (int)Math.Round(item.GetSellPrice());
        RemoveItem(item);
    }
    public void Buy(Item item)
    {
        var price = (int)Math.Round(item.GetBuyPrice());

        if (Gold < price)
            throw new ItemPurchaseException($"You are short by {price - Gold} gold.");

        try
        {
            AddItem(item);
        }
        catch (InventoryException ex)
        {
            throw new ItemPurchaseException(ex.Message);
        }

        Gold -= price;
    }
}
