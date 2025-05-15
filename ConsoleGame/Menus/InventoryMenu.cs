using ConsoleGame.Helpers.CrudHelpers;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers;
using ConsoleGame.Managers.CrudHelpers;

namespace ConsoleGame.Menus;

public class InventoryMenu(InputManager inputManager, OutputManager outputManager, ItemDisplay itemDisplay, ItemManagement itemManagement, InventoryManagement inventoryManagement)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly ItemDisplay _itemDisplay = itemDisplay;
    private readonly ItemManagement _itemManagement = itemManagement;
    private readonly InventoryManagement _inventoryManagement = inventoryManagement;

    public void InventoryMainMenu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Inventory Main Menu ===", ConsoleColor.Cyan);
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