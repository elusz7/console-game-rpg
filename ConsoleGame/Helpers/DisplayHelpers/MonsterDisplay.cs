using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Managers.DisplayHelpers;

public class MonsterDisplay(OutputManager outputManager, InputManager inputManager, MonsterDao monsterDao)
{
    private readonly OutputManager _outputManager = outputManager;
    private readonly InputManager _inputManager = inputManager;
    private readonly MonsterDao _monsterDao = monsterDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Monster Display Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. List All Monsters"
                + "\n2. List Monsters By Threat"
                + "\n3. List Monsters By Damage Type"
                + "\n4. Return to Monster Menu");
            var input = _inputManager.ReadMenuKey(4);
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
                    _outputManager.Clear();
                    return;
            }
        }
    }

    private void ListMonsters(string? criteria = null)
    {
        var monsters = new List<Monster>();

        switch (criteria)
        {
            case "Threat Level":
                string[] threatLevels = ["Low", "Medium", "High", "Elite", "Boss"];

                for (int i = 0; i < threatLevels.Length; i++)
                {
                    _outputManager.WriteLine($"{i + 1} :  {threatLevels[i]}");
                }

                int levelChoice = _inputManager.ReadInt("Select a Threat Level: ", threatLevels.Length) - 1;

                monsters = _monsterDao.GetMonstersByThreatLevel(levelChoice);
                break;
            case "Damage Type":
                string[] damageTypes = ["Martial", "Magical", "Hybrid"];

                for (int i = 0; i < damageTypes.Length; i++)
                {
                    _outputManager.WriteLine($"{i + 1}: {damageTypes[i]}");
                }

                int damageChoice = _inputManager.ReadInt("Select a Damage Type: ", damageTypes.Length) - 1;

                monsters = _monsterDao.GetMonstersByDamageType(damageChoice);
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

        _inputManager.PaginateList(monsters);
    }
}
