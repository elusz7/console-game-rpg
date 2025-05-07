using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.Helpers;

namespace ConsoleGame.Managers;

public class SkillManager
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly SkillDisplay _skillDisplay;
    private readonly SkillManagement _skillManagement;

    public SkillManager(InputManager inputManager, OutputManager outputManager, SkillDisplay skillDisplay, SkillManagement skillManagement)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _skillDisplay = skillDisplay;
        _skillManagement = skillManagement;
    }

    public void SkillMainMenu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Skill Main Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. View Skills"
                + "\n2. Manage Skills"
                + "\n3. Return to Main Menu");

            var choice = _inputManager.ReadMenuKey(3);
            switch (choice)
            {
                case 1:
                    _skillDisplay.Menu();
                    break;
                case 2:
                    _skillManagement.Menu();
                    break;
                case 3:
                    _outputManager.Clear();
                    return;
            }
        }
    }
}
