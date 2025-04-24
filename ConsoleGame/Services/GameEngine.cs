using ConsoleGame.Helpers;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConsoleGame.Services;

public class GameEngine(GameContext context, StartMenuManager menuManager, InputManager inputManager, OutputManager outputManager, CharacterManager characterManager, InventoryManager inventoryManager, RoomManager roomManager, MonsterManager monsterManager)
{
    private readonly GameContext _context = context;
    private readonly StartMenuManager _menuManager = menuManager;
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly CharacterManager _characterManager = characterManager;
    private readonly InventoryManager _inventoryManager = inventoryManager;
    private readonly RoomManager _roomManager = roomManager;
    private readonly MonsterManager _monsterManager = monsterManager;

    private IPlayer? _player;
    private IMonster? _goblin;

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
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
            string menuPrompt = "1. Character Management"
                + "\n2. Inventory Management"
                + "\n3. Room Management"
                + "\n4. Quit"
                + "\n\tSelect an option: ";

            var input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    _characterManager.CharacterMainMenu();
                    _outputManager.Clear();
                    break;
                case "2":
                    _inventoryManager.InventoryMainMenu("Main Menu");
                    _outputManager.Clear();
                    break;
                case "3":
                    _roomManager.RoomMainMenu();
                    _outputManager.Clear();
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
