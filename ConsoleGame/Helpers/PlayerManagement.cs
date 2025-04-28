using ConsoleGame.GameDao;
using ConsoleGameEntities.Models;
using ConsoleGameEntities.Models.Abilities;
using ConsoleGameEntities.Models.Entities;
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
            _outputManager.WriteLine("Player Management Menu", ConsoleColor.Cyan);
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
            }
        };

        _playerDao.AddPlayer(newPlayer);

        string assignItems = _inputManager.ReadString($"\nWould you like to assign {name} some starting items? (y/n) ").ToLower();
        if (assignItems == "y")
            _inventoryManagement.Menu(newPlayer);
        else
            _outputManager.WriteLine("No items assigned. Items can be added later through the main menu.\n");

        _outputManager.WriteLine($"New Player [{name}] added successfully!\n");
        _outputManager.Display();
    }
    private void EditPlayer()
    {
        Player player = SelectPlayer("Select the number of the player you'd like to edit: ");

        string[] validResponses = { "Name", "Health", "Experience", "Gold", "Capacity"};

        while (true)
        {
            _outputManager.WriteLine($"\nEditing player: {player.Name}", ConsoleColor.Cyan);
            _outputManager.WriteLine($"{player.ToString()}\n", ConsoleColor.Green);

            for (int i = 0; i < validResponses.Length; i++)
            {
                _outputManager.WriteLine($"{i + 1}. Change {validResponses[i]}");
            }
            _outputManager.WriteLine($"{validResponses.Length + 1}. Exit");

            int index = _inputManager.ReadInt("\nWhat property would you like to edit? (exit to quit) ", validResponses.Length + 1) - 1;

            if (index == validResponses.Length)
            {
                _playerDao.UpdatePlayer(player);
                _outputManager.WriteLine($"\nExiting. Changes successfuly saved to {player.Name}\n", ConsoleColor.Green);
                return;
            }
            else
            {
                string newName = "";
                int newValue = -1;
                decimal newCapacity = -1.0M;

                if (validResponses[index] == "Name")
                    newName = _inputManager.ReadString($"\nEnter new value for {validResponses[index]}: ");
                else if (validResponses[index] == "Capacity")
                    newCapacity = _inputManager.ReadDecimal($"\nEnter new value for {validResponses[index]}: ");
                else
                    newValue = _inputManager.ReadInt($"\nEnter new value for {validResponses[index]}: ");

                switch (validResponses[index])
                {
                    case "Name":
                        player.Name = newName;
                        break;
                    case "Health":
                        player.Health = newValue;
                        break;
                    case "Experience":
                        player.Experience = newValue;
                        break;
                    case "Gold":
                        player.Inventory.Gold = newValue;
                        break;
                    case "Capacity":
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

            _outputManager.WriteLine("\nCharacter has been removed successfully!\n", ConsoleColor.Green);
        }
        else
        {
            _outputManager.WriteLine($"\nDeletion cancelled. Character [{playerToDelete.Name}] has not been removed.\n", ConsoleColor.Red);
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