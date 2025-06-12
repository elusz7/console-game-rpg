using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Helpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGame.Helpers.AdventureEnums;

namespace ConsoleGame.Services;

//modify so that you can stay on the floor after all the monsters are dead so you can loot them

public class AdventureService(IOutputManager outputManager, IInputManager inputManager, PlayerHelper playerHelper,
    IMapManager mapManager, CombatHelper combatHelper, MerchantHelper merchantHelper,
    EquipmentHelper equipmentHelper, LootHelper lootHelper, IFloorFactory floorFactory,
    CraftingHelper craftingHelper)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;
    private readonly PlayerHelper _playerHelper = playerHelper;
    private readonly IMapManager _mapManager = mapManager;
    private readonly CombatHelper _combatHelper = combatHelper;
    private readonly EquipmentHelper _equipmentHelper = equipmentHelper;
    private readonly MerchantHelper _merchantHelper = merchantHelper;
    private readonly LootHelper _lootHelper = lootHelper;
    private readonly IFloorFactory _floorFactory = floorFactory;
    private readonly CraftingHelper _craftingHelper = craftingHelper;
    private bool IsCampaign;
    private bool floorCleared = false;
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

            bool moveToNextFloor = false;

            while (!moveToNextFloor)
            {
                CheckFloorCompletion();

                Move();
                bool roomChanged = false;

                while (!roomChanged)
                {
                    CheckFloorCompletion();

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

                            if (player.CurrentHealth <= 0)
                                break;

                            CleanUpDeadMonsters();

                            CheckFloorCompletion();

                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
                            _outputManager.WriteLine("\nYou have defeated all the monsters in the room!", ConsoleColor.Green);
                            _outputManager.WriteLine("You are now free to continue exploring.", ConsoleColor.Cyan);

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
                        case AdventureOptions.Crafting:
                            _outputManager.Clear();
                            _craftingHelper.CraftingTable(player);
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
                            break;
                        case AdventureOptions.NextFloor:
                            _outputManager.WriteLine("\nSetting up next floor...", ConsoleColor.DarkGray);
                            _outputManager.Display();

                            SetUpNextFloor();
                            floorCleared = false; // reset for the new floor
                            moveToNextFloor = true;

                            if (!IsCampaign) _outputManager.WriteLine("\n");

                            _outputManager.Write("Press any key to continue...", ConsoleColor.DarkCyan);
                            _outputManager.Display();
                            _inputManager.ReadKey();
                            roomChanged = true; // reset inner loop
                            break;
                        case AdventureOptions.SameFloor:
                            _outputManager.WriteLine("\nResetting the floor...", ConsoleColor.DarkGray);
                            _outputManager.Display();

                            ResetFloor();
                            floorCleared = false; // reset for the new floor
                            moveToNextFloor = false;

                            if (!IsCampaign) _outputManager.WriteLine("\n");

                            _outputManager.Write("Press any key to continue...", ConsoleColor.DarkCyan);
                            _outputManager.Display();
                            _inputManager.ReadKey();
                            roomChanged = true; // reset inner loop
                            _outputManager.Clear();
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
        }
    }
    private void CheckFloorCompletion()
    {
        if (!floorCleared && !floor.Monsters.Any(m => m.CurrentHealth > 0))
        {
            floorCleared = true;
            _outputManager.WriteLine("You have cleared the floor of monsters. Congratulations!", ConsoleColor.Green);
            _outputManager.WriteLine("You can now descend to the next floor whenever you're ready.", ConsoleColor.Cyan);
            _outputManager.Display();
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
            _outputManager.WriteLine("\tIn the far corner, there is a stone table etched with strange patterns.", ConsoleColor.Cyan);
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
        var monsters = player.CurrentRoom?.Monsters?.Where(m => !m.Looted || m.Treasure != null).ToList() ?? [];

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

        _lootHelper.LootMonster(selection, player);

        if (selection.Looted && selection.Treasure == null)
        {
            player.CurrentRoom?.Monsters.Remove(selection);
            floor.Monsters.Remove(selection);
        }

        _outputManager.Display();
    }
    private void Rest()
    {
        _outputManager.WriteLine("\nYou lay down on the soft moss next to the gently bubbling fountain and fall asleep.", ConsoleColor.Gray);

        player.MaxHeal();

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
                AddMenuOption(options, "Use Crafting Table", AdventureOptions.Crafting);
            }

            var directions = currentRoom.GetConnections() ?? [];
            foreach (var direction in directions)
            {
                AddMenuOption(options, $"Go {direction.Key}", AdventureOptions.Move);
            }
        }

        if (floorCleared)
        {
            AddMenuOption(options, "Descend to the Next Floor", AdventureOptions.NextFloor);
            AddMenuOption(options, "Challenge Floor Again", AdventureOptions.SameFloor);
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
        if (player.CurrentRoom == null) throw new InvalidDataException();
        if (player.CurrentRoom.Monsters.Count == 0) return;

        var deadMonsters = player.CurrentRoom.Monsters.Where(m => m.Looted && m.Treasure == null).ToList();
        if (deadMonsters.Count == 0) return;

        player.CurrentRoom.Monsters.RemoveAll(m => deadMonsters.Contains(m));
        floor.Monsters.RemoveAll(m => deadMonsters.Contains(m));
    }

    private void SetUpNextFloor()
    {
        player.LevelUp();
        bool merchant = floor.HasMetMerchant;

        var map = 1;
        if (!IsCampaign)
        {
            map = GetAdventureMapOption();
        }

        floor = _floorFactory.CreateFloor(player.Level, IsCampaign, map == 2);
        floor.HasMetMerchant = merchant;
        player.CurrentRoom = floor.GetEntrance();
    }

    private void ResetFloor()
    {
        bool merchant = floor.HasMetMerchant;

        var map = 1;
        if (!IsCampaign)
        {
            map = GetAdventureMapOption();
        }

        floor = _floorFactory.CreateFloor(player.Level, IsCampaign, map == 2);
        floor.HasMetMerchant = merchant;
        player.CurrentRoom = floor.GetEntrance();
    }
}
