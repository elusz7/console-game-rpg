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

    private Player? player;
    private static string sortOrder = "ASC";
    private List<Item>? items;
    private string returnPoint;

    public void InventoryMainMenu(string origin, Player? character = null)
    {
        returnPoint = origin;
        player = character;
        items = _context.Items
            .Include(i => i.Inventory)
            .ThenInclude(static inv => inv.Player).ToList();

        _outputManager.Clear();

        while (true)
        {
            _outputManager.Write("Inventory Main Menu", ConsoleColor.Cyan);
            _outputManager.Write("\n1. View Items"
                + "\n2. Manage Items"
                + "\n3. Manage Character's Inventory"
                + $"\n4. Return to {origin}"
                + "\n\tChoose an option: ");
            _outputManager.Display();
            var input = _inputManager.ReadString();

            switch (input)
            {
                case "1":
                    ItemDisplayMenu();
                    break;
                case "2":
                    _outputManager.WriteLine("Item Management Menu not implemented yet.\n");
                    break;
                case "3":
                    CharacterInventoryMenu();
                    break;
                case "4":
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
    private void ItemDisplayMenu()
    {
        while (true)
        {
            _outputManager.WriteLine("\nItem Display Menu", ConsoleColor.Cyan);
            _outputManager.Write("1. Search For Item(s) By Name"
                + "\n2. List Items By Type"
                + "\n3. Sort & List Items"
                + "\n4. Return To Inventory Main Menu"
                    + "\n\tChoose an option: ");
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
                    Console.WriteLine();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
    private void CharacterInventoryMenu()
    {
        if (player == null)
        {
            SelectCharacter();
        }

        while (true)
        {
            _outputManager.WriteLine($"\n{player.Name}'s Inventory Management", ConsoleColor.Cyan);
            _outputManager.Write("1. List Items Equippable"
                + "\n2. Add Item to Inventory"
                + "\n3. Remove Item from Inventory"
                + "\n4. Return to Inventory Main Menu"
                + "\n\tChoose an option: ");
            _outputManager.Display();
            var input = _inputManager.ReadString();

            switch (input)
            {
                case "1":
                    ListEquippableItems();
                    break;
                case "2":
                    AddItemToInventory();
                    break;
                case "3":
                    RemoveItemFromInventory();
                    break;
                case "4":
                    if (returnPoint == "Main Menu")
                        player = null;
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
    private void SearchItemByName()
    {
        _outputManager.Write("\nEnter item name to find: ");
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
    private void ListItemsByType()
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
            _outputManager.WriteLine("Invalid category.");
            _outputManager.Display();
        }
    }
    private void SortItems()
    {
        while (true)
        {
            _outputManager.WriteLine("\nItem Sorted List Options:", ConsoleColor.Cyan);
            _outputManager.Write("1. Sort by Name"
                + "\n2. Sort by Attack Value"
                + "\n3. Sort by Defense Value"
                + $"\n4. Change Sort Order (currently: {sortOrder})"
                + "\n5. Return to Inventory Menu"
                + "\n\tChoose an option: ");
            _outputManager.Display();

            var input = _inputManager.ReadString();

            switch (input)
            {
                case "1":
                    _outputManager.WriteLine();
                    SortByName();
                    break;
                case "2":
                    _outputManager.WriteLine();
                    SortByAttack();
                    break;
                case "3":
                    _outputManager.WriteLine();
                    SortByDefense();
                    break;
                case "4":
                    sortOrder = sortOrder == "ASC" ? "DESC" : "ASC";
                    _outputManager.WriteLine($"\nNow sorting by {sortOrder}");
                    break;
                case "5":
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Try again.\n");
                    break;
            }
        }
    }
    private void SortByName()
    {
        var sortedItems = sortOrder == "ASC"
            ? items.OrderBy(i => i.Name).ToList()
            : items.OrderByDescending(i => i.Name).ToList();

        ListItems(sortedItems);
    }
    private void SortByAttack()
    {
        var weapons = items.OfType<Weapon>().ToList();
        
        var sortedWeapons = sortOrder == "ASC"
            ? weapons.OrderBy(w => w.AttackPower).ToList()
            : weapons.OrderByDescending(w => w.AttackPower).ToList();

        ListItems(sortedWeapons);
    }
    private void SortByDefense()
    {
        var armors = items.OfType<Armor>().ToList();

        var sortedArmors = sortOrder == "ASC"
            ? armors.OrderBy(a => a.DefensePower).ToList()
            : armors.OrderByDescending(a => a.DefensePower).ToList();

        ListItems(sortedArmors);
    }
    private void ListEquippableItems()
    {
        decimal currentCarryingWeight = player.Inventory.Items.Sum(i => i.Weight);
        decimal weightAvailable = player.Inventory.Capacity - currentCarryingWeight;

        _outputManager.WriteLine($"\nCapacity: {currentCarryingWeight} / {player.Inventory.Capacity}");
        var equippableItems = items
            .Where(i => i.Weight <= weightAvailable)
            .Where(i => i.InventoryId == null)
            .ToList();

        if (!equippableItems.Any())
            _outputManager.WriteLine("No equippable items available.\n");

        _outputManager.Display();

        ListItems(equippableItems);
    }
    private void AddItemToInventory()
    {
        _outputManager.Write("\nEnter the name of the item to add to your inventory: ");
        _outputManager.Display();
        string itemName = _inputManager.ReadString();

        var itemToAdd = items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (itemToAdd != null)
        {
            try
            {
                player.Inventory.AddItem(itemToAdd);
                _context.UpdateInventory(player.Inventory);
                _outputManager.WriteLine($"Item {itemToAdd.Name} added to inventory.");
            }
            catch (Exception ex) when (ex is DuplicateItemException || ex is OverweightException || ex is InventoryException)
            {
                _outputManager.WriteLine(ex.Message);
                _outputManager.WriteLine($"Item {itemToAdd.Name} not added to inventory.");
            }
        }
        else
        {
            _outputManager.WriteLine($"Item {itemName} not found.\n");
        }
        _outputManager.Display();
    }
    private void RemoveItemFromInventory()
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
                _outputManager.WriteLine($"Item {itemToRemove.Name} removed from inventory.");
                _context.UpdateInventory(player.Inventory);
            }
            else
            {
                player.Inventory.RemoveItem(input);
                _outputManager.WriteLine($"Item {input} removed from inventory.");
                _context.UpdateInventory(player.Inventory);
            }
        }
        catch (ItemNotFoundException ex)
        {
            _outputManager.WriteLine(ex.Message);
        }
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
    }
    private void SelectCharacter()
    {
        var characters = _context.Players.ToList();
        _outputManager.WriteLine();
        for (int i = 0; i < characters.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {characters[i].Name}");
        }
        _outputManager.Write("\tWhich character's inventory would you like to manage? ", ConsoleColor.DarkGreen);
        _outputManager.Display();

        while (player == null) { 
            var input = _inputManager.ReadString();
            bool isNumber = int.TryParse(input, out int index);
            if (isNumber && index > 0 && index <= characters.Count)
            {
                player = characters[index - 1];
            }
            else if (characters.Any(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase)))
            {
                player = characters.FirstOrDefault(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                _outputManager.Write("Invalid selection. Please try again: ");
                _outputManager.Display();
            }
        }
    }    
}