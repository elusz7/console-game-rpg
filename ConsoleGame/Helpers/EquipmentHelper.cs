using ConsoleGame.Managers;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers;

public class EquipmentHelper(InputManager inputManager, OutputManager outputManager)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;

    public void ManageEquipment(Player player)
    {
        while (true)
        {
            DisplayEquipment(player);

            _outputManager.WriteLine("\n1. Equip an item");
            _outputManager.WriteLine("2. Unequip an item");
            _outputManager.WriteLine("3. Use a consumable");
            _outputManager.WriteLine("4. Exit");

            var choice = _inputManager.ReadInt("\tChoose an option: ", 4);

            switch (choice)
            {
                case 1:
                    EquipItem(player);
                    break;
                case 2:
                    UnequipItem(player);
                    break;
                case 3:
                    UseConsumable(player);
                    break;
                case 4:
                    _outputManager.WriteLine();
                    return; // Exit the equipment management
            }
        }
    }
    private void DisplayEquipment(Player player)
    {
        var equipment = player.Equipment;

        if (equipment.Count == 0)
        {
            _outputManager.WriteLine("\nNo items currently equipped.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine("\nCurrent Equipment: ");
        foreach (var item in equipment)
        {
            _outputManager.WriteLine($"{item.Name}{GetArmorType(item)} [DUR: {item.Durability}{GetItemStats(item)}]", GetItemColor(item));
        }
    }
    private void UnequipItem(Player player)
    {
        if (player.Equipment.Count == 0)
        {
            _outputManager.WriteLine("\nNo items currently equipped.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var unequippedItem = SelectItem("Select an item to unequip: ", player.Equipment);
        
        if (unequippedItem == null)
        {
            _outputManager.WriteLine("\nNo item selected.\n");
        }
        else
        {
            player.Unequip(unequippedItem);
            OutputActionItems(player);
        }
    }
    private void EquipItem(Player player)
    {
        var availableItems = player.Inventory.Items
            .Where(i => i is Weapon || i is Armor)
            .Where(i => !i.IsEquipped())
            .ToList();

        if (availableItems.Count == 0)
        {
            _outputManager.WriteLine("\nNo items available for equipping.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var selectedItem = SelectItem("Select an item to equip: ", availableItems);

        if (selectedItem == null)
        {
            _outputManager.WriteLine("\nNo item selected.\n", ConsoleColor.Red);
            return;
        }
        
        try
        {
            player.Equip(selectedItem);
            OutputActionItems(player);
        }
        catch (EquipmentException ex)
        {
            _outputManager.WriteLine("\nAn error occurred while equipping the item!", ConsoleColor.Red);
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            _outputManager.WriteLine();
        }
    }
    private void UseConsumable(IPlayer player)
    {
        var consumables = player.Inventory.Items.Where(i => i is Consumable).ToList();

        if (consumables.Count == 0)
        {
            _outputManager.WriteLine("\nNo consumables available for use.");
            return;
        }

        _outputManager.WriteLine();
        var item = SelectItem("Select a consumable to use:", consumables);

        if (item == null)
        {
            _outputManager.WriteLine("\nConsumable selection cancelled.");
        }
        else if (item is Consumable consumable)
        {
            switch (consumable.ConsumableType)
            {
                case ConsumableType.Durability:
                    _outputManager.WriteLine();
                    var target = SelectItem("Select an item to increase durability", [.. player.Inventory.Items.Where(i => i is Armor || i is Weapon)]);

                    if (target == null)
                    {
                        _outputManager.WriteLine("\nItem selection cancelled.");
                        return;
                    }

                    consumable.UseOn(target);
                    break;
                default:
                    consumable.UseOn(player);
                    break;
            }
        }
    }
    private Item? SelectItem(string prompt, List<Item> items)
    {
        return _inputManager.SelectFromList(
            items,
            i => $"{i.Name}{GetArmorType(i)} [DUR: {i.Durability}{GetItemStats(i)}]",
            prompt,
            i => GetItemColor(i)
        );
    }
    private static string GetItemStats(IItem item)
    {
        return item switch
        {
            Weapon weapon => $" | ATK: {weapon.AttackPower}",
            Armor armor => $" | DEF: {armor.DefensePower} | RES: {armor.Resistance}",
            Consumable consumable => $" | POW: {consumable.Power} | AFF: {consumable.ConsumableType}",
            _ => ""
        };
    }
    private static string GetArmorType(IItem item)
    {
        return item switch
        {
            Armor armor => $" ({armor.ArmorType})",
            _ => ""
        };
    }
    private static ConsoleColor GetItemColor(IItem item)
    {
        return item switch
        {
            Weapon => ConsoleColor.Red,
            Armor => ConsoleColor.Green,
            Consumable => ConsoleColor.Blue,
            _ => ConsoleColor.Yellow
        };
    }
    private void OutputActionItems(Player player)
    {
        // SortedList auto-orders by key and avoids Dictionary's duplicate key issue
        var actions = new SortedList<long, string>();

        // Add player actions
        foreach (var kvp in player.ActionItems)
        {
            long key = kvp.Key;
            while (actions.ContainsKey(key)) key++; // Avoid duplicate keys
            actions.Add(key, kvp.Value);
        }

        // Output ordered actions
        _outputManager.WriteLine();
        foreach (var action in actions)
        {
            _outputManager.WriteLine(action.Value, ConsoleColor.Cyan);
        }

        player.ClearActionItems();

        _outputManager.Display();
    }
}
