using ConsoleGame.GameDao;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers;
using ConsoleGame.Managers.CrudHelpers;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers.CrudHelpers;

public class PlayerManagement(InputManager inputManager, OutputManager outputManager, PlayerDao playerDao, ArchetypeDao archetypeDao, InventoryManagement inventoryManagement)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly PlayerDao _playerDao = playerDao;
    private readonly ArchetypeDao _archetypeDao = archetypeDao;
    private readonly InventoryManagement _inventoryManagement = inventoryManagement;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Player Management Menu", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Add Player"
                + "\n2. Edit Player"
                + "\n3. Remove Player"
                + "\n4. Return to Player Main Menu");

            var choice = _inputManager.ReadMenuKey(4);

            switch (choice)
            {
                case 1:
                    AddPlayer();
                    break;
                case 2:
                    EditPlayer();
                    break;
                case 3:
                    DeletePlayer();
                    break;
                case 4:
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private void AddPlayer()
    {
        do
        {
            CreatePlayer();

        } while (_inputManager.LoopAgain("create"));
        _outputManager.WriteLine();
    }

    private void CreatePlayer()
    {
        _outputManager.WriteLine();
        string name = _inputManager.ReadString("Enter player name: ");

        _outputManager.WriteLine();
        var archetype = _inputManager.Selector(
                    _archetypeDao.GetAllArchetypes(),
                    a => ColorfulToStringHelper.ArchetypeToString(a),
                    "Select archetype for new player",
                    b => ColorfulToStringHelper.GetArchetypeColor(b)
                );

        if (archetype == null)
        {
            _outputManager.WriteLine("\nNo archetype selected. Player creation cancelled.\n", ConsoleColor.Red);
            return;
        }

        int health = _inputManager.ReadInt("\nEnter player's health: ");

        int gold = _inputManager.ReadInt("Enter player's gold: ");

        decimal capacity = _inputManager.ReadDecimal("Enter player's weight carrying capacity: ");

        List<Item> items = [];

        Player newPlayer = new()
        {
            Name = name,
            ArchetypeId = archetype.Id,
            MaxHealth = health,
            Level = 1,
            Inventory = new Inventory
            {
                Gold = gold,
                Capacity = capacity,
                Items = items
            }
        };

        _playerDao.AddPlayer(newPlayer);

        string assignItems = _inputManager.ReadString($"\nWould you like to assign {name} some starting items? (y/n) ", ["y", "n"]).ToLower();
        if (assignItems == "y")
            _inventoryManagement.Menu(newPlayer);
        else
            _outputManager.WriteLine("No items assigned. Items can be added later through the main menu.\n");

        _outputManager.WriteLine($"New Player [{name}] added successfully!\n", ConsoleColor.Green);
    }
    private void EditPlayer()
    {
        var players = _playerDao.GetAllPlayers();

        if (players.Count == 0)
        {
            _outputManager.WriteLine("\nNo players available for editing.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine();
        var player = _inputManager.Selector(
            players,
            p => ColorfulToStringHelper.PlayerToString(p),
            "Select a player to edit",
            b => ColorfulToStringHelper.GetArchetypeColor(b.Archetype)
        );

        if (player == null)
        {
            _outputManager.WriteLine("No player selected for editing.\n", ConsoleColor.Red);
            return;
        }

        var propertyActions = new Dictionary<string, Action>
        {
            { "Name", () => player.Name = _inputManager.ReadString("\nEnter new value for Name: ") },
            { "Health", () => player.MaxHealth = _inputManager.ReadInt("\nEnter new value for Health: ") },
            { "Level", () => player.Level = _inputManager.ReadInt("\nEnter new value for Level: ") },
            { "Archetype", () => {

                var archetype = _inputManager.Selector(
                    _archetypeDao.GetAllArchetypes(),
                    a => a.Name,
                    "Select a new archetype",
                    b => ColorfulToStringHelper.GetArchetypeColor(b)
                );

                if (archetype == null)
                {
                    _outputManager.WriteLine("\nArchetype not selected.\n", ConsoleColor.Red);
                    return;
                }

                player.ArchetypeId = archetype.Id;
                player.Archetype = archetype;
                } },
            { "Gold", () => player.Inventory.Gold = _inputManager.ReadInt("\nEnter new value for Gold: ") },
            { "Capacity", () => player.Inventory.Capacity = _inputManager.ReadDecimal("\nEnter new value for Capacity: ") }
        };

        while (true)
        {
            _outputManager.WriteLine($"\nEditing player: {player.Name}", ConsoleColor.Cyan);
            _outputManager.WriteLine($"{ColorfulToStringHelper.PlayerToString(player)}\n", ConsoleColor.Green);

            int option = _inputManager.DisplayEditMenu([.. propertyActions.Keys]);

            if (option == propertyActions.Count + 1)
            {
                _playerDao.UpdatePlayer(player);
                _outputManager.WriteLine($"\nExiting. Any changes made have been successfully saved to {player.Name}\n", ConsoleColor.Green);
                return;
            }

            string selectedProperty = propertyActions.Keys.ElementAt(option - 1);
            propertyActions[selectedProperty].Invoke();
        }
    }
    private void DeletePlayer()
    {
        do
        {
            var players = _playerDao.GetAllPlayers();

            if (players.Count == 0)
            {
                _outputManager.WriteLine("\nNo players available for deletion.\n", ConsoleColor.Red);
                break;
            }

            _outputManager.WriteLine();
            var playerToDelete = 
                _inputManager.Selector(
                    players,
                    p => ColorfulToStringHelper.PlayerToString(p),
                    "Select a player to delete",
                    b => ColorfulToStringHelper.GetArchetypeColor(b.Archetype)
                );

            if (playerToDelete == null)
            {
                _outputManager.WriteLine("No player selected to delete.", ConsoleColor.Red);
                break;
            }

            if (!_inputManager.ConfirmAction("delete"))
            {
                _outputManager.WriteLine($"\nDeletion cancelled. Character [{playerToDelete.Name}] has not been deleted.", ConsoleColor.Red);
                break;
            }

            _playerDao.DeletePlayer(playerToDelete);

            _outputManager.WriteLine("\nCharacter has been deleted successfully!\n", ConsoleColor.Green);
        } while (_inputManager.LoopAgain("delete"));
        _outputManager.WriteLine();
    }
}