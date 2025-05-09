using ConsoleGame.Helpers;
using ConsoleGame.Helpers.CrudHelpers;
using ConsoleGame.Helpers.DisplayHelpers;

namespace ConsoleGame.Menus;

public class InventoryMenu
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly ItemDisplay _itemDisplay;
    private readonly ItemManagement _itemManagement;
    private readonly InventoryManagement _inventoryManagement;

    public InventoryMenu(InputManager inputManager, OutputManager outputManager, ItemDisplay itemDisplay, ItemManagement itemManagement, InventoryManagement inventoryManagement)
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
            _outputManager.WriteLine("1. View Items"
                + "\n2. Manage Items"
                + "\n3. Manage Character's Inventory"
                + $"\n4. Return to Admin Menu");

            var input = _inputManager.ReadMenuKey(4);

            switch (input)
            {
                case 1:
                    _itemDisplay.Menu();
                    break;
                case 2:
                    _itemManagement.Menu();
                    break;
                case 3:
                    _inventoryManagement.Menu();
                    break;
                case 4:
                    _outputManager.Clear();
                    return;
            }
        }
    } 
}