using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.Helpers;
using ConsoleGame.Helpers.CrudHelpers;
using ConsoleGame.Helpers.DisplayHelpers;

namespace ConsoleGame.Menus;

public class ArchetypeMenu
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly ArchetypeDisplay _archetypeDisplay;
    private readonly ArchetypeManagement _archetypeManagement;

    public ArchetypeMenu(InputManager inputManager, OutputManager outputManager, ArchetypeDisplay archetypeDisplay, ArchetypeManagement archetypeManagement)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _archetypeDisplay = archetypeDisplay;
        _archetypeManagement = archetypeManagement;
    }

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
