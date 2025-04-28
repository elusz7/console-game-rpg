using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Abilities;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers;

public class PlayerManagement
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly PlayerDao _playerDao;
    private readonly InventoryManagement _inventoryManagement;

    public PlayerManagement(InputManager inputManager, OutputManager outputManager, PlayerDao playerDao, InventoryManagement inventoryManagement)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _playerDao = playerDao;
        _inventoryManagement = inventoryManagement;
    }

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

        } while (LoopAgain("add"));
        _outputManager.WriteLine();
    }

    private void CreatePlayer()
    {
        _outputManager.WriteLine();
        string name = _inputManager.ReadString("Enter player name: ");

        int health = _inputManager.ReadInt("Enter player's starting health: ");

        int gold = _inputManager.ReadInt("Enter player's starting gold: ");

        decimal capacity = _inputManager.ReadDecimal("Enter player's weight carrying capacity: ");

        List<Ability> abilities = new List<Ability>();
        List<Item> items = new List<Item>();

        Player newPlayer = new Player
        {
            Name = name,
            Health = health,
            Experience = 0,
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
        { "Health", () => player.Health = _inputManager.ReadInt("\nEnter new value for Health: ") },
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

            if (!ConfirmAction("deletion"))
            {
                _outputManager.WriteLine($"\nDeletion cancelled. Character [{playerToDelete.Name}] has not been deleted.", ConsoleColor.Red);
                break;
            }

            _playerDao.DeletePlayer(playerToDelete);

            _outputManager.WriteLine("\nCharacter has been deleted successfully!\n", ConsoleColor.Green);
        } while (LoopAgain("delete"));
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
    private bool ConfirmAction(string action)
    {
        string confirm = _inputManager.ReadString($"\nPlease confirm {action} (y/n): ", new[] { "y", "n" }).ToLower();
        return confirm == "y";
    }
    private bool LoopAgain(string action)
    {
        string again = _inputManager.ReadString($"Would you like to {action} another? (y/n): ", new[] { "y", "n" }).ToLower();
        return again == "y";
    }
}