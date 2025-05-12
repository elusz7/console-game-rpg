using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Managers.CrudHelpers;

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

        var archetypes = _archetypeDao.GetArchetypeNames();

        var selectedArchetype = _inputManager.PaginateList(archetypes, "archetype", $"assign {name}", true, false);

        if (selectedArchetype == null)
        {
            _outputManager.WriteLine("\nNo archetype selected. Player creation cancelled.\n", ConsoleColor.Red);
            return;
        }
        int archetypeId = _archetypeDao.GetArchetypeId(selectedArchetype);
        _outputManager.WriteLine($"Archetype Assigned: {selectedArchetype}", ConsoleColor.Green);

        int health = _inputManager.ReadInt("Enter player's health: ");

        int gold = _inputManager.ReadInt("Enter player's gold: ");

        decimal capacity = _inputManager.ReadDecimal("Enter player's weight carrying capacity: ");

        List<Item> items = [];

        Player newPlayer = new()
        {
            Name = name,
            ArchetypeId = archetypeId,
            MaxHealth = health,
            Experience = 0,
            Level = 1,
            Inventory = new Inventory
            {
                Gold = gold,
                Capacity = capacity,
                Items = items
            }
        };

        _playerDao.AddPlayer(newPlayer);

        string assignItems = _inputManager.ReadString($"\nWould you like to assign {name} some starting items? (y/n) ", new[] {"y", "n"}).ToLower();
        if (assignItems == "y")
            _inventoryManagement.Menu(newPlayer);
        else
            _outputManager.WriteLine("No items assigned. Items can be added later through the main menu.\n");

        _outputManager.WriteLine($"New Player [{name}] added successfully!\n", ConsoleColor.Green);
    }
    private void EditPlayer()
    {
        Player player = SelectPlayer("Select the number of the player you'd like to edit: ");

        if (player == null)
        {
            _outputManager.WriteLine("\nNo players available for editing.\n", ConsoleColor.Red);
            return;
        }

        var propertyActions = new Dictionary<string, Action>
        {
            { "Name", () => player.Name = _inputManager.ReadString("\nEnter new value for Name: ") },
            { "Health", () => player.MaxHealth = _inputManager.ReadInt("\nEnter new value for Health: ") },
            { "Level", () => player.Level = _inputManager.ReadInt("\nEnter new value for Level: ") },
            { "Archetype", () => player.ArchetypeId = _archetypeDao.GetArchetypeId(_inputManager.PaginateList(_archetypeDao.GetArchetypeNames(), "archetype", $"assign to {player.Name}", true, false)) },
            { "Experience", () => player.Experience = _inputManager.ReadInt("\nEnter new value for Experience: ") },
            { "Gold", () => player.Inventory.Gold = _inputManager.ReadInt("\nEnter new value for Gold: ") },
            { "Capacity", () => player.Inventory.Capacity = _inputManager.ReadDecimal("\nEnter new value for Capacity: ") }
        };

        while (true)
        {
            _outputManager.WriteLine($"\nEditing player: {player.Name}", ConsoleColor.Cyan);
            _outputManager.WriteLine($"{player}\n", ConsoleColor.Green);

            int option = DisplayEditMenu(propertyActions.Keys.ToList());

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
    private int DisplayEditMenu(List<string> properties)
    {
        for (int i = 0; i < properties.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. Change {properties[i]}");
        }
        _outputManager.WriteLine($"{properties.Count + 1}. Exit", ConsoleColor.Red);

        return _inputManager.ReadInt("\nWhat property would you like to edit? ", properties.Count + 1);
    }

    private void DeletePlayer()
    {
        do
        {
            _outputManager.WriteLine();
            Player playerToDelete = SelectPlayer("Select the number of the player you'd like to delete: ");

            if (playerToDelete == null)
            {
                _outputManager.WriteLine("No players available to delete.", ConsoleColor.Red);
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
    private Player SelectPlayer(string prompt)
    {
        List<Player> players = _playerDao.GetAllPlayers();
        Player selectedPlayer = null;

        if (players.Count != 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                _outputManager.WriteLine($"{i + 1}. {players[i].Name}");
            }

            int index = _inputManager.ReadInt($"\t{prompt}", players.Count);
            selectedPlayer = players[index - 1];
        }
        return selectedPlayer;
    }
}