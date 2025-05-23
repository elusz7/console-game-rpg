using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers.DisplayHelpers;

public class MonsterDisplay(IOutputManager outputManager, IInputManager inputManager, IMonsterDao monsterDao)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;
    private readonly IMonsterDao _monsterDao = monsterDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Monster Display Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. List All Monsters"
                + "\n2. List Monsters By Threat"
                + "\n3. List Monsters By Damage Type"
                + "\n4. Search For Monster By Name"
                + "\n5. Return to Monster Menu");
            
            var input = _inputManager.ReadMenuKey(5);
            switch (input)
            {
                case 1:
                    ListMonsters();
                    break;
                case 2:
                    ListMonsters("Threat Level");
                    break;
                case 3:
                    ListMonsters("Damage Type");
                    break;
                case 4:
                    ListMonsters("Search");
                    break;
                case 5:
                    _outputManager.Clear();
                    return;
            }
        }
    }

    private void ListMonsters(string? criteria = null)
    {
        List<Monster> monsters;

        switch (criteria)
        {
            case "Threat Level":
                var level = _inputManager.GetEnumChoice<ThreatLevel>("Select a Threat Level: ");
                monsters = _monsterDao.GetMonstersByThreatLevel(level);
                break;

            case "Damage Type":
                var damage = _inputManager.GetEnumChoice<DamageType>("Select a Damage Type: ");
                monsters = _monsterDao.GetMonstersByDamageType(damage);
                break;

            case "Search":
                var name = _inputManager.ReadString("Enter monster name: ");
                monsters = _monsterDao.GetMonstersByName(name);
                break;

            default:
                monsters = _monsterDao.GetAllMonsters();
                break;
        }

        if (monsters == null || monsters.Count == 0)
        {
            _outputManager.WriteLine("\nNo monsters found.\n", ConsoleColor.Red);
            return;
        }

        _inputManager.Viewer(monsters, m => ColorfulToStringHelper.MonsterToString(m), "", m => ColorfulToStringHelper.GetMonsterColor(m));
    }
}
