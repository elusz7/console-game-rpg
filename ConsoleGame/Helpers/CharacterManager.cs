using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Abilities;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers;

public class CharacterManager(GameContext context, InputManager inputManager, OutputManager outputManager)
{
    private readonly GameContext _context = context;
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;

    private List<Player> characters;

    public void CharacterMenu()
    {
        characters = _context.Players.ToList();

        _outputManager.Clear();

        while (true)
        {
            _outputManager.Write("Character Menu", ConsoleColor.Cyan);
            _outputManager.Write("\n1. Display All Characters"
                + "\n2. Create New Character"
                + "\n3. Update Existing Character"
                + "\n4. Search Characters"
                + "\n5. Delete Character"
                + "\n6. Return to Main Menu"
                + "\n\tSelect an option: ");
            _outputManager.Display();
            string choice = _inputManager.ReadString();

            switch (choice)
            {
                case "1":
                    ListCharacters(characters);
                    break;
                case "2":
                    AddCharacter();
                    break;
                case "6":
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.Write("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    public void ListCharacters(List<Player> characterList)
    {
        _outputManager.WriteLine();

        foreach (var ch in characterList)
        {
            _outputManager.Write("Name: ");
            _outputManager.Write(ch.Name, ConsoleColor.Magenta);

            _outputManager.Write(", Health: ");
            _outputManager.Write(ch.Health.ToString(), ConsoleColor.Green);

            _outputManager.Write(", Experience: ");
            _outputManager.Write(ch.Experience.ToString(), ConsoleColor.Cyan);

            _outputManager.Write(", Capacity: ");
            _outputManager.Write(ch.Inventory.Capacity.ToString(), ConsoleColor.Red);

            _outputManager.Write(", Gold: ");
            _outputManager.Write(ch.Inventory.Gold.ToString(), ConsoleColor.Yellow);

            _outputManager.Write("\n\tAbilities: ");
            _outputManager.Write(string.Join(", ", ch.Abilities.Select(a => a.Name)), ConsoleColor.Blue);

            _outputManager.Write("\n\tInventory: ");
            _outputManager.Write(string.Join(", ", ch.Inventory.Items.Select(i => i.Name)), ConsoleColor.DarkBlue);

            _outputManager.WriteLine();
        }

        _outputManager.WriteLine();
        _outputManager.Display();
    }

    public void AddCharacter()
    {
        _outputManager.Write("\nEnter character name: ");
        _outputManager.Display();
        string name = _inputManager.ReadString();

        _outputManager.Write("Enter character's starting health: ");
        _outputManager.Display();
        int health = _inputManager.ReadInt();

        while (health < 0)
        {
            _outputManager.Write("\tInvalid input. Please enter a positive number: ");
            _outputManager.Display();
            health = _inputManager.ReadInt();
        }

        _outputManager.Write("Enter character's starting gold: ");
        _outputManager.Display();
        int gold = _inputManager.ReadInt();
        
        while (gold == -1)
        {
            _outputManager.Write("\tInvalid input. Please enter a positive number: ");
            _outputManager.Display();
            gold = _inputManager.ReadInt();
        }

        _outputManager.Write("Enter character's weight carrying capacity: ");
        _outputManager.Display();
        int capacity = _inputManager.ReadInt();

        _outputManager.Write($"\nWould you like to assign {name} some starting abilities? (y/n) ");
        _outputManager.Display();
        string assignAbilities = _inputManager.ReadString().ToLower();

        if (assignAbilities == "y")
            _outputManager.Write("TO BE ADDED LATER");
        else
            _outputManager.Write("No abilities assigned. Abilities can be added later through the main menu.");

        List<Ability> abilities = new List<Ability>();
        _outputManager.Write($"\nWould you like to assign {name} some starting items? (y/n) ");
        _outputManager.Display();
        string assignItems = _inputManager.ReadString().ToLower();

        List<Item> items = new List<Item>();
        if (assignItems == "y")
            _outputManager.Write("TO BE ADDED LATER");
        else
            _outputManager.Write("No items assigned. Items can be added later through the main menu.\n");

        Player newCharacter = new Player
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
        _context.Players.Add(newCharacter);
        _context.SaveChanges();

        characters.Add(newCharacter);

        _outputManager.WriteLine();
        _outputManager.Display();
    }
}
