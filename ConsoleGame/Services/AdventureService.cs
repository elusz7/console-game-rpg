using ConsoleGame.Helpers;
using ConsoleGame.Models;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using static ConsoleGame.Helpers.AdventureEnums;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Factories.Interfaces;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGame.Services;

public class AdventureService(IOutputManager outputManager, IInputManager inputManager, PlayerHelper playerHelper, 
    IMapManager mapManager, CombatHelper combatHelper, MerchantHelper merchantHelper, 
    EquipmentHelper equipmentHelper, IFloorFactory floorFactory)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;
    private readonly PlayerHelper _playerHelper = playerHelper;
    private readonly IMapManager _mapManager = mapManager;
    private readonly CombatHelper _combatHelper = combatHelper;
    private readonly EquipmentHelper _equipmentHelper = equipmentHelper;
    private readonly MerchantHelper _merchantHelper = merchantHelper;
    private readonly IFloorFactory _floorFactory = floorFactory;
    private bool IsCampaign;
    private Player player;
    private Floor floor;

    private bool SetUp(bool isCampaign)
    {
        IsCampaign = isCampaign;

        _outputManager.Clear();
        if (IsCampaign)
            _outputManager.WriteLine("Welcome to the Campaign!", ConsoleColor.Magenta);
        else
            _outputManager.WriteLine("Welcome to the Adventure!", ConsoleColor.Yellow);

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

        _outputManager.Display();

        var map = 1;
        if (!IsCampaign)
        {
            map = GetAdventureMapOption();
        }

        floor = _floorFactory.CreateFloor(player.Level, IsCampaign, map == 2);
        player.CurrentRoom = floor.GetEntrance();

        if (player.CurrentRoom != floor.GetEntrance())
            throw new InvalidDataException("Why is this throwing");

        return true;
    }
    public bool SetUpAdventure() => SetUp(false);
    public bool SetUpCampaign() => SetUp(true);
    private int GetAdventureMapOption()
    {
        _outputManager.Write("\n1. Use Map As Configured\n2. Generate Random Map");
        return _inputManager.ReadMenuKey(2);
    }
    public void Adventure()
    {
        while (true)
        {
            _outputManager.Clear();
            _outputManager.WriteLine($"Now adventuring on Floor {floor.Level}.\n", ConsoleColor.Yellow);            

            bool floorCompleted = false;

            while (!floorCompleted)
            {
                DebugRoomAndNeighbors();

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

                    _outputManager.WriteLine();
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
                            _outputManager.Clear();
                            var monsters = player.CurrentRoom?.Monsters ?? [];
                            _combatHelper.CombatRunner(player, monsters);
                            
                            CleanUpDeadMonsters();

                            if (player.CurrentHealth <= 0)
                                break;

                            if (!CheckFloorCompletion())
                            {
                                _outputManager.Clear();
                                _outputManager.WriteLine();
                                _mapManager.DisplayMap();
                                _outputManager.WriteLine("\nYou have defeated all the monsters in the room!", ConsoleColor.Green);
                                _outputManager.WriteLine("You are now free to continue exploring.", ConsoleColor.Cyan);                                
                            }

                            break;
                        case AdventureOptions.Examine:  
                            Examine();
                            break;
                        case AdventureOptions.Rest:
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
                            Rest();
                            break;
                        case AdventureOptions.Merchant:
                            _merchantHelper.Meeting(floor, player);
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
                            break;
                        case AdventureOptions.Move:
                            var direction = choice.Key.Split(' ').Last();
                            roomChanged = ChangeCurrentRoom(direction);
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            break;
                        case AdventureOptions.Equipment:
                            _equipmentHelper.ManageEquipment(player);
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
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
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
                            break;
                    }

                    if (player.CurrentHealth <= 0)
                    {
                        _outputManager.WriteLine("You have died! Adventure is now over.", ConsoleColor.Red);
                        _outputManager.WriteLine("Press any key to continue...", ConsoleColor.Gray);
                        _outputManager.Display();
                        _playerHelper.SavePlayer(player);
                        _inputManager.ReadKey();
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
            _inputManager.ReadKey();
        }
    }
    private bool CheckFloorCompletion()
    {
        if (!floor.Monsters.Any(m => m.CurrentHealth > 0))
        {
            _outputManager.WriteLine("You have cleared the floor of monsters. Congratulations!", ConsoleColor.Green);

            _outputManager.Write("Press any key to continue!");
            _outputManager.Display();
            _inputManager.ReadKey();
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
            if (!floor.HasMetMerchant)
            {
                _merchantHelper.Meeting(floor, player);
            }
        }

        if (room.Monsters.Count != 0 && room.Monsters.Any(m => m.CurrentHealth > 0))
        {
            _outputManager.WriteLine("\nYou are not alone in the room. Prepare to fight. Or die.", ConsoleColor.Red);
        }        
    }
    private void Examine()
    {
        var monsters = player.CurrentRoom?.Monsters?.Where(m => m.CurrentHealth <= 0).ToList() ?? [];

        if (monsters.Count == 0)
        {
            _outputManager.WriteLine("\nThere are no monsters to examine.", ConsoleColor.Yellow);
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

            _outputManager.WriteLine();
            foreach (var kvp in player.ActionItems)
            {
                _outputManager.WriteLine($"{kvp.Value}", ConsoleColor.Magenta);
            }
            player.ClearActionItems();
        }
        catch (TreasureException ex)
        {
            _outputManager.WriteLine($"\n{ex.Message}", ConsoleColor.Red);
        }

        player.CurrentRoom?.Monsters.Remove(selection);
        floor.Monsters.Remove(selection);

        _outputManager.Display();
    }
    private void Rest()
    {
        _outputManager.WriteLine("\nYou lay down on the soft moss next to the gently bubbling fountain and fall asleep.", ConsoleColor.Gray);
        
        player.CurrentHealth = player.MaxHealth;

        _outputManager.WriteLine("You wake up feeling refreshed and ready to take on the world!", ConsoleColor.Green);
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
        if (player.CurrentRoom?.Monsters == null) return;

        var deadMonsters = player.CurrentRoom.Monsters.Where(m => m.Treasure == null).ToList();
        if (deadMonsters.Count == 0) return;

        player.CurrentRoom.Monsters.RemoveAll(m => deadMonsters.Contains(m));
        floor.Monsters.RemoveAll(m => deadMonsters.Contains(m));
    }

    private void SetUpNextFloor()
    {
        player.LevelUp();
        bool merchant = floor.HasMetMerchant;

        AutomaticLoot([.. floor.Monsters.Where(m => m.Treasure != null)]);

        var map = 1;
        if (!IsCampaign)
        {
            map = GetAdventureMapOption();
        }

        floor = _floorFactory.CreateFloor(player.Level, IsCampaign, map == 2);
        floor.HasMetMerchant = merchant;
        player.CurrentRoom = floor.GetEntrance();
    }
    private void AutomaticLoot(List<Monster> monstersWithLoot)
    {
        var automaticLoot = new List<Item>();
        if (monstersWithLoot.Count > 0)
        {
            foreach (var monster in monstersWithLoot)
            {
                automaticLoot.Add(monster.Treasure!);
                monster.Treasure = null;
            }
        }

        var skippedItems = new List<Item>();
        foreach (var item in automaticLoot.ToList())
        {
            try
            {
                player.Inventory.AddItem(item);
            }
            catch (InventoryException)
            {
                skippedItems.Add(item);
            }
        }
        automaticLoot.RemoveAll(item => skippedItems.Contains(item));

        if (automaticLoot.Count > 0)
        {
            _outputManager.WriteLine($"Automatically looted: {string.Join(", ", automaticLoot.Select(a => a.Name))}", ConsoleColor.DarkGreen);
        }
    }
    private void DebugRoomAndNeighbors()
    {
        var currentRoom = player.CurrentRoom;

        if (currentRoom == null)
        {
            System.Diagnostics.Debug.WriteLine("Player's current room is null.");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[CURRENT ROOM] {currentRoom.Name}");
        if (currentRoom.Monsters.Any())
        {
            foreach (var monster in currentRoom.Monsters)
            {
                System.Diagnostics.Debug.WriteLine($"\tMonster: {monster.Name} (HP: {monster.CurrentHealth}) Loot: {monster.Treasure?.Name ?? "None"}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("\tNo monsters in this room.");
        }

        var directions = new[] { "North", "South", "East", "West" };

        foreach (var dir in directions)
        {
            if (currentRoom.GetConnections().TryGetValue(dir, out var neighbor))
            {
                System.Diagnostics.Debug.WriteLine($"[{dir.ToUpper()}] {neighbor.Name}");
                if (neighbor.Monsters.Any())
                {
                    foreach (var monster in neighbor.Monsters)
                    {
                        System.Diagnostics.Debug.WriteLine($"\tMonster: {monster.Name} (HP: {monster.CurrentHealth}) Loot: {monster.Treasure?.Name ?? "None"}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("\tNo monsters in this room.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[{dir.ToUpper()}] No room.");
            }
        }
    }

}
