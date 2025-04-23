using System.Reflection;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Abilities;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ConsoleGame.Helpers;

public class CharacterManager(GameContext context, InputManager inputManager, OutputManager outputManager, InventoryManager inventoryManager)
{
    private readonly GameContext _context = context;
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly InventoryManager _inventoryManager = inventoryManager;

    private List<Player> characters;

    public void CharacterMainMenu()
    {
        characters = _context.Players
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items)
            .Include(p => p.Abilities)
            .ToList();

        _outputManager.Clear();

        while (true)
        {
            _outputManager.Write("Character Main Menu", ConsoleColor.Cyan);
            _outputManager.Write("\n1. View Characters"
                + "\n2. Manage Characters"
                + "\n3. Return to Main Menu"
                + "\n\tSelect an option: ");
            _outputManager.Display();
            string choice = _inputManager.ReadString();

            switch (choice)
            {
                case "1":
                    CharacterDisplayMenu();
                    break;
                case "2":
                    CharacterManagementMenu();
                    break;
                case "3":
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.Write("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }
    private void CharacterDisplayMenu()
    {
        while (true)
        {
            _outputManager.WriteLine("\nCharacter Display Menu", ConsoleColor.Cyan);
            _outputManager.Write("1. List All Characters"
                + "\n2. Search Character By Name"
                + "\n3. Return to Character Main Menu"
                + "\n\tSelect an option: ");
            _outputManager.Display();
            string choice = _inputManager.ReadString();

            switch (choice)
            {
                case "1":
                    ListCharactersFullInfo(characters);
                    break;
                case "2":
                    FindCharacterByName();
                    break;
                case "3":
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.Write("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }
    private void CharacterManagementMenu()
    {
        while (true)
        {
            _outputManager.WriteLine("\nCharacter Management Menu", ConsoleColor.Cyan);
            _outputManager.Write("1. Add Character"
                + "\n2. Edit Character"
                + "\n3. Remove Character"
                + "\n4. Return to Character Main Menu"
                + "\n\tSelect an option: ");
            _outputManager.Display();
            string choice = _inputManager.ReadString();

            switch (choice)
            {
                case "1":
                    AddCharacter();
                    break;
                case "2":
                    EditCharacter();
                    break;
                case "3":
                    RemoveCharacter();
                    break;
                case "4":
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.Write("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }
    private void ListCharactersFullInfo(List<Player> characterList)
    {
        _outputManager.WriteLine();
        foreach (var ch in characterList)
        {
            _outputManager.WriteLine(ch.ToString());
        }
        _outputManager.Display();
    }
    private Player SelectCharacter(string prompt)
    {
        int choice = -1;

        _outputManager.WriteLine();
        for (int i = 0; i < characters.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {characters[i].Name}");
        }
        _outputManager.Write($"\t{prompt}: ");
        _outputManager.Display();

        while (true)
        {
            string input = _inputManager.ReadString();

            if (int.TryParse(input, out int index) && index > 0 && index <= characters.Count)
            {
                choice = index - 1;
                break;
            }
            else
            {
                //find character by name
                var character = characters.FirstOrDefault(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
                if (character != null)
                {
                    choice = characters.IndexOf(character);
                }
                else
                {
                    _outputManager.Write("Invalid choice. Please choose again: ");
                    _outputManager.Display();
                }
            }
        }

        return characters[choice];
    }
    private void FindCharacterByName()
    {
        _outputManager.Write("\nEnter name of character: ");
        _outputManager.Display();
        string name = _inputManager.ReadString();

        var characterList = characters
            .Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (characterList.Count == 0)
        {
            _outputManager.WriteLine($"\nNo characters found matching [{name}]\n");
        }
        else
        {
            _outputManager.WriteLine($"\nFound {characterList.Count} character(s) matching [{name}]:");
            ListCharactersFullInfo(characterList);
        }
    }
    private void AddCharacter()
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

        List<Ability> abilities = new List<Ability>();
        List<Item> items = new List<Item>();

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

        _outputManager.Write($"\nWould you like to assign {name} some starting abilities? (y/n) ");
        _outputManager.Display();
        string assignAbilities = _inputManager.ReadString().ToLower();
        if (assignAbilities == "y")
            _outputManager.Write("TO BE ADDED LATER");
        else
            _outputManager.WriteLine("No abilities assigned. Abilities can be added later through the main menu.");
        
        _outputManager.Write($"\nWould you like to assign {name} some starting items? (y/n) ");
        _outputManager.Display();
        string assignItems = _inputManager.ReadString().ToLower();
        if (assignItems == "y")
            _inventoryManager.InventoryMainMenu("Character Menu", newCharacter);
        else
            _outputManager.WriteLine("No items assigned. Items can be added later through the main menu.\n");

        _outputManager.WriteLine($"New Character [{name}] added successfully!");
        _outputManager.Display();
    }
    private void EditCharacter()
    {
        Player character = SelectCharacter("Select A Character To Edit");

        string[] editableProperties = { "Name", "Health", "Experience", "Gold", "Capacity" };

        while (true)
        {
            _outputManager.WriteLine($"\nEditing character: {character.Name}", ConsoleColor.Cyan);
            _outputManager.Write($"{character.ToString()}"
                + "\n\n\tWhat property would you like to edit? (exit to quit) ");
            _outputManager.Display();

            string input = _inputManager.ReadString().Trim();

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                _context.UpdatePlayer(character);
                _outputManager.WriteLine($"Changes successfuly saved to {character.Name}\n");
                return;
            }
            else if (editableProperties.Any(property => property.Equals(input, StringComparison.OrdinalIgnoreCase)))
            {
                _outputManager.Write($"\tEnter new value for {input}: ");
                _outputManager.Display();
                string newValue = _inputManager.ReadString().Trim();
                bool isNumber = int.TryParse(newValue, out int newIntValue);

                switch (input.ToLower())
                {
                    case "name":
                        character.Name = newValue;
                        break;
                    case "health":
                        if (isNumber)
                        {
                            character.Health = newIntValue;
                        }
                        else
                        {
                            _outputManager.WriteLine("Invalid input. Health must be a number.");
                            continue;
                        }
                        break;
                    case "experience":
                        if (isNumber)
                        {
                            character.Experience = newIntValue;
                        }
                        else
                        {
                            _outputManager.WriteLine("Invalid input. Experience must be a number.");
                            continue;
                        }
                        break;
                    case "gold":
                        if (isNumber)
                        {
                            character.Inventory.Gold = newIntValue;
                        }
                        else
                        {
                            _outputManager.WriteLine("Invalid input. Gold must be a number.");
                            continue;
                        }
                        break;
                    case "capacity":
                        if (isNumber)
                        {
                            character.Inventory.Capacity = newIntValue;
                        }
                        else
                        {
                            _outputManager.WriteLine("Invalid input. Capacity must be a number.");
                            continue;
                        }
                        break;
                }
            }
            else
            {
                _outputManager.WriteLine("Invalid property. Please try again.\n");
                continue;
            }
        }
    }
    private void RemoveCharacter()
    {
        Player characterToRemove = SelectCharacter("Select A Character To Remove");

        _outputManager.Write($"\nPlease confirm deletion of {characterToRemove.Name} (y/n): ");
        _outputManager.Display();
        char confirm = _inputManager.ReadString().Trim().ToLower()[0];

        if (confirm == 'y')
        {
            //update local list
            characters.Remove(characterToRemove);

            //update database
            _context.Players.Remove(characterToRemove);
            _context.SaveChanges();

            //confirm deletion
            _outputManager.WriteLine("Character has been removed successfully!", ConsoleColor.Green);
        }
        else
        {
            _outputManager.WriteLine($"Deletion cancelled. Character [{characterToRemove.Name}] has not been removed.", ConsoleColor.Red);
        }
    }
}
