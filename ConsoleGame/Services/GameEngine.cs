using ConsoleGame.Managers;
using ConsoleGame.Menus;

namespace ConsoleGame.Services;

public class GameEngine(StartMenu startMenu, AdminMenu adminMenu, InputManager inputManager, OutputManager outputManager, AdventureService adventureService)
{
    private readonly StartMenu _startMenu = startMenu;
    private readonly AdminMenu _adminMenu = adminMenu;
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly AdventureService _adventureService = adventureService;

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
                    if (_adventureService.SetUpAdventure())
                    {
                        _outputManager.WriteLine("\nPress any key to begin your adventure!", ConsoleColor.Cyan);
                        _outputManager.Display();
                        _inputManager.ReadKey();
                        _adventureService.Adventure();
                    }
                    break;
                case 3:
                    if (_adventureService.SetUpCampaign())
                    {
                        _outputManager.WriteLine("\nPress any key to begin the campaign!", ConsoleColor.Cyan);
                        _outputManager.Display();
                        _inputManager.ReadKey();
                        _adventureService.Adventure();
                    }
                    break;
                case 4:
                    _outputManager.WriteLine("Until next time, intrepid adventurer...", ConsoleColor.Red);
                    _outputManager.Display();
                    return;
            }
        }
    }
}
