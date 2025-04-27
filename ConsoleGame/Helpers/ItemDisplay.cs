using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers;

public class ItemDisplay(InputManager inputManager, OutputManager outputManager, ItemDao itemDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly ItemDao _itemDao = itemDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("\nItem Display Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. List All Items"
                + "\n2. Search For Item(s) By Name"
                + "\n3. List Items By Type"
                + $"\n4. Change Sort Order (currently: {_itemDao.SortOrder})"
                + "\n4. Return To Inventory Main Menu"
                    + "\n\tChoose an option: ";
            var input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    ListAllItems();
                    break;
                case "2":
                    SearchItemByName();
                    break;
                case "3":
                    ListItemsByType();
                    break;
                case "4":
                    _itemDao.SortOrder = _itemDao.SortOrder == "ASC" ? "DESC" : "ASC";
                    break;
                case "5":
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
    private void ListAllItems()
    {
        var items = _itemDao.GetAllItems();
        if (items.Count != 0)
        {
            ListItemsFullDetails(items);
        }
        else
        {
            _outputManager.WriteLine("No items found.");
            _outputManager.Display();
        }
    }
    private void SearchItemByName()
    {
        string itemName = _inputManager.ReadString("\nEnter item name to find: ");

        var itemsFound = _itemDao.GetAllItems(itemName);

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
        string category = _inputManager.ReadString("\nWeapon or Armor? ", new[] {"weapon", "armor"}).ToLower();

        List<Item> items = _itemDao.GetItemsByType(category);

        if (items.Count != 0)
        {
            ListItemsFullDetails(items);
        }
        else
        {
            _outputManager.WriteLine("No items found.");
            _outputManager.Display();
        }
    }
    private void ListItemsFullDetails<T>(List<T> itemList) where T : Item
    {
        foreach (var item in itemList)
        {
            DisplayItemDetails(item);
        }
    }
    private void DisplayItemDetails(Item item)
    {
        if (item is Weapon weapon)
            _outputManager.WriteLine(weapon.ToString(), ConsoleColor.Magenta);
        else if (item is Armor armor)
            _outputManager.WriteLine(armor.ToString(), ConsoleColor.DarkYellow);
    }
}