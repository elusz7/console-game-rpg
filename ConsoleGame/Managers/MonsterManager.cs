using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;

namespace ConsoleGame.Helpers;

public class MonsterManager(GameContext context, InputManager inputManager, OutputManager outputManager, MonsterDisplay monsterDisplay, MonsterManagement monsterManagement)
{
    private readonly GameContext _context = context;
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly MonsterDisplay _monsterDisplay = monsterDisplay;
    private readonly MonsterManagement _monsterManagement = monsterManagement;

    public void MonsterMainMenu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Monster Main Menu", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. View Monsters"
                + "\n2. Manage Monsters"
                + "\n3. Return to Main Menu");
            var input = _inputManager.ReadMenuKey(3);
            switch (input)
            {
                case 1:
                    _monsterDisplay.Menu();
                    break;
                case 2:
                    _monsterManagement.Menu();
                    break;
                case 3:
                    _outputManager.Clear();
                    return;
            }
        }
    }
}
