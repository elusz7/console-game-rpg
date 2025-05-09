using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.Helpers;

namespace ConsoleGame.Menus;

public class AdminMenu(InputManager inputManager, OutputManager outputManager,
    PlayerMenu playerManager, InventoryMenu inventoryManager, RoomMenu roomManager,
    MonsterMenu monsterManager, ArchetypeMenu archetypeManager, SkillMenu skillManager)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly PlayerMenu _playerManager = playerManager;
    private readonly InventoryMenu _inventoryManager = inventoryManager;
    private readonly RoomMenu _roomManager = roomManager;
    private readonly MonsterMenu _monsterManager = monsterManager;
    private readonly ArchetypeMenu _archetypeManager = archetypeManager;
    private readonly SkillMenu _skillManager = skillManager;

    public void AdminMainMenu()
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
                + "\n7. Return to Main Menu");

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
                    _outputManager.Clear();
                    return;
            }
        }
    }
}
