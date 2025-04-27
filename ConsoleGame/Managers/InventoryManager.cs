using ConsoleGameEntities.Data;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Helpers;

public class InventoryManager
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly ItemDisplay _itemDisplay;
    private readonly ItemManagement _itemManagement;
    private readonly InventoryManagement _inventoryManagement;

    public InventoryManager(InputManager inputManager, OutputManager outputManager, ItemDisplay itemDisplay, ItemManagement itemManagement, InventoryManagement inventoryManagement)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _itemDisplay = itemDisplay;
        _itemManagement = itemManagement;
        _inventoryManagement = inventoryManagement;
    }

    public void InventoryMainMenu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Inventory Main Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. View Items"
                + "\n2. Manage Items"
                + "\n3. Manage Character's Inventory"
                + $"\n4. Return to Main Menu"
                + "\n\tChoose an option: ";

            var input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    _itemDisplay.Menu();
                    break;
                case "2":
                    _itemManagement.Menu();
                    break;
                case "3":
                    _inventoryManagement.Menu();
                    break;
                case "4":
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.\n");
                    break;
            }
        }
    } 
}