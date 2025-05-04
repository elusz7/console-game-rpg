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
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public void AddItem(Item item)
    {
        if (item is Consumable)
        {
            AddConsumable(item);
            return;
        }

        decimal currentCarryingWeight = Items
            .Where(i => i is not Consumable)
            .Sum(i => i.Weight);

        decimal wiggleRoom = Capacity - currentCarryingWeight;

        if (Items.Contains(item))
        {
            throw new DuplicateItemException($"You already have {item.Name} in your inventory.");
        }
        else if (item.Weight <= wiggleRoom)
        {
            Items.Add(item);
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
    public void RemoveItem(Item item)
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
    private void AddConsumable(IItem item)
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
            }
            else
            {
                throw new InventoryException($"You cannot carry more than {consumableCap} {consumable.ConsumableType} consumables.");
            }
        }
    }
}
