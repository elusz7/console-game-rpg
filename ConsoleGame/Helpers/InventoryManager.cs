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
            _outputManager.WriteLine("Inventory Main Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. View Items"
                + "\n2. Manage Items"
                + "\n3. Manage Character's Inventory"
                + $"\n4. Return to {origin}"
                + "\n\tChoose an option: ";

            var input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    ItemDisplayMenu();
                    break;
                case "2":
                    ItemManagementMenu();
                    break;
                case "3":
                    CharacterInventoryMenu();
                    break;
                case "4":
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.\n");
                    break;
            }
        }
    }
    private void ItemDisplayMenu()
    {
        while (true)
        {
            _outputManager.WriteLine("\nItem Display Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. Search For Item(s) By Name"
                + "\n2. List Items By Type"
                + "\n3. Sort & List Items"
                + "\n4. Return To Inventory Main Menu"
                    + "\n\tChoose an option: ";
            var input = _inputManager.ReadString(menuPrompt);

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
    private void ItemManagementMenu()
    {
        while (true)
        {
            _outputManager.WriteLine("\nItem Management Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. Add Item"
                + "\n2. Edit Item"
                + "\n3. Remove Item"
                + "\n4. Return to Inventory Main Menu"
                + "\n\tChoose an option: ";
            string input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    AddItem();
                    break;
                case "2":
                    EditItem();
                    break;
                case "3":
                    RemoveItem();
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
    private void CharacterInventoryMenu()
    {
        if (player == null)
        {
            SelectCharacter();
        }

        while (true)
        {
            _outputManager.WriteLine($"\n{player.Name}'s Inventory Management", ConsoleColor.Cyan);
            string menuPrompt = "1. List Items Equippable"
                + "\n2. Add Item to Inventory"
                + "\n3. Remove Item from Inventory"
                + "\n4. Return to Inventory Main Menu"
                + "\n\tChoose an option: ";
            var input = _inputManager.ReadString(menuPrompt);

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
        string itemName = _inputManager.ReadString("\nEnter item name to find: ");

        var itemsFound = items
            .Where(i => i.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!itemsFound.Any())
        {
            _outputManager.WriteLine($"\n\tNo items found matching [{itemName}]");
        }
        else
        {
            _outputManager.WriteLine($"\n\t{itemsFound.Count} items found matching [{itemName}]\n");
            ListItemsFullDetails(itemsFound);
        }
    }
    private void ListItemsByType()
    {
        string category = _inputManager.ReadString("\nWeapon or Armor? ").ToLower();

        if (category == "weapon")
        {
            ListItemsFullDetails(items.OfType<Weapon>().ToList());
        }
        else if (category == "armor")
        {
            ListItemsFullDetails(items.OfType<Armor>().ToList());
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
            string menuPrompt = "1. Sort by Name"
                + "\n2. Sort by Attack Value"
                + "\n3. Sort by Defense Value"
                + $"\n4. Change Sort Order (currently: {sortOrder})"
                + "\n5. Return to Inventory Menu"
                + "\n\tChoose an option: ";
            var input = _inputManager.ReadString(menuPrompt);

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

        ListItemsFullDetails(sortedItems);
    }
    private void SortByAttack()
    {
        var weapons = items.OfType<Weapon>().ToList();
        
        var sortedWeapons = sortOrder == "ASC"
            ? weapons.OrderBy(w => w.AttackPower).ToList()
            : weapons.OrderByDescending(w => w.AttackPower).ToList();

        ListItemsFullDetails(sortedWeapons);
    }
    private void SortByDefense()
    {
        var armors = items.OfType<Armor>().ToList();

        var sortedArmors = sortOrder == "ASC"
            ? armors.OrderBy(a => a.DefensePower).ToList()
            : armors.OrderByDescending(a => a.DefensePower).ToList();

        ListItemsFullDetails(sortedArmors);
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

        ListItemsFullDetails(equippableItems);
    }
    private void AddItemToInventory()
    {
        string itemName = _inputManager.ReadString("\nEnter the name of the item to add to your inventory: ");

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
        
        int index = _inputManager.ReadInt("\nEnter Item Number To Remove (-1 to cancel): ", player.Inventory.Items.ToList().Count, true);

        if (index == -1)
        {
            _outputManager.WriteLine("Item Removal Cancelled.");
            return;
        }

        try
        {
            var itemToRemove = player.Inventory.Items.ToList()[index - 1];
            player.Inventory.RemoveItem(itemToRemove);
            _outputManager.WriteLine($"Item {itemToRemove.Name} removed from inventory.");
            _context.UpdateInventory(player.Inventory);
            _context.SaveChanges();
        }
        catch (ItemNotFoundException ex)
        {
            _outputManager.WriteLine(ex.Message);
        }        
    }
    private void ListItemsFullDetails<T>(List<T> itemList) where T : Item
    {
        foreach (var item in itemList)
        {
            DisplayItemDetails(item);
        }
    }
    private Item SelectItem(string purpose) 
    {
        string prompt = "\tEnter item number: ";
        if (purpose == "EditItem")
            prompt = "\tEnter item number to edit: ";
        else if (purpose == "RemoveItem")
            prompt = "\tEnter item number to remove: ";

        for (int i = 0; i < items.Count; i++)
            {
                _outputManager.WriteLine($"{i + 1}. {items[i].Name}");
            }
        int index = _inputManager.ReadInt(prompt, items.Count);
        return items[index - 1];
    }
    private void SelectCharacter()
    {
        var characters = _context.Players.ToList();
        _outputManager.WriteLine();
        for (int i = 0; i < characters.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {characters[i].Name}");
        }
        
        int index = _inputManager.ReadInt("\tEnter number of character for which inventory you'd like to manage: ", characters.Count);

        player = characters[index - 1];
    }    
    private void AddItem()
    {
        string itemType = _inputManager.ReadString("\nWeapon or armor? ", ["weapon", "armor"]).ToLower();

        string name = _inputManager.ReadString("Enter item name: ");

        string description = _inputManager.ReadString("Enter item description: ");

        decimal value = _inputManager.ReadDecimal("Enter item value: ");

        int durability = _inputManager.ReadInt("Enter item durability: ");

        decimal weight = _inputManager.ReadDecimal("Enter item weight: ");

        int attackPower = 0;
        int defensePower = 0;

        if (itemType.Equals("weapon"))
            attackPower = _inputManager.ReadInt("Enter item attack power: ");
        else if (itemType.Equals("armor"))
            defensePower = _inputManager.ReadInt("Enter item defense power: ");

        Item item = itemType switch
        {
            "weapon" => new Weapon(name, value, description, durability, weight, attackPower),
            "armor" => new Armor(name, value, description, durability, weight, defensePower),
            _ => throw new ArgumentException("Invalid item type.")
        };

        items.Add(item);
        _context.Items.Add(item);
        _context.SaveChanges();

        _outputManager.WriteLine($"\nItem {item.Name} successfully created.", ConsoleColor.Green);
    }
    private void EditItem()
    {
        _outputManager.WriteLine();
        Item item = SelectItem("EditItem");

        while (true)
        {
            _outputManager.WriteLine($"\nEditing item {item.Name}.", ConsoleColor.Cyan);
            DisplayItemDetails(item);

            string response = _inputManager.ReadString("\nWhat property would you like to edit? (exit to leave): ",
                new[] { "name", "description", "value", "durability", "weight", "attack power", "defense power", "exit" }).ToLower();

            if (response == "exit")
            {
                _context.UpdateItem(item);
                _context.SaveChanges();
                _outputManager.WriteLine($"\nItem {item.Name} successfully updated.", ConsoleColor.Green);
                return;
            }

            UpdateItemProperty(item, response);
        }
    }

    private void DisplayItemDetails(Item item)
    {
        if (item is Weapon weapon)
            _outputManager.WriteLine(weapon.ToString(), ConsoleColor.Magenta);
        else if (item is Armor armor)
            _outputManager.WriteLine(armor.ToString(), ConsoleColor.DarkYellow);
    }

    private void UpdateItemProperty(Item item, string property)
    {
        switch (property)
        {
            case "name":
                item.Name = _inputManager.ReadString("\nEnter new name: ");
                break;
            case "description":
                item.Description = _inputManager.ReadString("\nEnter new description: ");
                break;
            case "value":
                item.Value = _inputManager.ReadDecimal("\nEnter new value: ");
                break;
            case "durability":
                item.Durability = _inputManager.ReadInt("\nEnter new durability: ");
                break;
            case "weight":
                item.Weight = _inputManager.ReadDecimal("\nEnter new weight: ");
                break;
            case "attack power":
                if (item is Weapon weapon)
                    weapon.AttackPower = _inputManager.ReadInt("\nEnter new attack power: ");
                else
                    _outputManager.WriteLine("Item is not a weapon.");
                break;
            case "defense power":
                if (item is Armor armor)
                    armor.DefensePower = _inputManager.ReadInt("\nEnter new defense power: ");
                else
                    _outputManager.WriteLine("Item is not an armor.");
                break;
        }
    }


    private void RemoveItem()
    {
        _outputManager.WriteLine();
        Item itemToDelete = SelectItem("RemoveItem");

        string confirm = _inputManager.ReadString($"\nPlease confirm deletion of {itemToDelete.Name} (y/n): ", ["y", "n"]);

        if (confirm == "y")
        {
            //update local list
            items.Remove(itemToDelete);

            //update database
            _context.Items.Remove(itemToDelete);
            _context.SaveChanges();

            //confirm deletion
            _outputManager.WriteLine("Item has been removed successfully!", ConsoleColor.Green);
        }
        else
        {
            _outputManager.WriteLine($"Deletion cancelled. Item [{itemToDelete.Name}] has not been removed.", ConsoleColor.Red);
        }
    }
}