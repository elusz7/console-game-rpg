using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Helpers.CrudHelpers;

namespace ConsoleGame.Menus;

public class ArchetypeMenu(IInputManager inputManager, IOutputManager outputManager, ArchetypeDisplay archetypeDisplay, ArchetypeManagement archetypeManagement)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly ArchetypeDisplay _archetypeDisplay = archetypeDisplay;
    private readonly ArchetypeManagement _archetypeManagement = archetypeManagement;

    public void ArchetypeMainMenu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Archetype Main Menu ===");
            _outputManager.WriteLine("1. View Archetypes"
                + "\n2. Manage Archetypes"
                + "\n3. Return to Admin Menu");

            var choice = _inputManager.ReadMenuKey(3);

            switch (choice)
            {
                case 1:
                    _archetypeDisplay.Menu();
                    break;
                case 2:
                    _archetypeManagement.Menu();
                    break;
                case 3:
                    _outputManager.Clear();
                    return;
            }
        }
    }
}
