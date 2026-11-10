using ConsoleGame.Helpers;
using ConsoleGame.Models;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGame.Helpers.AdventureEnums;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Menus;

namespace ConsoleGame.Services;

public class AdventureService(
    IOutputManager outputManager,
    IInputManager inputManager,
    PlayerHelper playerHelper,
    IMapManager mapManager,
    CombatHelper combatHelper,
    MerchantHelper merchantHelper,
    EquipmentHelper equipmentHelper,
    IFloorFactory floorFactory,
    FloorProgressionService floorProgressionService,
    RoomNavigationService roomNavigationService,
    LootService lootService,
    CampaignService campaignService,
    CraftMenu craftMenu)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;
    private readonly PlayerHelper _playerHelper = playerHelper;
    private readonly IMapManager _mapManager = mapManager;
    private readonly CombatHelper _combatHelper = combatHelper;
    private readonly EquipmentHelper _equipmentHelper = equipmentHelper;
    private readonly MerchantHelper _merchantHelper = merchantHelper;
    private readonly CraftMenu _craftMenu = craftMenu;

    private readonly IFloorFactory _floorFactory = floorFactory;

    // Handles leveling up, collecting leftover loot, and creating the next floor.
    private readonly FloorProgressionService _floorProgressionService = floorProgressionService;

    // Handles player movement and room-related monster cleanup.
    private readonly RoomNavigationService _roomNavigationService = roomNavigationService;

    // Handles monster treasure collection and cleanup.
    private readonly LootService _lootService = lootService;

    // Handles persistence of player upon quit or death
    private readonly CampaignService _campaignService = campaignService;

    // Holds the state for the current adventure or campaign.
    private GameSession? _session;

    // Provides access to the active session.
    // Throws a clear exception if gameplay is accessed before setup succeeds.
    private GameSession Session =>
        _session ?? throw new InvalidOperationException("No active game session.");


    private bool SetUp(bool isCampaign)
    {
        // Clear any previous session before starting a new one.
        _session = null;

        _outputManager.Clear();

        // Display the appropriate welcome message.
        if (isCampaign)
            _outputManager.WriteLine("Welcome to the Campaign!", ConsoleColor.Magenta);
        else
            _outputManager.WriteLine("Welcome to the Adventure!", ConsoleColor.Yellow);

        _outputManager.Display();

        // Initialize the player before creating the game session.
        Player player;

        try
        {
            player = _playerHelper.InitializePlayer(isCampaign)
                     ?? throw new InvalidOperationException("No player selected.");
        }
        catch (InvalidOperationException ex)
        {
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            return false;
        }

        // Introduce the selected player.
        if (isCampaign)
            _outputManager.WriteLine($"\nYou are starting a campaign with {player.Name}!", ConsoleColor.Green);
        else
            _outputManager.WriteLine($"\nYou are adventuring with {player.Name}!", ConsoleColor.Green);

        _outputManager.Display();

        // Let the player choose a map for adventure mode.
        var map = 1;
        if (!isCampaign)
        {
            map = GetAdventureMapOption();
        }

        // Create the first floor.
        var floor = _floorFactory.CreateFloor(player.Level, isCampaign, map == 2);

        // Place the player at the entrance.
        player.CurrentRoom = floor.GetEntrance();

        // Create the session after the player and floor are ready.
        _session = new GameSession(player, floor, isCampaign);

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
            _outputManager.WriteLine($"Now adventuring on Floor {Session.CurrentFloor.Level}.\n", ConsoleColor.Yellow);

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
                            var monsters = Session.Player.CurrentRoom?.Monsters ?? [];
                            _combatHelper.CombatRunner(Session.Player, monsters);

                            // Remove monsters that no longer have treasure after combat.
                            _roomNavigationService.CleanUpDeadMonsters(Session);

                            if (Session.Player.CurrentHealth <= 0)
                                break;

                            if (!CheckFloorCompletion())
                            {
                                _outputManager.Clear();
                                _outputManager.WriteLine();
                                _mapManager.DisplayMap();
                                _outputManager.WriteLine("\nYou have defeated all the monsters in the room!",
                                    ConsoleColor.Green);
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
                            _merchantHelper.Meeting(Session.CurrentFloor, Session.Player);
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
                            break;
                        case AdventureOptions.Move:
                            var direction = choice.Key.Split(' ').Last();
                            // Delegate room movement and state updates to RoomNavigationService.
                            _roomNavigationService.ChangeCurrentRoom(Session, direction);
                            roomChanged = true;
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            break;
                        case AdventureOptions.Equipment:
                            _equipmentHelper.ManageEquipment(Session.Player);
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
                            break;
                        case AdventureOptions.Craft:
                            _outputManager.Clear();
                            _craftMenu.CraftingTable(Session.Player.Id);
                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
                            break;
                        case AdventureOptions.Quit:
                            var prompt = Session.IsCampaign ? "Quit Campaign" : "Quit Adventure";
                            bool quit = _inputManager.ConfirmAction(prompt);
                            if (quit)
                            {
                                // Record that the player chose to end the session.
                                Session.End(AdventureStatus.Quit);

                                _outputManager.WriteLine("\nThanks for playing!\n", ConsoleColor.Yellow);
                                _campaignService.SaveIfCampaign(Session);
                                return;
                            }

                            _outputManager.Clear();
                            _outputManager.WriteLine();
                            _mapManager.DisplayMap();
                            break;
                    }

                    if (Session.Player.CurrentHealth <= 0)
                    {
                        // Record the session's end state after the player's death.
                        Session.End(AdventureStatus.PlayerDead);

                        _outputManager.WriteLine("You have died! Adventure is now over.", ConsoleColor.Red);
                        _outputManager.WriteLine("Press any key to continue...", ConsoleColor.Gray);
                        _outputManager.Display();

                        _campaignService.SaveIfCampaign(Session);

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

    // Checks whether the current floor has been cleared and handles the console feedback.
    private bool CheckFloorCompletion()
    {
        if (!Session.IsFloorCleared)
        {
            return false;
        }

        // Notify the player that the floor is cleared.
        _outputManager.WriteLine(
            "You have cleared the floor of monsters. Congratulations!",
            ConsoleColor.Green);

        _outputManager.Write("Press any key to continue!");
        _outputManager.Display();
        _inputManager.ReadKey();
        _outputManager.Clear();

        return true;
    }

    private void Move()
    {
        var room = Session.Player.CurrentRoom ?? throw new InvalidOperationException("Current room is null.");

        _mapManager.UpdateCurrentRoom(room);
        _mapManager.DisplayMap();

        _outputManager.WriteLine($"You now are in {room.Name}.", ConsoleColor.Cyan);
        _outputManager.WriteLine($"\t{room.Description}", ConsoleColor.DarkCyan);

        if (room == Session.CurrentFloor.GetEntrance())
        {
            _outputManager.WriteLine("\tThere is a water fountain with a patch of moss next to it on your right.",
                ConsoleColor.Cyan);
            _outputManager.WriteLine(
                $"\t{Session.CurrentFloor.GetMerchantName()} is sitting across the fountain, examining something shiny.",
                ConsoleColor.Cyan);
            if (!Session.CurrentFloor.HasMetMerchant)
            {
                _merchantHelper.Meeting(Session.CurrentFloor, Session.Player);
            }
        }

        var livingMonsters = room.Monsters
            .Where(m => m.CurrentHealth > 0)
            .ToList();

        if (livingMonsters.Count != 0)
        {
            var monsterNames = string.Join(", ", livingMonsters.Select(m => m.Name));

            _outputManager.WriteLine(
                $"\n{monsterNames} roar at you menacingly.\nPrepare to fight. Or die.",
                ConsoleColor.Red);
        }
    }

    private void Examine()
    {
        var monsters = Session.Player.CurrentRoom?.Monsters?.Where(m => m.CurrentHealth <= 0).ToList() ?? [];

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
            // Delegate treasure collection and monster cleanup.
            _lootService.LootMonster(Session, selection);

            // Display messages generated by the player's loot operation.
            _outputManager.WriteLine();

            foreach (var kvp in Session.Player.ActionItems)
            {
                _outputManager.WriteLine($"{kvp.Value}", ConsoleColor.Magenta);
            }

            Session.Player.ClearActionItems();
        }
        catch (TreasureException ex)
        {
            // Display any treasure or inventory errors.
            _outputManager.WriteLine($"\n{ex.Message}", ConsoleColor.Red);
        }

        _outputManager.Display();
    }

    private void Rest()
    {
        _outputManager.WriteLine(
            "\nYou lay down on the soft moss next to the gently bubbling fountain and fall asleep.", ConsoleColor.Gray);

        Session.Player.CurrentHealth = Session.Player.MaxHealth;

        _outputManager.WriteLine("You wake up feeling refreshed and ready to take on the world!", ConsoleColor.Green);
    }

    private Dictionary<string, AdventureOptions> GetMenuOptions()
    {
        var options = new Dictionary<string, AdventureOptions>();

        var currentRoom = Session.Player.CurrentRoom ?? throw new InvalidOperationException("Current room is null.");
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
            if (currentRoom == Session.CurrentFloor.GetEntrance())
            {
                AddMenuOption(options, "Rest", AdventureOptions.Rest);
                AddMenuOption(options, "Speak To Merchant", AdventureOptions.Merchant);
                AddMenuOption(options, "Manage Equipment", AdventureOptions.Equipment);
                AddMenuOption(options, "Craft Runes", AdventureOptions.Craft);
            }

            var directions = currentRoom.GetConnections() ?? [];
            foreach (var direction in directions)
            {
                AddMenuOption(options, $"Go {direction.Key}", AdventureOptions.Move);
            }
        }

        if (Session.IsCampaign)
            AddMenuOption(options, "Quit Campaigning", AdventureOptions.Quit);
        else
            AddMenuOption(options, "Quit Adventuring", AdventureOptions.Quit);

        return options;
    }

    private static void AddMenuOption(Dictionary<string, AdventureOptions> options, string label,
        AdventureOptions option)
    {
        options.TryAdd(label, option);
    }

    // Handles the console interaction for advancing to the next floor.
    // The actual gameplay state changes are delegated to FloorProgressionService.
    private void SetUpNextFloor()
    {
        // Adventure mode allows the player to choose whether to generate a random map.
        // Campaign mode always uses the configured map.
        var generateRandomMap = false;

        if (!Session.IsCampaign)
        {
            generateRandomMap = GetAdventureMapOption() == 2;
        }

        // Advance the session and receive the items successfully collected.
        var collectedItems = _floorProgressionService.AdvanceToNextFloor(
            Session,
            generateRandomMap);

        // Display the loot collected during floor progression.
        if (collectedItems.Count > 0)
        {
            _outputManager.WriteLine(
                $"\nAutomatically looted: {string.Join(", ", collectedItems.Select(item => item.Name))}",
                ConsoleColor.DarkGreen);
        }
    }
}