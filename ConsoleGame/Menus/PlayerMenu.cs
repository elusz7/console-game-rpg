﻿using ConsoleGame.Helpers.CrudHelpers;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;

namespace ConsoleGame.Menus;

public class PlayerMenu(IInputManager inputManager, IOutputManager outputManager, PlayerDisplay playerDisplayMenu, PlayerManagement playerManagementMenu)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly PlayerDisplay _playerDisplayMenu = playerDisplayMenu;
    private readonly PlayerManagement _playerManagementMenu = playerManagementMenu;

    public void PlayerMainMenu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Player Main Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. View Players"
                + "\n2. Manage Players"
                + "\n3. Return to Admin Menu");
            
            var choice = _inputManager.ReadMenuKey(3);
            
            switch (choice)
            {
                case 1:
                    _playerDisplayMenu.Menu();
                    break;
                case 2:
                    _playerManagementMenu.Menu();
                    break;
                case 3:
                    _outputManager.Clear();
                    return;
            }
        }
    }    
}
