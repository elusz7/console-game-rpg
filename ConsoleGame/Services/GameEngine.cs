using ConsoleGame.Helpers;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConsoleGame.Services;

public class GameEngine
{
    private readonly StartMenuManager _menuManager;
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly PlayerManager _playerManager;
    private readonly InventoryManager _inventoryManager;
    private readonly RoomManager _roomManager;
    private readonly MonsterManager _monsterManager;

    public GameEngine(
        StartMenuManager menuManager, InputManager inputManager, OutputManager outputManager,
        PlayerManager playerManager, InventoryManager inventoryManager, RoomManager roomManager,
        MonsterManager monsterManager
    )
    {
        _menuManager = menuManager;
        _inputManager = inputManager;
        _outputManager = outputManager;
        _playerManager = playerManager;
        _inventoryManager = inventoryManager;
        _roomManager = roomManager;
        _monsterManager = monsterManager;
    }

    public void Run()
    {
        if (_menuManager.ShowStartMenu())
        {
            GameLoop();
        }
    }
    private void GameLoop()
    {
        _outputManager.Clear();

        while (true)
        {
            _outputManager.WriteLine("Main Menu:", ConsoleColor.Cyan);
            string menuPrompt = "1. Player Management"
                + "\n2. Inventory Management"
                + "\n3. Room Management"
                + "\n4. Quit"
                + "\n\tSelect an option: ";

            var input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    _playerManager.PlayerMainMenu();
                    break;
                case "2":
                    _inventoryManager.InventoryMainMenu();
                    break;
                case "3":
                    _roomManager.RoomMainMenu();
                    break;
                case "4":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please try again.", ConsoleColor.Red);
                    break;
            }
        }
    }
}
