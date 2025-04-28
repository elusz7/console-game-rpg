using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Helpers;

public class InventoryManagement(InputManager inputManager, OutputManager outputManager, ItemDao itemDao, InventoryDao inventoryDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly InventoryDao _inventoryDao = inventoryDao;
    private Player _player = null;
    public void Menu(Player? player = null)
    {
        _outputManager.Clear();

        _player = player;
        if (_player == null)
        {
            List<Player> players = _inventoryDao.GetAllPlayers();
            for (int i = 0; i < players.Count; i++)
            {
                _outputManager.WriteLine($"{i + 1}. {players[i].Name}");
            }
            int index = _inputManager.ReadInt("\nSelect a player to manage their inventory: ", players.Count);
            _player = players[index - 1];
        }

        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine($"{_player.Name}'s Inventory Management", ConsoleColor.Cyan);
            string menuPrompt = "1. Add Item to Inventory"
                + "\n2. Remove Item from Inventory"
                + "\n3. Return to Previous Menu"
                + "\n\tChoose an option: ";
            var input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    AddItemToInventory();
                    break;
                case "2":
                    RemoveItemFromInventory();
                    break;
                case "3":
                    player = null;
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.WriteLine("\nInvalid option. Please try again.\n");
                    break;
            }
        }
    }
    private void AddItemToInventory()
    {
        var equippableItems = _inventoryDao.GetEquippableItems(_player);

        decimal currentCarryingWeight = _inventoryDao.GetInventoryWeight(_player);
        decimal weightAvailable = _player.Inventory.Capacity - currentCarryingWeight;

        _outputManager.WriteLine($"\nCapacity: {currentCarryingWeight} / {_player.Inventory.Capacity}");

        if (!equippableItems.Any())
        {
            _outputManager.WriteLine("No equippable items available.\n");
            return;
        }

        Item itemToAdd = SelectItem("\nSelect an item to add to inventory: ", equippableItems);

        string confirm = _inputManager.ReadString($"\nPlease confirm addition of {itemToAdd.Name} (y/n): ", new[] { "y", "n" });

        if (confirm == "n")
        {
            _outputManager.WriteLine($"\nItem {itemToAdd.Name} not added to inventory.", ConsoleColor.Red);
            return;
        }

        _player.Inventory.AddItem(itemToAdd);
        _inventoryDao.UpdateInventory(_player.Inventory);
        _outputManager.WriteLine($"\nItem {itemToAdd.Name} added to inventory.\n", ConsoleColor.Green);
    }
    private void RemoveItemFromInventory()
    {
        if (_player.Inventory.Items.Count == 0)
        {
            _outputManager.WriteLine("No items in inventory to remove.");
            return;
        }
        Item itemToRemove = SelectItem("\nSelect an item to remove from inventory: ", _player.Inventory.Items.ToList());

        string confirm = _inputManager.ReadString($"\nPlease confirm removal of {itemToRemove.Name} (y/n): ", new[] { "y", "n" });

        if (confirm == "n")
        {
            _outputManager.WriteLine("Item Removal Cancelled.");
            return;
        }

        _player.Inventory.RemoveItem(itemToRemove);
        _inventoryDao.UpdateInventory(_player.Inventory);

        _outputManager.WriteLine($"\nItem Successfully Removed From {_player.Name}'s Inventory\n", ConsoleColor.Green);
    }
    private Item SelectItem(string prompt, List<Item> itemList)
    {
        _outputManager.WriteLine();
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] is Weapon weapon)
                _outputManager.WriteLine($"{i + 1}. {GetItemDetails(itemList[i])}", ConsoleColor.DarkRed);
            else
                _outputManager.WriteLine($"{i + 1}. {GetItemDetails(itemList[i])}", ConsoleColor.DarkYellow);
        }
        int index = _inputManager.ReadInt(prompt, itemList.Count);
        return itemList[index - 1];
    }
    private string GetItemDetails(Item item)
    {
        string returnString = item.ToString();

        if (item is Weapon weapon)
            returnString = weapon.ToString();
        else if (item is Armor armor)
           returnString = armor.ToString();

        return returnString;
    }
}