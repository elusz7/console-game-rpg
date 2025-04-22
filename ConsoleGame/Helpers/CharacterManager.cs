using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Characters;

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
}
