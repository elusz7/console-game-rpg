using ConsoleGame.Helpers;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Attributes;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Characters.Monsters;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Services;

public class GameEngine(GameContext context, MenuManager menuManager, InputManager inputManager, OutputManager outputManager, CharacterManager characterManager, InventoryManager inventoryManager)
{
    private readonly GameContext _context = context;
    private readonly MenuManager _menuManager = menuManager;
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly CharacterManager _characterManager = characterManager;
    private readonly InventoryManager _inventoryManager = inventoryManager;

    private IPlayer? _player;
    private IMonster? _goblin;

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
            _outputManager.WriteLine("Main Menu:", ConsoleColor.Cyan);
            _outputManager.Write("1. Attack"
                + "\n2. Inventory Management"
                + "\n3. Character Management"
                + "\n4. Quit"
                + "\n\tSelect an option: ");
            _outputManager.Display();

            var input = _inputManager.ReadString();

            switch (input)
            {
                case "1":
                    AttackCharacter();
                    break;
                case "2":
                    _inventoryManager.InventoryMainMenu("Main Menu");
                    _outputManager.Clear();
                    break;
                case "3":
                    _characterManager.CharacterMainMenu();
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
