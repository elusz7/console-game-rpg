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
            string menuPrompt = "\n1. View Characters"
                + "\n2. Manage Characters"
                + "\n3. Return to Main Menu"
                + "\n\tSelect an option: ";
            string choice = _inputManager.ReadString(menuPrompt);

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
                    _outputManager.WriteLine("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }
    private void CharacterDisplayMenu()
    {
        while (true)
        {
            _outputManager.WriteLine("\nCharacter Display Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. List All Characters"
                + "\n2. Search Character By Name"
                + "\n3. Return to Character Main Menu"
                + "\n\tSelect an option: ";
            string choice = _inputManager.ReadString(menuPrompt);

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
            string menuPrompt = "1. Add Character"
                + "\n2. Edit Character"
                + "\n3. Remove Character"
                + "\n4. Return to Character Main Menu"
                + "\n\tSelect an option: ";
            string choice = _inputManager.ReadString(menuPrompt);

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
    private Player SelectCharacter(string purpose)
    {
        string prompt = "\tSelect the number of a character: ";

        if (purpose.Equals("EditCharacter"))
            prompt = "\tSelect the number of the character you'd like to edit: ";
        else if (purpose.Equals("RemoveCharacter"))
            prompt = "\tSelect the number of the character you'd like to remove: ";
        
        _outputManager.WriteLine();
        for (int i = 0; i < characters.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {characters[i].Name}");
        }

        int index = _inputManager.ReadInt(prompt, characters.Count);

        return characters[index - 1];
    }
    private void FindCharacterByName()
    {
        string name = _inputManager.ReadString("\nEnter name of character: ");

        var characterList = characters
            .Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (characterList.Count == 0)
        {
            _outputManager.WriteLine($"\nNo characters found matching [{name}]");
        }
        else
        {
            _outputManager.WriteLine($"\nFound {characterList.Count} character(s) matching [{name}]:");
            ListCharactersFullInfo(characterList);
        }
    }
    private void AddCharacter()
    {
        string name = _inputManager.ReadString("\nEnter character name: ");

        int health = _inputManager.ReadInt("Enter character's starting health: ");

        int gold = _inputManager.ReadInt("Enter character's starting gold: ");
        
        int capacity = _inputManager.ReadInt("Enter character's weight carrying capacity: ");

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

        string assignAbilities = _inputManager.ReadString($"\nWould you like to assign {name} some starting abilities? (y/n) ").ToLower();
        if (assignAbilities == "y")
            _outputManager.Write("TO BE ADDED LATER");
        else
            _outputManager.WriteLine("No abilities assigned. Abilities can be added later through the main menu.");
        
        string assignItems = _inputManager.ReadString($"\nWould you like to assign {name} some starting items? (y/n) ").ToLower();
        if (assignItems == "y")
            _inventoryManager.InventoryMainMenu("Character Menu", newCharacter);
        else
            _outputManager.WriteLine("No items assigned. Items can be added later through the main menu.\n");

        _outputManager.WriteLine($"New Character [{name}] added successfully!");
        _outputManager.Display();
    }
    private void EditCharacter()
    {
        Player character = SelectCharacter("EditCharacter");

        string[] validResponses = { "Name", "Health", "Experience", "Gold", "Capacity", "exit" };

        while (true)
        {
            _outputManager.WriteLine($"\nEditing character: {character.Name}", ConsoleColor.Cyan);
            _outputManager.Write($"{character.ToString()}\n\n\t");

            string input = _inputManager.ReadString("What property would you like to edit? (exit to quit) ", validResponses).ToLower();

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                _context.UpdatePlayer(character);
                _outputManager.WriteLine($"\nChanges successfuly saved to {character.Name}", ConsoleColor.Green);
                return;
            }
            else
            {
                string newName = "";
                int newValue = -1;
                
                if (input == "name")
                    newName = _inputManager.ReadString($"\tEnter new value for {input}: ");
                else
                    newValue = _inputManager.ReadInt($"\tEnter new value for {input}: ");

                switch (input)
                {
                    case "name":
                        character.Name = newName;
                        break;
                    case "health":
                        character.Health = newValue;
                        break;
                    case "experience":
                        character.Experience = newValue;
                        break;
                    case "gold":
                        character.Inventory.Gold = newValue;
                        break;
                    case "capacity":
                        character.Inventory.Capacity = newValue;
                        break;
                }
            }
        }
    }
    private void RemoveCharacter()
    {
        Player characterToRemove = SelectCharacter("RemoveCharacter");

        string confirm = _inputManager.ReadString($"\nPlease confirm deletion of {characterToRemove.Name} (y/n): ", ["y", "n"]);

        if (confirm == "y")
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
