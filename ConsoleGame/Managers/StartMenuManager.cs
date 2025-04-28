namespace ConsoleGame.Helpers;

public class StartMenuManager
{
    private readonly OutputManager _outputManager;
    private readonly InputManager _inputManager;

    public StartMenuManager(OutputManager outputManager, InputManager inputManager)
    {
        _outputManager = outputManager;
        _inputManager = inputManager;
    }

    public bool ShowStartMenu()
    {
        _outputManager.WriteLine("Welcome to the RPG Game!", ConsoleColor.Yellow);
        _outputManager.WriteLine("1. Start Game", ConsoleColor.Cyan);
        _outputManager.WriteLine("2. Exit", ConsoleColor.Cyan);
        
        var input = _inputManager.ReadMenuKey(2);

        return input == 1;
    }
}
