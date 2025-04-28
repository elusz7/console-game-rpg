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
                    ListAllItems();
                    break;
                case 2:
                    SearchItemByName();
                    break;
                case 3:
                    ListItemsByType();
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
    private void ListAllItems()
    {
        var items = _itemDao.GetAllItems();
        
        if (items == null || items.Count == 0)
        {
            _outputManager.WriteLine("No items found.");
            return;
        }

        _inputManager.PaginateList(items, i => i.ToString());
    }

    private void SearchItemByName()
    {
        string itemName = _inputManager.ReadString("\nEnter item name to find: ");

        var itemsFound = _itemDao.GetAllItems(itemName);

        if (!itemsFound.Any())
        {
            _outputManager.WriteLine($"\n\tNo items found matching [{itemName}]\n");
        }
        else
        {
            _outputManager.WriteLine($"\n\t{itemsFound.Count} items found matching [{itemName}]");
            _inputManager.PaginateList(itemsFound, i => i.ToString());
        }
    }
    private void ListItemsByType()
    {
        string category = _inputManager.ReadString("\nWeapon or Armor? ", new[] {"weapon", "armor"}).ToLower();

        List<Item> items = _itemDao.GetItemsByType(category);

        if (items.Count != 0)
        {
            _inputManager.PaginateList(items, i => i.ToString());
        }
        else
        {
            _outputManager.WriteLine("No items found.");
            _outputManager.Display();
        }
    }
}