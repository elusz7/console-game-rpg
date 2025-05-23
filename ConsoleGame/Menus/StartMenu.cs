using ConsoleGame.Managers.Interfaces;

namespace ConsoleGame.Menus;

public class StartMenu(IOutputManager outputManager, IInputManager inputManager)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;

    public bool ShowStartMenu()
    {
        _outputManager.WriteLine("Welcome to the RPG Game!", ConsoleColor.Yellow);
        _outputManager.WriteLine("1. Start Game", ConsoleColor.Cyan);
        _outputManager.WriteLine("2. Exit", ConsoleColor.Cyan);
        
        var input = _inputManager.ReadMenuKey(2);

        return input == 1;
    }
}
