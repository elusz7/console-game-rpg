using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers;

public class EquipmentHelper(IInputManager inputManager, IOutputManager outputManager)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;

    public void ManageEquipment(Player player)
    {
        while (true)
        {
            DisplayEquipment(player);

            _outputManager.WriteLine("1. Equip an item");
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
            _outputManager.WriteLine("\nNo items currently equipped.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine($"\nCurrent Equipment [{player.Inventory.GetCarryingWeight()}/{player.Inventory.Capacity}]", ConsoleColor.Yellow);
        foreach (Item item in equipment)
        {
            _outputManager.WriteLine(ColorfulToStringHelper.ItemStatsString(item), ColorfulToStringHelper.GetItemColor(item));
        }
        _outputManager.WriteLine();
    }
    private void UnequipItem(Player player)
    {
        if (player.Equipment.Count == 0)
        {
            //_outputManager.WriteLine("\nNo items currently equipped.", ConsoleColor.Red);
            return;
        }

        var equippedItems = player.Equipment
            .OfType<Item>() //cast equipment to items
            .ToList();

        _outputManager.WriteLine();
        var unequippedItem = _inputManager.SelectItem("Select an item to unequip", equippedItems);

        if (unequippedItem != null)
        {
            player.Unequip(unequippedItem);
            OutputActionItems(player);
        }
    }
    public bool EquipItem(Player player, bool weaponsOnly = false)
    {
        var availableItems = player.Inventory.Items
                .Where(i => i is IEquippable e && !e.IsEquipped() && i.Durability > 0).ToList();

        if (weaponsOnly)
        {
            availableItems = [.. availableItems.Where(i => i is Weapon)];
        }

        if (availableItems.Count == 0)
        {
            _outputManager.WriteLine("\nNo items available for equipping.", ConsoleColor.Red);
            return false;
        }

        _outputManager.WriteLine();
        var selectedItem = _inputManager.SelectItem("Select an item to equip", availableItems);

        if (selectedItem == null)
        {
            return false;
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

            return false;
        }

        return true;
    }
    public bool UseConsumable(Player player)
    {
        var consumables = player.Inventory.Items.Where(i => i is Consumable).ToList();

        if (consumables.Count == 0)
        {
            _outputManager.WriteLine("No consumables available for use.", ConsoleColor.Red);
            return false;
        }

        _outputManager.WriteLine();
        var item = _inputManager.SelectItem("Select a consumable to use", consumables);

        if (item == null)
        {
            return false;
        }

        if (item is Consumable consumable)
        {
            switch (consumable.ConsumableType)
            {
                case ConsumableType.Durability:

                    var items = player.Inventory.Items.Where(i => i is IEquippable).ToList();

                    if (items.Count == 0)
                    {
                        _outputManager.WriteLine($"\nNo items available to use {consumable.Name} on.", ConsoleColor.Red);
                    }

                    _outputManager.WriteLine();
                    var target = _inputManager.SelectItem("Select an item to increase durability", items);

                    if (target == null)
                    {
                        return false;
                    }

                    consumable.UseOn(target);
                    break;
                default:
                    consumable.UseOn(player);
                    break;
            }
        }

        OutputActionItems(player);
        return true;
    }

    private void OutputActionItems(Player player)
    {
        _outputManager.WriteLine();
        foreach (var action in player.Logger.GetOrderedLog())
        {
            _outputManager.WriteLine(action.Value, ConsoleColor.Cyan);
        }

        player.Logger.Clear();

        _outputManager.Display();
    }
}
