using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Helpers;

public class InventoryManager
{
    private readonly GameContext _context;
    private Player player;
    private static string sortOrder = "asc";
    private List<Item> items;

    public InventoryManager(GameContext context)
    {
        _context = context;
    }

    public void InventoryMenu(IPlayer character)
    {
        player = (Player)character;
        items = _context.Items
            .Include(i => i.Inventory)
            .ThenInclude(inv => inv.Player).ToList();

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

        var itemsFound = items
            .Where(i => i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!itemsFound.Any())
        {
            Console.WriteLine("\nNo items found\n");
        }
        else
        {
            ListItems(itemsFound);
        }
    }

    public void ListItemsByType()
    {
        string category;
        Console.Write("\nWeapon or Armor? ");
        category = Console.ReadLine().ToLower();

        if (category == "weapon")
        {
            ListItems(items.OfType<Weapon>().ToList());
        }
        else if (category == "armor")
        {
            ListItems(items.OfType<Armor>().ToList());
        }
        else
        {
            Console.WriteLine("Invalid category.\n");
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
        var sortedItems = sortOrder == "asc"
            ? items.OrderBy(i => i.Name).ToList()
            : items.OrderByDescending(i => i.Name).ToList();

        ListItems(sortedItems);
    }
    private void SortByAttack()
    {
        var weapons = items.OfType<Weapon>().ToList();
        
        var sortedWeapons = sortOrder == "asc"
            ? weapons.OrderBy(w => w.AttackPower).ToList()
            : weapons.OrderByDescending(w => w.AttackPower).ToList();

        ListItems(sortedWeapons);
    }
    private void SortByDefense()
    {
        var armors = items.OfType<Armor>().ToList();

        var sortedArmors = sortOrder == "asc"
            ? armors.OrderBy(a => a.DefensePower).ToList()
            : armors.OrderByDescending(a => a.DefensePower).ToList();

        ListItems(sortedArmors);
    }
    public void ListEquippableItems()
    {
        decimal currentCarryingWeight = player.Inventory.Items.Sum(i => i.Weight);
        decimal weightAvailable = player.Inventory.Capacity - currentCarryingWeight;

        Console.WriteLine($"\nCapacity: {currentCarryingWeight} / {player.Inventory.Capacity}\n");
        var equippableItems = items
            .Where(i => i.Weight <= weightAvailable)
            .Where(i => i.InventoryId == null)
            .ToList();

        if (!equippableItems.Any())
            Console.WriteLine("No equippable items available.\n");

        ListItems(equippableItems);
    }
    public void AddItemToInventory()
    {
        Console.Write("\nEnter the name of the item to add to your inventory: ");
        string itemName = Console.ReadLine();
        Console.WriteLine();

        var itemToAdd = items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (itemToAdd != null)
        {
            player.Inventory.AddItem(itemToAdd);
            _context.UpdateInventory(player.Inventory);
        }
        else
        {
            Console.WriteLine($"Item {itemName} not found.\n");
        }
    }
    public void RemoveItemFromInventory()
    {
        Console.WriteLine();
        for (int i = 0; i < player.Inventory.Items.ToList().Count; i++)
        {
            Console.WriteLine($"{i + 1}. {player.Inventory.Items.ToList()[i].Name}");
        }
        while (true)
        {
            Console.Write("\nEnter the number of the item to remove from your inventory (-1 to quit): ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int index) && index > 0 && index <= player.Inventory.Items.Count)
            {
                var itemToRemove = player.Inventory.Items.ToList()[index - 1];
                player.Inventory.RemoveItem(itemToRemove);
                _context.UpdateInventory(player.Inventory);
                Console.WriteLine();
                return;
            }
            else if (index == -1)
            {
                Console.WriteLine("Operation cancelled.");
                return;
            }
            else
            {
                Console.WriteLine("Invalid input. Please try again.");
            }
        }
    }
    private void ListItems<T>(List<T> itemList) where T : Item
    {
        foreach (var item in itemList)
        {
            Console.Write($"\t{item.Name}");

            if (item is Weapon weapon)
                Console.Write($", Attack Power: {weapon.AttackPower}");
            else if (item is Armor armor)
                Console.Write($", Defense Power: {armor.DefensePower}");

            Console.Write($", Weight: {item.Weight}");

            if (item.Inventory != null)
                Console.Write($", Held by: {item.Inventory.Player.Name}");

            Console.WriteLine();
        }

        Console.WriteLine();
    }
}