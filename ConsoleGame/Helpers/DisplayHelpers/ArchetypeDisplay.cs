using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers.DisplayHelpers;

public class ArchetypeDisplay(IInputManager inputManager, IOutputManager outputManager, IArchetypeDao archetypeDao)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IArchetypeDao _archetypeDao = archetypeDao;

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
                var archetypeType = _inputManager.GetEnumChoice<ArchetypeType>("Select an archetype type to list:");

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

        _inputManager.Viewer(archetypes, a => ColorfulToStringHelper.ArchetypeToString(a), "", a => ColorfulToStringHelper.GetArchetypeColor(a));
    }
}
