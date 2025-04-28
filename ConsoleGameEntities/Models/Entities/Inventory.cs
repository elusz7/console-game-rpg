using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Models.Entities;

public class Inventory
{
    public int Id { get; set; }
    public int Gold { get; set; }
    public decimal Capacity { get; set; }
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; }
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    //adding, using, equipping, and removing items 
    public void AddItem(Item item)
    {
        decimal currentCarryingWeight = Items.Sum(i => i.Weight);

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

    public void UseItem(Item item)
    {
        if (Items.Contains(item))
        {
            if (item.Durability <= 0)
            {
                Console.WriteLine($"{item.Name} is broken and cannot be used anymore!");
            }
            else
            {
                if (item is Weapon weapon)
                {
                    weapon.Use();
                    Console.WriteLine($"{weapon.Name} has been used.");
                }
                else if (item is Armor armor)
                {
                    armor.Use();
                    Console.WriteLine($"{armor.Name} has been used.");
                }
            }
        }
        else
        {
            Console.WriteLine($"You do not have {item.Name} in your inventory.");
        }
    }
}
