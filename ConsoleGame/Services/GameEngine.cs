using ConsoleGame.Helpers;
using ConsoleGame.Managers;

namespace ConsoleGame.Services;

public class GameEngine
{
    private readonly StartMenuManager _menuManager;
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly PlayerManager _playerManager;
    private readonly InventoryManager _inventoryManager;
    private readonly RoomManager _roomManager;
    private readonly MonsterManager _monsterManager;
    private readonly ArchetypeManager _archetypeManager;
    private readonly SkillManager _skillManager;

    public GameEngine(
        StartMenuManager menuManager, InputManager inputManager, OutputManager outputManager,
        PlayerManager playerManager, InventoryManager inventoryManager, RoomManager roomManager,
        MonsterManager monsterManager, ArchetypeManager archetypeManager, SkillManager skillManager)
    {
        _menuManager = menuManager;
        _inputManager = inputManager;
        _outputManager = outputManager;
        _playerManager = playerManager;
        _inventoryManager = inventoryManager;
        _roomManager = roomManager;
        _monsterManager = monsterManager;
        _archetypeManager = archetypeManager;
        _skillManager = skillManager;
    }

    public void Run()
    {
        if (_menuManager.ShowStartMenu())
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
            _outputManager.WriteLine("1. Player Management"
                + "\n2. Inventory Management"
                + "\n3. Room Management"
                + "\n4. Monster Management"
                + "\n5. Archetype Management"
                + "\n6. Skill Management"
                + "\n7. Quit");

            var input = _inputManager.ReadMenuKey(7);

            switch (input)
            {
                case 1:
                    _playerManager.PlayerMainMenu();
                    break;
                case 2:
                    _inventoryManager.InventoryMainMenu();
                    break;
                case 3:
                    _roomManager.RoomMainMenu();
                    break;
                case 4:
                    _monsterManager.MonsterMainMenu();
                    break;
                case 5:
                    _archetypeManager.ArchetypeMainMenu();
                    break;
                case 6:
                    _skillManager.SkillMainMenu();
                    break;
                case 7:
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    return;
            }
        }
    }
}
