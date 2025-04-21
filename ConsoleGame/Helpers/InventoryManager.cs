using ConsoleGame.Services;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Helpers;

public class InventoryManager
{
    private Player player;
    private List<Item> itemList;
    private static string sortOrder = "asc";

    public void InventoryMenu(IPlayer character, List<Item> items, GameEngine home)
    {
        player = (Player)character;
        itemList = items;

        Console.Clear();

        while (true)
        {
            Console.WriteLine("Inventory Menu");
            Console.WriteLine("1. Search for item by name");
            Console.WriteLine("2. List items by type");
            Console.WriteLine("3. Sort items");
            Console.WriteLine("4. List items equippable by player");
            Console.WriteLine("5. Add item to inventory");
            Console.WriteLine("6. Remove item from inventory");
            Console.WriteLine("7. Return to main menu");

            Console.Write("Choose an option: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    SearchItemByName();
                    break;
                case "2":
                    ListItemsByType();
                    break;
                case "3":
                    SortItems();
                    break;
                case "4":
                    ListEquippableItems();
                    break;
                case "5":
                    AddItemToInventory();
                    break;
                case "6":
                    RemoveItemFromInventory();
                    break;
                case "7":
                    Console.Clear();
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    public void SearchItemByName()
    {
        Console.Write("\nEnter item name to search: ");
        string itemName = Console.ReadLine();

        var itemsFound = itemList
            .Where(i => i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!itemsFound.Any())
            Console.WriteLine("\nNo items found");

        foreach (var item in itemsFound)
        {
            Console.WriteLine($"\tItem Name: {item.Name}, Type: {item.ItemType}");
        }

        Console.WriteLine();
    }
    public void ListItemsByType()
    {
        string[] validTypes = { "Weapon", "Armor", "weapon", "armor" };
        string category;
        while (true)
        {
            Console.Write("\nWeapon or Armor? ");
            category = Console.ReadLine();

            if (!validTypes.Contains(category))
            {
                Console.WriteLine("Invalid category. Please enter 'Weapon' or 'Armor'.");
                continue;
            }
            else
            {
                break;
            }
        }

        switch (category.ToLower())
        {
            case "weapon":
                var weapons = itemList.OfType<Weapon>().ToList();
                foreach (var weapon in weapons)
                {
                    Console.WriteLine($"\tWeapon Name: {weapon.Name}, AttackPower: {weapon.AttackPower}");
                }
                Console.WriteLine();
                break;
            case "armor":
                var armors = itemList.OfType<Armor>().ToList();
                foreach (var armor in armors)
                {
                    Console.WriteLine($"\tArmor Name: {armor.Name}, DefensePower: {armor.DefensePower}");
                }
                Console.WriteLine();
                break;
        }
    }
    public void SortItems()
    {
        while (true)
        {
            Console.WriteLine("\nSort Options:");
            Console.WriteLine("1. Sort by Name");
            Console.WriteLine("2. Sort by Attack Value");
            Console.WriteLine("3. Sort by Defense Value");
            Console.WriteLine($"4. Change Sort Order (currently: {sortOrder})");
            Console.WriteLine("5. Return to Inventory Menu");

            Console.Write("Choose an option: ");
            var input = Console.ReadLine();
            Console.WriteLine();

            switch (input)
            {
                case "1":
                    SortByName();
                    break;
                case "2":
                    SortByAttack();
                    break;
                case "3":
                    SortByDefense();
                    break;
                case "4":
                    sortOrder = sortOrder == "asc" ? "desc" : "asc";
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid option. Try again.\n");
                    break;
            }

        }
    }
    private void SortByName()
    {
        List<Item> sorted;
        if (sortOrder == "asc")
        {
            sorted = itemList.OrderBy(i => i.Name).ToList();
        }
        else
        {
            sorted = itemList.OrderByDescending(i => i.Name).ToList();
        }
        foreach (var item in sorted)
        {
            Console.WriteLine($"\tItem Name: {item.Name}, Type: {item.ItemType}");
        }
    }
    private void SortByAttack()
    {
        List<Weapon> sorted;
        if (sortOrder == "asc")
        {
            sorted = itemList.OfType<Weapon>().OrderBy(w => w.AttackPower).ToList();
        }
        else
        {
            sorted = itemList.OfType<Weapon>().OrderByDescending(w => w.AttackPower).ToList();
        }
        foreach (var item in sorted)
        {
            Console.WriteLine($"\tItem Name: {item.Name}, AttackPower: {item.AttackPower}");
        }
    }
    private void SortByDefense()
    {
        List<Armor> sorted;
        if (sortOrder == "asc")
        {
            sorted = itemList.OfType<Armor>().OrderBy(w => w.DefensePower).ToList();
        }
        else
        {
            sorted = itemList.OfType<Armor>().OrderByDescending(w => w.DefensePower).ToList();
        }
        foreach (var item in sorted)
        {
            Console.WriteLine($"\tItem Name: {item.Name}, DefensePower: {item.DefensePower}");
        }
    }
    public void ListEquippableItems()
    {
        decimal currentCarryingWeight = player.Inventory.Items.Sum(i => i.Weight);
        decimal weightAvailable = player.Inventory.Capacity - currentCarryingWeight;

        Console.WriteLine($"\nCapacity: {currentCarryingWeight} / {player.Inventory.Capacity}\n");
        var equippableItems = itemList
            .Where(i => i.Weight <= weightAvailable)
            .Where(i => i.InventoryId != player.Inventory.Id)
            .ToList();

        if (!equippableItems.Any())
            Console.WriteLine("No equippable items available.\n");

        foreach (var item in equippableItems)
        {
            Console.WriteLine($"\tItem Name: {item.Name}, Type: {item.ItemType}, Weight: {item.Weight}");
        }
        Console.WriteLine();
    }
    public void AddItemToInventory()
    {
        Console.Write("\nEnter the name of the item to add to your inventory: ");
        string itemName = Console.ReadLine();
        Console.WriteLine();

        var itemToAdd = itemList.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (itemToAdd != null)
        {
            player.Inventory.AddItem(itemToAdd);
        }
        else
        {
            Console.WriteLine($"Item {itemName} not found.");
        }
        Console.WriteLine();
    }

    public void RemoveItemFromInventory()
    {
        Console.WriteLine();
        for (int i = 0; i < player.Inventory.Items.ToList().Count; i++)
        {
            Console.WriteLine($"{i + 1}. {player.Inventory.Items.ToList()[i].Name}");
        }
        Console.Write("\nEnter the number of the item to remove from your inventory: ");
        string input = Console.ReadLine();

        if (int.TryParse(input, out int index) && index > 0 && index <= player.Inventory.Items.Count)
        {
            var itemToRemove = player.Inventory.Items.ToList()[index - 1];
            player.Inventory.RemoveItem(itemToRemove);
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("Invalid input. Please try again.");
        }
    }
}