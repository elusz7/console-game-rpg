using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGame.GameDao;

namespace ConsoleGame.Managers.DisplayHelpers;

public class ArchetypeDisplay(InputManager inputManager, OutputManager outputManager, ArchetypeDao archetypeDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly ArchetypeDao _archetypeDao = archetypeDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Archetype Display Menu ===");
            _outputManager.WriteLine("1. List All Archetypes"
                + "\n2. List Archetypes By Type"
                + "\n3. Return to Archetype Main Menu");

            var choice = _inputManager.ReadMenuKey(3);

            switch (choice)
            {
                case 1:
                    ListArchetypes();
                    break;
                case 2:
                    ListArchetypes("Type");
                    break;
                case 3:
                    _outputManager.Clear();
                    return;
            }
        }
    }

    private void ListArchetypes(string? criteria = null)
    {
        var archetypes = new List<Archetype>();
        
        switch (criteria)
        {
            case "Type":
                _outputManager.WriteLine("1. Martial Archetypes\n2. Magical Archetypes", ConsoleColor.Yellow);
                var typeChoice = _inputManager.ReadInt("Choose Archetype Type: ", 2);
                var archetypeType = typeChoice == 1 ? ArchetypeType.Martial : ArchetypeType.Magical;

                archetypes = _archetypeDao.GetArchetypesByType(archetypeType);
                break;
            default:
                archetypes = _archetypeDao.GetAllArchetypes();
                break;
        }

        if (archetypes.Count == 0)
        {
            _outputManager.WriteLine("\nNo archetypes found.", ConsoleColor.Red);
            return;
        }

        _inputManager.PaginateList(archetypes);
    }
}
