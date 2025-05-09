using ConsoleGame.Helpers;
using ConsoleGame.Menus;

namespace ConsoleGame.Services;

public class GameEngine
{
    private readonly StartMenu _startMenu;
    private readonly AdminMenu _adminMenu;
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;

    public GameEngine(StartMenu startMenu, AdminMenu adminMenu, InputManager inputManager, OutputManager outputManager)
    {
        _startMenu = startMenu;
        _adminMenu = adminMenu;
        _inputManager = inputManager;
        _outputManager = outputManager;
    }

    public void Run()
    {
        if (_startMenu.ShowStartMenu())
        {
            GameLoop();
        }
    }
    private void GameLoop()
    {
        _outputManager.Clear();

        while (true)
        {
            _outputManager.WriteLine("Main Menu:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Administrator View"
                + "\n2. Adventure"
                + "\n3. Campaign"
                + "\n4. Quit");

            var input = _inputManager.ReadMenuKey(4);

            switch (input)
            {
                case 1:
                    _adminMenu.AdminMainMenu();
                    break;
                case 2:
                    _outputManager.WriteLine("Adventure mode is not implemented yet.", ConsoleColor.Red);
                    break;
                case 3:
                    _outputManager.WriteLine("Campaign mode is not implemented yet.", ConsoleColor.Red);
                    break;
                case 4:
                    _outputManager.WriteLine("Until next time, intrepid adventurer...", ConsoleColor.Red);
                    _outputManager.Display();
                    return;
            }
        }
    }
}
