using ConsoleGame.Managers;

namespace ConsoleGame.Menus;

public class StartMenu(OutputManager outputManager, InputManager inputManager)
{
    private readonly OutputManager _outputManager = outputManager;
    private readonly InputManager _inputManager = inputManager;

    public bool ShowStartMenu()
    {
        _outputManager.WriteLine("Welcome to the RPG Game!", ConsoleColor.Yellow);
        _outputManager.WriteLine("1. Start Game", ConsoleColor.Cyan);
        _outputManager.WriteLine("2. Exit", ConsoleColor.Cyan);
        
        var input = _inputManager.ReadMenuKey(2);

        return input == 1;
    }
}
