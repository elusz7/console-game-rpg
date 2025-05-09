using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers.DisplayHelpers;

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
            _outputManager.WriteLine("Item Display Menu", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. List All Items"
                + "\n2. Search For Item(s) By Name"
                + "\n3. List Items By Type"
                + $"\n4. Change Sort Order (currently: {_itemDao.SortOrder})"
                + "\n5. Return To Inventory Main Menu");

            var input = _inputManager.ReadMenuKey(5);

            switch (input)
            {
                case 1:
                    ListItems();
                    break;
                case 2:
                    ListItems("Search");
                    break;
                case 3:
                    ListItems("Type");
                    break;
                case 4:
                    _itemDao.SortOrder = _itemDao.SortOrder == "ASC" ? "DESC" : "ASC";
                    _outputManager.WriteLine($"\nSorting now by {_itemDao.SortOrder}!\n");
                    break;
                case 5:
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private void ListItems(string? criteria = null)
    {
        var items = new List<Item>();

        switch (criteria)
        {
            case "Search":
                var itemName = _inputManager.ReadString("\nEnter item name to find: ");
                items = _itemDao.GetAllItems(itemName);
                break;
            case "Type":
                var itemType = _inputManager.ReadString("\nWeapon, Armor, Valuable, or Consumable? ", ["weapon", "armor", "valuable", "consumable"]).ToLower();
                items = _itemDao.GetItemsByType(itemType);
                break;
            default:
                items = _itemDao.GetAllItems();
                break;
        }

        if (items == null || items.Count == 0)
        {
            _outputManager.WriteLine("\nNo items found.\n", ConsoleColor.Red);
            return;
        }

        _inputManager.PaginateList(items);
    }
}