using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Characters;

namespace ConsoleGameEntities.Models.Items;

public class Inventory
{
    public int Id { get; set; }
    public int Gold { get; set; }
    public int Capacity { get; set; }
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; }
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    //adding, using, equipping, and removing items 
    public void AddItem(Item item)
    {
        decimal currentCarryingWeight = 0;
        foreach (Item i in Items)
        {
            currentCarryingWeight += i.Weight;
        }

        decimal wiggleRoom = Capacity - currentCarryingWeight;

        if (Items.Contains(item))
        {
            Console.Error.WriteLine($"You already have {item.Name} in your inventory.");
        }
        else if (item.Weight <= wiggleRoom)
        {
            Items.Add(item);
            Console.WriteLine($"You have added {item.Name} to your inventory.");
        }
        else
        {
            Console.WriteLine($"You cannot add {item.Name} to your inventory. Exceeds carry weight capacity.");
        }
    }

    public void RemoveItem(Item item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
            Console.WriteLine($"You have removed {item.Name} from your inventory.");
        }
        else
        {
            Console.WriteLine($"You do not have {item.Name} in your inventory.");
        }
    }

    public void RemoveItem(string itemName)
    {
        Item itemToRemove = Items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (itemToRemove != null)
        {
            Items.Remove(itemToRemove);
            Console.WriteLine($"You have removed {itemToRemove.Name} from your inventory.");
        }
        else
        {
            Console.WriteLine($"You do not have {itemName} in your inventory.");
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
    public void UseItem(string itemName)
    {
        Item item = Items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (item != null)
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
            Console.WriteLine($"You do not have {itemName} in your inventory.");
        }
    }
}
