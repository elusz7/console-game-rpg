using ConsoleGame.Helpers;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Attributes;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Characters.Monsters;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Services;

public class GameEngine
{
    private readonly GameContext _context;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;
    private readonly InventoryManager _inventoryManager;

    private IPlayer? _player;
    private IMonster? _goblin;

    public GameEngine(GameContext context, MenuManager menuManager, OutputManager outputManager, InventoryManager inventoryManager)
    {
        _menuManager = menuManager;
        _outputManager = outputManager;
        _inventoryManager = inventoryManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void GameLoop()
    {
        _outputManager.Clear();

        while (true)
        {
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Attack");
            _outputManager.WriteLine("2. Inventory Management");
            _outputManager.WriteLine("3. Quit");

            _outputManager.Display();

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    AttackCharacter();
                    break;
                case "2":
                    _inventoryManager.InventoryMenu(_player);
                    break;
                case "3":
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

    private void AttackCharacter()
    {
        if (_goblin == null || _player == null)
        {
            _outputManager.WriteLine("No characters available to attack.", ConsoleColor.Red);
            return;
        }

        if (_goblin is ITargetable targetableGoblin)
        {
            _player.Attack(targetableGoblin);
            if (((Goblin)_goblin).Health <= 0)
            {
                _goblin = null; // Remove the goblin from the game
                return;
            }
            _player.UseAbility(_player.Abilities.First(), targetableGoblin);
            if (((Goblin)_goblin).Health <= 0)
            {
                _goblin = null; // Remove the goblin from the game
                return;
            }
        }
        if (_player is ITargetable targetablePlayer)
        {
            _goblin.Attack(targetablePlayer);
        }

        
        if (((Player)_player).Health <= 0)
            _player = null; // Remove the player from the game 
    }


    private void SetupGame()
    {
        _player = _context.Players.OfType<Player>().FirstOrDefault();
        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause before starting the game loop
        Thread.Sleep(500);
        GameLoop();
    }

    private void LoadMonsters()
    {
        _goblin = _context.Monsters.OfType<Goblin>().FirstOrDefault();
    }

}
