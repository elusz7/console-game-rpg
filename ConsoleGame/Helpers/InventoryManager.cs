using ConsoleGameEntities.Data;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Helpers;

public class InventoryManager(GameContext context, InputManager inputManager, OutputManager outputManager)
{
    private readonly GameContext _context = context;
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;

    private Player player;
    private static string sortOrder = "asc";
    private List<Item> items;

    public void InventoryMenu(IPlayer character)
    {
        player = (Player)character;
        items = _context.Items
            .Include(i => i.Inventory)
            .ThenInclude(inv => inv.Player).ToList();

        _outputManager.Clear();

        while (true)
        {
            _outputManager.WriteLine("Inventory Menu");
            _outputManager.WriteLine("1. Search for item by name");
            _outputManager.WriteLine("2. List items by type");
            _outputManager.WriteLine("3. Sort items");
            _outputManager.WriteLine("4. List items equippable by player");
            _outputManager.WriteLine("5. Add item to inventory");
            _outputManager.WriteLine("6. Remove item from inventory");
            _outputManager.WriteLine("7. Return to main menu");

            _outputManager.Write("Choose an option: ");
            _outputManager.Display();
            var input = _inputManager.ReadString();

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
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
    public void SearchItemByName()
    {
        _outputManager.Write("\nEnter item name to search: ");
        _outputManager.Display();
        string itemName = _inputManager.ReadString();

        var itemsFound = items
            .Where(i => i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!itemsFound.Any())
        {
            _outputManager.WriteLine("\nNo items found\n");
            _outputManager.Display();
        }
        else
        {
            ListItems(itemsFound);
        }
    }
    public void ListItemsByType()
    {
        string category;
        _outputManager.Write("\nWeapon or Armor? ");
        _outputManager.Display();
        category = _inputManager.ReadString().ToLower();

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
            _outputManager.WriteLine("Invalid category.\n");
            _outputManager.Display();
        }
    }
    public void SortItems()
    {
        while (true)
        {
            _outputManager.Write("\nSort Options:"
                + "\n1. Sort by Name"
                + "\n2. Sort by Attack Value"
                + "\n3. Sort by Defense Value"
                + $"\n4. Change Sort Order (currently: {sortOrder})"
                + "\n5. Return to Inventory Menu"
                + "\n\tChoose an option: ");
            _outputManager.Display();

            var input = _inputManager.ReadString();
            _outputManager.WriteLine();

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
                    _outputManager.WriteLine("Invalid option. Try again.\n");
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

        _outputManager.WriteLine($"\nCapacity: {currentCarryingWeight} / {player.Inventory.Capacity}\n");
        var equippableItems = items
            .Where(i => i.Weight <= weightAvailable)
            .Where(i => i.InventoryId == null)
            .ToList();

        if (!equippableItems.Any())
            _outputManager.WriteLine("No equippable items available.\n");

        _outputManager.Display();

        ListItems(equippableItems);
    }
    public void AddItemToInventory()
    {
        _outputManager.Write("\nEnter the name of the item to add to your inventory: ");
        _outputManager.Display();
        string itemName = _inputManager.ReadString();
        _outputManager.WriteLine();

        var itemToAdd = items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (itemToAdd != null)
        {
            try
            {
                player.Inventory.AddItem(itemToAdd);
                _context.UpdateInventory(player.Inventory);
                _outputManager.WriteLine($"Item {itemToAdd.Name} added to inventory.\n");
            }
            catch (Exception ex) when (ex is DuplicateItemException || ex is OverweightException || ex is InventoryException)
            {
                _outputManager.WriteLine(ex.Message);
                _outputManager.WriteLine($"Item {itemToAdd.Name} not added to inventory.\n");
            }
        }
        else
        {
            _outputManager.WriteLine($"Item {itemName} not found.\n");
        }
        _outputManager.Display();
    }
    public void RemoveItemFromInventory()
    {
        _outputManager.WriteLine();
        for (int i = 0; i < player.Inventory.Items.ToList().Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {player.Inventory.Items.ToList()[i].Name}");
        }
        
        _outputManager.Write("\nWhich item would you like to remove: ");
        _outputManager.Display();
        string input = _inputManager.ReadString();

        bool isNumber = int.TryParse(input, out int index);
        try
        {
            if ((isNumber) && (index > 0 && index <= player.Inventory.Items.Count))
            {
                var itemToRemove = player.Inventory.Items.ToList()[index - 1];
                player.Inventory.RemoveItem(itemToRemove);
                _outputManager.WriteLine($"Item {itemToRemove.Name} removed from inventory.\n");
                _context.UpdateInventory(player.Inventory);
            }
            else
            {
                player.Inventory.RemoveItem(input);
                _outputManager.WriteLine($"Item {input} removed from inventory.\n");
                _context.UpdateInventory(player.Inventory);
            }
        }
        catch (ItemNotFoundException ex)
        {
            _outputManager.WriteLine(ex.Message);
        }
        _outputManager.WriteLine();
        _outputManager.Display();
        
    }
    private void ListItems<T>(List<T> itemList) where T : Item
    {
        foreach (var item in itemList)
        {
            _outputManager.Write($"\t{item.Name}", ConsoleColor.Yellow);

            if (item is Weapon weapon)
            {
                _outputManager.Write(", Attack Power: ");
                _outputManager.Write(weapon.AttackPower.ToString(), ConsoleColor.Red);
            }
            else if (item is Armor armor)
            {
                _outputManager.Write(", Defense Power: ");
                _outputManager.Write(armor.DefensePower.ToString(), ConsoleColor.Green);
            }

            _outputManager.Write(", Weight: ");
            _outputManager.Write(item.Weight.ToString(), ConsoleColor.Magenta);

            if (item.Inventory != null)
            {
                _outputManager.Write(", Held by: ");
                _outputManager.Write(item.Inventory.Player.Name, ConsoleColor.Cyan);
            }

            _outputManager.WriteLine();
        }

        _outputManager.WriteLine();
    }
}