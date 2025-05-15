using ConsoleGame.Factories;
using ConsoleGame.GameDao;
using ConsoleGame.Helpers;
using ConsoleGame.Models;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGame.Helpers.AdventureEnums;
using ConsoleGame.Managers;

namespace ConsoleGame.Services;

public class AdventureService(OutputManager outputManager, InputManager inputManager, PlayerHelper playerHelper, 
    MapManager mapManager, CombatHelper combatHelper, MerchantHelper merchantHelper, 
    EquipmentHelper equipmentHelper, FloorFactory floorFactory, PlayerDao playerDao)
{
    private readonly OutputManager _outputManager = outputManager;
    private readonly InputManager _inputManager = inputManager;
    private readonly PlayerHelper _playerHelper = playerHelper;
    private readonly MapManager _mapManager = mapManager;
    private readonly CombatHelper _combatHelper = combatHelper;
    private readonly EquipmentHelper _equipmentHelper = equipmentHelper;
    private readonly MerchantHelper _merchantHelper = merchantHelper;
    private readonly FloorFactory _floorFactory = floorFactory;
    private readonly PlayerDao _playerDao = playerDao;
    private Player player;
    private Floor floor;

    public void SetUpAdventure()
    {
        _outputManager.Clear();
        _outputManager.WriteLine("Welcome to the Adventure!\n", ConsoleColor.Yellow);

        _outputManager.Display();

        try
        {
            player = _playerHelper.InitializePlayer(false) ?? throw new InvalidOperationException("No player selected.");
        }
        catch (InvalidOperationException ex)
        {
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            return;
        }
        
        _outputManager.WriteLine($"\nYou are adventuring with {player.Name}!", ConsoleColor.Green);
        _outputManager.WriteLine("\nNow generating a map...", ConsoleColor.DarkBlue);
        _outputManager.Display();

        floor = _floorFactory.CreateFloor(player.Level, false);
        player.CurrentRoom = floor.GetEntrance();        
    }
    public void Adventure()
    {
        _outputManager.Write($"Now adventuring on Floor {floor.Level}.", ConsoleColor.Yellow);
        while (true)
        {
            //if no monsters are left alive on the floor, exit the game
            if (!floor.Monsters.Any(m => m.CurrentHealth > 0))
            {
                _outputManager.WriteLine("You have cleared the floor of monsters. Congratulations!", ConsoleColor.Green);
                _outputManager.WriteLine("Press any key to exit this adventure.", ConsoleColor.Cyan);
                _outputManager.Display();
                _inputManager.ReadKey();
                _outputManager.Clear();
                return;
            }
            _outputManager.Clear();

            _outputManager.Write($"{floor.Monsters.Count} monsters remaining!\n|");
            foreach (var monster in floor.Monsters)
            {
                _outputManager.Write($" {monster.Room?.Name} |");
            }
            _outputManager.WriteLine("\n");
            _outputManager.Display();

            Move();
            bool roomChanged = false;

            while (!roomChanged)
            {
                if (!floor.Monsters.Any(m => m.CurrentHealth > 0))
                {
                    _outputManager.WriteLine("You have cleared the floor of monsters. Congratulations!", ConsoleColor.Green);
                    _outputManager.WriteLine("Press any key to exit this adventure.", ConsoleColor.Cyan);
                    _outputManager.Display();
                    _inputManager.ReadKey();
                    _outputManager.Clear();
                    return;
                }

                var options = GetMenuOptions();

                var index = 1;
                foreach (var option in options)
                {
                    _outputManager.WriteLine($"{index}. {option.Key}", ConsoleColor.Yellow);
                    index++;
                }

                var choice = options.ElementAt(_inputManager.ReadInt("\tSelect an option: ", options.Count) - 1);              

                switch (choice.Value)
                {
                    case AdventureOptions.Attack:
                        var monsters = player.CurrentRoom?.Monsters ?? [];

                        _combatHelper.CombatRunner(player, monsters);
                        break;
                    case AdventureOptions.Examine:
                        Examine();
                        break;
                    case AdventureOptions.Rest:
                        Rest();
                        break;
                    case AdventureOptions.Merchant:
                        _merchantHelper.Meeting(floor, player);
                        break;
                    case AdventureOptions.Move:
                        var direction = choice.Key.Split(' ').Last();
                        roomChanged = ChangeCurrentRoom(direction);
                        break;
                    case AdventureOptions.Equipment:
                        _equipmentHelper.ManageEquipment(player);
                        break;
                    case AdventureOptions.Quit:
                        bool quit = _inputManager.ConfirmAction("Quit Adventuring");
                        if (quit)
                        {
                            _outputManager.WriteLine("\nThanks for playing!\n", ConsoleColor.Yellow);
                            return;
                        }
                        break;
                    default:
                        break;
                }

                if (player.CurrentHealth <= 0)
                {
                    _outputManager.WriteLine("You have died! Adventure is now over.", ConsoleColor.Gray);
                    _outputManager.WriteLine("Press any key to continue...", ConsoleColor.Gray);
                    _outputManager.Display();
                    _inputManager.ReadKey();
                    _outputManager.Clear();
                    return;
                }
            }
        }
    }
    private void Move()
    {
        var room = player.CurrentRoom ?? throw new InvalidOperationException("Current room is null.");

        _mapManager.UpdateCurrentRoom(room);
        _mapManager.DisplayMap();

        _outputManager.WriteLine($"You now are in {room.Name}.", ConsoleColor.Cyan);
        _outputManager.WriteLine($"\t{room.Description}", ConsoleColor.DarkCyan);

        if (room == floor.GetEntrance())
        {
            _outputManager.WriteLine("\tThere is a water fountain with a patch of moss next to it on your right.", ConsoleColor.Cyan);
            _outputManager.WriteLine($"\t{floor.GetMerchantName()} is sitting across the fountain, examining something shiny.", ConsoleColor.Cyan);
        }

        if (room.Monsters != null && room.Monsters.Any(m => m.CurrentHealth > 0))
        {
            _outputManager.WriteLine("\nYou are not alone in the room. Prepare to fight. Or die.", ConsoleColor.Red);
        }

        _outputManager.WriteLine();
    }
    private void Examine()
    {
        var monsters = player.CurrentRoom?.Monsters?.Where(m => m.CurrentHealth <= 0).ToList() ?? [];

        if (monsters.Count == 0)
        {
            _outputManager.WriteLine("\nThere are no monsters to examine.\n", ConsoleColor.Yellow);
            return;
        }

        _outputManager.WriteLine();
        var selection = _inputManager.SelectFromList(monsters, m => m.Name, "Select a monster to examine");

        if (selection == null)
        {
            _outputManager.WriteLine("\nNo monster selected.", ConsoleColor.Red);
            return;
        }

        try
        {
            player.Loot(selection);

            foreach (var kvp in player.ActionItems)
            {
                _outputManager.WriteLine($"\n{kvp.Value}\n", ConsoleColor.Magenta);
            }

            player.ClearActionItems();
        }
        catch (TreasureException ex)
        {
            _outputManager.WriteLine($"\n{ex.Message}\n");
        }

        player.CurrentRoom?.Monsters.Remove(selection);
        floor.Monsters.Remove(selection);

        _outputManager.Display();
    }
    private void Rest()
    {
        _outputManager.WriteLine("\nYou lay down on the soft moss next to the gently bubbling fountain and fall asleep.", ConsoleColor.Gray);
        
        player.CurrentHealth = player.MaxHealth;

        _outputManager.WriteLine("\nYou wake up feeling refreshed and ready to take on the world!\n", ConsoleColor.Green);
    }
    private Dictionary<string, AdventureOptions> GetMenuOptions()
    {
        var options = new Dictionary<string, AdventureOptions>();

        var currentRoom = player.CurrentRoom ?? throw new InvalidOperationException("Current room is null.");
        var monsters = currentRoom.Monsters ?? [];
        var aliveMonsters = monsters.Where(m => m.CurrentHealth > 0).ToList();

        if (aliveMonsters.Count > 0)
        {
            AddMenuOption(options, "Fight Monsters", AdventureOptions.Attack);
        }
        else if (monsters.Count > 0)
        {
            AddMenuOption(options, "Examine Monsters", AdventureOptions.Examine);
        }

        if (!options.ContainsValue(AdventureOptions.Attack))
        {
            if (currentRoom == floor.GetEntrance())
            {
                AddMenuOption(options, "Rest", AdventureOptions.Rest);
                AddMenuOption(options, "Speak To Merchant", AdventureOptions.Merchant);
                AddMenuOption(options, "Manage Equipment", AdventureOptions.Equipment);
            }

            var directions = currentRoom.GetConnections() ?? [];
            foreach (var direction in directions)
            {
                AddMenuOption(options, $"Go {direction.Key}", AdventureOptions.Move);
            }
        }
        AddMenuOption(options, "Quit Adventuring", AdventureOptions.Quit);

        return options;
    }
    private static void AddMenuOption(Dictionary<string, AdventureOptions> options, string label, AdventureOptions option)
    {
        options.TryAdd(label, option);
    }
    private bool ChangeCurrentRoom(string direction)
    {
        var room = GetRoom(direction);

        CleanUpDeadMonsters();        

        if (player.CurrentRoom == floor.GetEntrance())
        {
            floor.UpdateMerchantItems();
        }
        
        player.CurrentRoom = room;
        return true;
    }
    private Room GetRoom(string direction)
    {
        var connections = player.CurrentRoom?.GetConnections();
        if (connections == null || !connections.ContainsKey(direction))
        {
            throw new InvalidOperationException($"No room found in direction: {direction}");
        }
        return connections[direction];
    }
    private void CleanUpDeadMonsters()
    {
        var deadMonsters = player.CurrentRoom?.Monsters?.Where(m => m.Treasure == null).ToList() ?? [];

        if (deadMonsters.Count == 0)
        {
            return;
        }

        foreach (var monster in deadMonsters)
        {
            player.CurrentRoom.Monsters.Remove(monster);
            floor.Monsters.Remove(monster);
        }
    }
}
