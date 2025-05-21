using ConsoleGame.Factories;
using ConsoleGame.GameDao;
using ConsoleGame.Helpers;
using ConsoleGame.Models;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using static ConsoleGame.Helpers.AdventureEnums;
using ConsoleGame.Managers;

namespace ConsoleGame.Services;

public class AdventureService(OutputManager outputManager, InputManager inputManager, PlayerHelper playerHelper, 
    MapManager mapManager, CombatHelper combatHelper, MerchantHelper merchantHelper, 
    EquipmentHelper equipmentHelper, FloorFactory floorFactory)
{
    private readonly OutputManager _outputManager = outputManager;
    private readonly InputManager _inputManager = inputManager;
    private readonly PlayerHelper _playerHelper = playerHelper;
    private readonly MapManager _mapManager = mapManager;
    private readonly CombatHelper _combatHelper = combatHelper;
    private readonly EquipmentHelper _equipmentHelper = equipmentHelper;
    private readonly MerchantHelper _merchantHelper = merchantHelper;
    private readonly FloorFactory _floorFactory = floorFactory;
    private bool IsCampaign;
    private Player player;
    private Floor floor;

    private bool SetUp(bool isCampaign)
    {
        IsCampaign = isCampaign;

        _outputManager.Clear();
        if (IsCampaign)
            _outputManager.WriteLine("Welcome to the Campaign!\n", ConsoleColor.Magenta);
        else
            _outputManager.WriteLine("Welcome to the Adventure!\n", ConsoleColor.Yellow);

        _outputManager.Display();

        try
        {
            player = _playerHelper.InitializePlayer(IsCampaign) ?? throw new InvalidOperationException("No player selected.");
        }
        catch (InvalidOperationException ex)
        {
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            return false;
        }
        
        if (IsCampaign)
            _outputManager.WriteLine($"\nYou are starting a campaign with {player.Name}!", ConsoleColor.Green);
        else
            _outputManager.WriteLine($"\nYou are adventuring with {player.Name}!", ConsoleColor.Green);

        _outputManager.WriteLine("\nNow generating a map...", ConsoleColor.DarkBlue);
        _outputManager.Display();

        var map = 1;
        if (!IsCampaign)
        {
            map = _inputManager.ReadInt("\n1. Use map from database\n2. Generate Random Map\n\tSelect an option: ", 2);
        }

        floor = _floorFactory.CreateFloor(player.Level, IsCampaign, map == 2);
        player.CurrentRoom = floor.GetEntrance();

        return true;
    }
    public bool SetUpAdventure() => SetUp(false);
    public bool SetUpCampaign() => SetUp(true);
    public void Adventure()
    {
        while (true)
        {
            _outputManager.Clear();
            _outputManager.Write($"Now adventuring on Floor {floor.Level}.\n\n", ConsoleColor.Yellow);            

            bool floorCompleted = false;

            while (!floorCompleted)
            {
                floorCompleted = CheckFloorCompletion();
                if (floorCompleted)
                {
                    break;
                }

                Move();
                bool roomChanged = false;

                while (!roomChanged)
                {
                    floorCompleted = CheckFloorCompletion();
                    if (floorCompleted)
                    {
                        break;
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
                            _outputManager.Clear();
                            break;
                        case AdventureOptions.Equipment:
                            _equipmentHelper.ManageEquipment(player);
                            break;
                        case AdventureOptions.Quit:
                            var prompt = IsCampaign ? "Quit Campaign" : "Quit Adventure";
                            bool quit = _inputManager.ConfirmAction(prompt);
                            if (quit)
                            {
                                _outputManager.WriteLine("\nThanks for playing!\n", ConsoleColor.Yellow);
                                if (IsCampaign)
                                    _playerHelper.SavePlayer(player);
                                return;
                            }
                            break;
                    }

                    if (player.CurrentHealth <= 0)
                    {
                        _outputManager.WriteLine("\nYou have died! Adventure is now over.", ConsoleColor.Red);
                        _outputManager.WriteLine("Press any key to continue...", ConsoleColor.Gray);
                        _outputManager.Display();
                        _playerHelper.SavePlayer(player);
                        InputManager.ReadKey();
                        _outputManager.Clear();
                        return;
                    }
                }
            }
            
            _outputManager.Write("Setting up next floor...", ConsoleColor.DarkGray);
            _outputManager.Display();

            SetUpNextFloor();

            _outputManager.Write("Press any key to continue...", ConsoleColor.DarkCyan);
            _outputManager.Display();
            InputManager.ReadKey();
        }
    }
    private bool CheckFloorCompletion()
    {
        if (!floor.Monsters.Any(m => m.CurrentHealth > 0))
        {
            _outputManager.WriteLine("You have cleared the floor of monsters. Congratulations!", ConsoleColor.Green);

            _outputManager.Write("Press any key to continue!");
            _outputManager.Display();
            InputManager.ReadKey();
            _outputManager.Clear();
            return true;
        }
        return false;
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
        var selection = _inputManager.Selector(monsters, m => m.Name, "Select a monster to examine");

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

        _outputManager.WriteLine("You wake up feeling refreshed and ready to take on the world!\n", ConsoleColor.Green);
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
        if (IsCampaign)
            AddMenuOption(options, "Quit Campaigning", AdventureOptions.Quit);
        else
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

        if (connections == null || !connections.TryGetValue(direction, out var room))
        {
            throw new InvalidOperationException($"No room found in direction: {direction}");
        }

        return room!;
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
            player.CurrentRoom!.Monsters.Remove(monster);
            floor.Monsters.Remove(monster);
        }
    }
    private void SetUpNextFloor()
    {
        player.LevelUp();
        bool merchant = floor.HasMetMerchant;

        var leftovers = floor.Monsters.Where(m => m.Treasure != null).ToList();
        var automaticLoot = new List<Item>();
        if (leftovers.Count > 0)
        {
            foreach (var monster in leftovers)
            {
                automaticLoot.Add(monster.Treasure!);
                monster.Treasure = null;
            }
        }

        if (automaticLoot.Count > 0)
        {
            var loopList = automaticLoot.ToList();
            foreach (var item in loopList)
            {
                try
                {
                    player.Inventory.AddItem(item);
                }
                catch (InventoryException)
                {
                    automaticLoot.Remove(item);
                }
            }
            _outputManager.WriteLine($"\n\nAutomatically looted: {string.Join(", ", automaticLoot.Select(a => a.Name))}\n", ConsoleColor.DarkGreen);
        }

        var map = 1;
        if (!IsCampaign)
        {
            map = _inputManager.ReadInt("\"1. Use map from database\\n2. Generate Random Map\n\tSelect an option: ", 2);
        }

        floor = _floorFactory.CreateFloor(player.Level, IsCampaign, map == 2);
        floor.HasMetMerchant = merchant;
        player.CurrentRoom = floor.GetEntrance();
    }
}
