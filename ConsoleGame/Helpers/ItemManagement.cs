using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Helpers;

public class ItemManagement(InputManager inputManager, OutputManager outputManager, ItemDao itemDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly ItemDao _itemDao = itemDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Item Management Menu", ConsoleColor.Cyan);
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
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
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
            "armor" => new Armor(name, value, description, durability, weight, defensePower)
        };

        _itemDao.AddItem(item);

        _outputManager.WriteLine($"\nItem {item.Name} successfully created.", ConsoleColor.Green);
    }
    private void EditItem()
    {
        _outputManager.WriteLine();

        string itemName = _inputManager.ReadString("\nEnter item name to edit: ");

        Item item = _itemDao.GetItemByName(itemName);


        if (item == null)
        {
            _outputManager.WriteLine($"\nItem {itemName} not found.", ConsoleColor.Red);

            List<Item> items = _itemDao.GetAllItems(itemName);

            item = SelectItem("\tSelect item to edit: ", items);
        }


        while (true)
        {
            _outputManager.WriteLine($"\nEditing item {item.Name}.", ConsoleColor.Cyan);
            DisplayItemDetails(item);

            string response = _inputManager.ReadString("\nWhat property would you like to edit? (exit to leave): ",
                new[] { "name", "description", "value", "durability", "weight", "attack power", "defense power", "exit" }).ToLower();

            if (response == "exit")
            {
                _itemDao.UpdateItem(item);
                _outputManager.WriteLine($"\nItem {item.Name} successfully updated.", ConsoleColor.Green);
                return;
            }

            UpdateItemProperty(item, response);
        }
    }
    private void RemoveItem()
    {
        _outputManager.WriteLine();
        Item itemToDelete = SelectItem("\tSelect an item to delete by number: ", _itemDao.GetAllItems());

        string confirm = _inputManager.ReadString($"\nPlease confirm deletion of {itemToDelete.Name} (y/n): ", ["y", "n"]);

        if (confirm == "y")
        {
            _itemDao.DeleteItem(itemToDelete);

            _outputManager.WriteLine("Item has been removed successfully!", ConsoleColor.Green);
        }
        else
        {
            _outputManager.WriteLine($"Deletion cancelled. Item [{itemToDelete.Name}] has not been removed.", ConsoleColor.Red);
        }
    }
    private Item SelectItem(string prompt, List<Item> itemList)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {itemList[i].Name}");
        }
        int index = _inputManager.ReadInt(prompt, itemList.Count);
        return itemList[index - 1];
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
}
