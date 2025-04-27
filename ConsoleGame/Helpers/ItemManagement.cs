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
                    DeleteItem();
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

        string name = _inputManager.ReadString("\nEnter item name: ");

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

        _outputManager.WriteLine($"\nItem {item.Name} successfully created.\n", ConsoleColor.Green);
    }
    private void EditItem()
    {
        Item item = SelectItem("\tSelect item to edit by number: ");

        if (item == null)
        {
            _outputManager.WriteLine($"\nItem Editing Cancelled.\n", ConsoleColor.Red);
            return;
        }

        string[] validResponses = item switch
        {
            Weapon => new[] { "Name", "Description", "Value", "Durability", "Weight", "Attack power" },
            Armor => new[] { "Name", "Description", "Value", "Durability", "Weight", "Defense power" }
        };

        while (true)
        {
            _outputManager.Clear();
            _outputManager.WriteLine($"Editing item {item.Name}.", ConsoleColor.Cyan);
            DisplayItemDetails(item);
            _outputManager.WriteLine();
            for (int i = 0; i < validResponses.Length; i++)
            {
                _outputManager.WriteLine($"{i + 1}. {validResponses[i]}");
            }
            _outputManager.WriteLine($"{validResponses.Length + 1}. Exit");

            int index = _inputManager.ReadInt("\nSelect property to edit by number: ", validResponses.Length + 1) - 1;

            if (index == validResponses.Length)
            {
                _itemDao.UpdateItem(item);
                _outputManager.WriteLine($"\nItem {item.Name} successfully updated.\n", ConsoleColor.Green);
                return;
            }

            UpdateItemProperty(item, validResponses[index]);
        }
    }
    private void DeleteItem()
    {
        Item itemToDelete = SelectItem("\tSelect an item to delete by number: ");

        string confirm = _inputManager.ReadString($"\nPlease confirm deletion of {itemToDelete.Name} (y/n): ", ["y", "n"]);

        if (confirm == "y")
        {
            _itemDao.DeleteItem(itemToDelete);

            _outputManager.WriteLine("\nItem has been deleted successfully!\n", ConsoleColor.Green);
        }
        else
        {
            _outputManager.WriteLine($"\nDeletion cancelled. Item [{itemToDelete.Name}] has not been deleted.\n", ConsoleColor.Red);
        }
    }
    private Item SelectItem(string prompt)
    {
        List<Item> itemList = _itemDao.GetAllItems();
        _outputManager.WriteLine();
        for (int i = 0; i < itemList.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {itemList[i].Name}");
        }
        int index = _inputManager.ReadInt(prompt, itemList.Count, true);

        if (index == -1)
            return null;

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
            case "Name":
                item.Name = _inputManager.ReadString("\nEnter new name: ");
                break;
            case "Description":
                item.Description = _inputManager.ReadString("\nEnter new description: ");
                break;
            case "Value":
                item.Value = _inputManager.ReadDecimal("\nEnter new value: ");
                break;
            case "Durability":
                item.Durability = _inputManager.ReadInt("\nEnter new durability: ");
                break;
            case "Weight":
                item.Weight = _inputManager.ReadDecimal("\nEnter new weight: ");
                break;
            case "Attack power":
                ((Weapon)item).AttackPower = _inputManager.ReadInt("\nEnter new attack power: ");
                break;
            case "Defense power":
                    ((Armor)item).DefensePower = _inputManager.ReadInt("\nEnter new defense power: ");
                break;
        }
    }
}
