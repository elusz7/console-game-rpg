using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Abilities;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Helpers;

public class PlayerManagement(InputManager inputManager, OutputManager outputManager, PlayerDao playerDao, InventoryManagement inventoryManagement)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly PlayerDao _playerDao = playerDao;
    private readonly InventoryManagement _inventoryManagement = inventoryManagement;
    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("\nPlayer Management Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. Add Player"
                + "\n2. Edit Player"
                + "\n3. Remove Player"
                + "\n4. Return to Player Main Menu"
                + "\n\tSelect an option: ";
            string choice = _inputManager.ReadString(menuPrompt);

            switch (choice)
            {
                case "1":
                    AddPlayer();
                    break;
                case "2":
                    EditPlayer();
                    break;
                case "3":
                    RemovePlayer();
                    break;
                case "4":
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.Write("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }
    private void AddPlayer()
    {
        string name = _inputManager.ReadString("\nEnter player name: ");

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
            },
            Abilities = abilities
        };

        _playerDao.AddPlayer(newPlayer);

        string assignAbilities = _inputManager.ReadString($"\nWould you like to assign {name} some starting abilities? (y/n) ").ToLower();
        if (assignAbilities == "y")
            _outputManager.Write("TO BE ADDED LATER");
        else
            _outputManager.WriteLine("No abilities assigned. Abilities can be added later through the main menu.");

        string assignItems = _inputManager.ReadString($"\nWould you like to assign {name} some starting items? (y/n) ").ToLower();
        if (assignItems == "y")
            _inventoryManagement.Menu(newPlayer);
        else
            _outputManager.WriteLine("No items assigned. Items can be added later through the main menu.\n");

        _outputManager.WriteLine($"New Player [{name}] added successfully!");
        _outputManager.Display();
    }
    private void EditPlayer()
    {
        Player player = SelectPlayer("Select the number of the player you'd like to edit: ");

        string[] validResponses = { "Name", "Health", "Experience", "Gold", "Capacity", "exit" };

        while (true)
        {
            _outputManager.WriteLine($"\nEditing player: {player.Name}", ConsoleColor.Cyan);
            _outputManager.Write($"{player.ToString()}\n\n\t");

            string input = _inputManager.ReadString("What property would you like to edit? (exit to quit) ", validResponses).ToLower();

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                _playerDao.UpdatePlayer(player);
                _outputManager.WriteLine($"\nChanges successfuly saved to {player.Name}", ConsoleColor.Green);
                return;
            }
            else
            {
                string newName = "";
                int newValue = -1;
                decimal newCapacity = -1.0M;

                if (input == "name")
                    newName = _inputManager.ReadString($"\tEnter new value for {input}: ");
                else if (input == "capacity")
                    newCapacity = _inputManager.ReadDecimal($"\tEnter new value for {input}: ");
                else
                    newValue = _inputManager.ReadInt($"\tEnter new value for {input}: ");

                switch (input)
                {
                    case "name":
                        player.Name = newName;
                        break;
                    case "health":
                        player.Health = newValue;
                        break;
                    case "experience":
                        player.Experience = newValue;
                        break;
                    case "gold":
                        player.Inventory.Gold = newValue;
                        break;
                    case "capacity":
                        player.Inventory.Capacity = newCapacity;
                        break;
                }
            }
        }
    }
    private void RemovePlayer()
    {
        Player playerToDelete = SelectPlayer("Select the number of the player you'd like to delete: ");

        string confirm = _inputManager.ReadString($"\nPlease confirm deletion of {playerToDelete.Name} (y/n): ", ["y", "n"]);

        if (confirm == "y")
        {
            _playerDao.DeletePlayer(playerToDelete);

            _outputManager.WriteLine("Character has been removed successfully!", ConsoleColor.Green);
        }
        else
        {
            _outputManager.WriteLine($"Deletion cancelled. Character [{playerToDelete.Name}] has not been removed.", ConsoleColor.Red);
        }
    }
    private Player SelectPlayer(string prompt)
    {
        List<Player> players = _playerDao.GetAllPlayers();
        Player selectedPlayer = null;

        if (players.Count == 0)
            _outputManager.WriteLine($"\nNo players found.");

        _outputManager.WriteLine();
        for (int i = 0; i < players.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {players[i].Name}");
        }

        int index = _inputManager.ReadInt($"\t{prompt}", players.Count);
        selectedPlayer = players[index - 1];

        return selectedPlayer;
    }
}
