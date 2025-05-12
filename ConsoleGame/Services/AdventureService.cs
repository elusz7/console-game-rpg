

using ConsoleGame.Factories;
using ConsoleGame.GameDao;
using ConsoleGame.Helpers;
using ConsoleGame.Models;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Skills;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGame.Helpers.AdventureEnums;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGame.Managers;

namespace ConsoleGame.Services;

public class AdventureService(OutputManager outputManager, InputManager inputManager, 
    MapManager mapManager, CombatHelper combatHelper, MerchantHelper merchantHelper, 
    EquipmentHelper equipmentHelper, FloorFactory floorFactory, PlayerDao playerDao)
{
    private readonly OutputManager _outputManager = outputManager;
    private readonly InputManager _inputManager = inputManager;
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
        _outputManager.WriteLine("Welcome to the Adventure!", ConsoleColor.Yellow);
        _outputManager.WriteLine("Let's start by selecting a character!", ConsoleColor.Cyan);

        try
        {
            SelectCharacter();
        }
        catch (InvalidOperationException ex)
        {
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            return;
        }
        
        _outputManager.WriteLine($"You are now adventuring with {player.Name}!", ConsoleColor.Green);


        floor = _floorFactory.CreateFloor(player.Level, false);
        player.CurrentRoom = floor.GetEntrance();

        
        
    }
    public void Adventure()
    {
        _outputManager.WriteLine($"Now adventuring on Floor {floor.Level}.", ConsoleColor.Yellow);
        while (true)
        {
            //if no monsters are left alive on the floor, exit the game
            if (!floor.Monsters.Any(m => m.CurrentHealth > 0))
            {
                _outputManager.WriteLine("You have cleared the floor of monsters. Congratulations!", ConsoleColor.Green);
                _outputManager.WriteLine("Press any key to exit this adventure.", ConsoleColor.Cyan);
                _inputManager.ReadKey();
                return;
            }

            Move();
            bool roomChanged = false;
            
            while (!roomChanged)
            {
                var options = GetMenuOptions();

                var index = 1;
                foreach (var option in options)
                {
                    _outputManager.WriteLine($"{index}. {option.Value}", ConsoleColor.Cyan);
                    index++;
                }

                var choice = options.ElementAt(_inputManager.ReadInt("Select an option", options.Count) - 1);              

                switch (choice.Value)
                {
                    case AdventureOptions.Attack:
                        var monsters = player.CurrentRoom?.Monsters ?? [];

                        _combatHelper.CombatRunner(player, monsters);

                        if (player.CurrentHealth <= 0) return;

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
                            _outputManager.WriteLine("Thanks for playing!", ConsoleColor.Yellow);
                            return;
                        }
                        break;
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
        _outputManager.WriteLine($"\n\t{room.Description}", ConsoleColor.DarkCyan);

        if (room == floor.GetEntrance())
        {
            _outputManager.WriteLine("\nThere is a water fountain with a patch of moss next to it on your right.", ConsoleColor.Cyan);
            _outputManager.WriteLine($"A {floor.GetMerchantName()} is sitting across the fountain, looking at you.", ConsoleColor.Cyan);
        }

        if (room.Monsters != null && room.Monsters.Count > 0)
        {
            _outputManager.WriteLine("\nYou are not alone in the room. Prepare to fight. Or die.", ConsoleColor.Red);
        }
    }
    private void Examine()
    {
        var monsters = player.CurrentRoom?.Monsters?.Where(m => m.CurrentHealth <= 0).ToList() ?? [];

        if (monsters.Count == 0)
        {
            _outputManager.WriteLine("There are no monsters to examine.", ConsoleColor.Yellow);
            return;
        }

        var selection = _inputManager.PaginateNames(monsters, m => m.Name, "Monster", "To Examine", true, false) ?? throw new InvalidOperationException("No monster selected.");
        try
        {
            player.Loot(selection);

            foreach (var kvp in player.ActionItems)
            {
                _outputManager.WriteLine(kvp.Value, ConsoleColor.Magenta);
            }

            player.ClearActionItems();
        }
        catch (TreasureException ex)
        {
            _outputManager.WriteLine(ex.Message);
        }

        _outputManager.Display();
    }
    private void Rest()
    {
        _outputManager.WriteLine("\nYou lay down on the soft moss next to the gently bubbling fountain and fall asleep.", ConsoleColor.Cyan);
        
        player.CurrentHealth = player.MaxHealth;

        _outputManager.WriteLine("\nYou wake up feeling refreshed and ready to take on the world!", ConsoleColor.Green);
    }
    private void SelectCharacter()
    {
        var availableCharacters = _playerDao.GetAllPlayers();

        if (availableCharacters.Count == 0)
        {
            throw new InvalidOperationException("No characters available for adventuring.");
        }

        var selection = _inputManager.PaginateNames(availableCharacters, p => p.Name, "Character", "To Adventure With", true, false) ?? throw new InvalidOperationException("No character selected.");

        player = selection;
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
                AddMenuOption(options, "Speak to merchant", AdventureOptions.Merchant);
                AddMenuOption(options, "Manage Equipment", AdventureOptions.Equipment);
                AddMenuOption(options, "Quit Adventuring", AdventureOptions.Quit);
            }

            var directions = currentRoom.GetConnections() ?? [];
            foreach (var direction in directions)
            {
                AddMenuOption(options, $"Go {direction}", AdventureOptions.Move);
            }
        }

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
        foreach (var monster in player.CurrentRoom?.Monsters?.Where(m => m.Treasure != null) ?? [])
        {
            player.CurrentRoom?.Monsters?.Remove(monster);
            floor.Monsters.Remove(monster);
        }
    }
}
