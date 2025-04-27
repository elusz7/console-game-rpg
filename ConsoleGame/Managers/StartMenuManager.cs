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
        _outputManager.Display();

        return HandleStartMenuInput();
    }

    private bool HandleStartMenuInput()
    {
        while (true)
        {
            var input = _inputManager.ReadKey().KeyChar.ToString();
            switch (input)
            {
                case "1":
                    _outputManager.WriteLine("Starting game...", ConsoleColor.Green);
                    _outputManager.Display();
                    return true;
                case "2":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    return false;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose 1 or 2.", ConsoleColor.Red);
                    _outputManager.Display();
                    break;
            }
        }
    }
}
