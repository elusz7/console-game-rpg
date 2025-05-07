using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers;

public class MonsterDisplay
{
    private readonly OutputManager _outputManager;
    private readonly InputManager _inputManager;
    private readonly MonsterDao _monsterDao;

    public MonsterDisplay(OutputManager outputManager, InputManager inputManager, MonsterDao monsterDao)
    {
        _outputManager = outputManager;
        _inputManager = inputManager;
        _monsterDao = monsterDao;
    }

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Monster Display Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. List All Monsters"
                + "\n2. List Monsters By Threat"
                + "\n3. List Monsters By Damage Type"
                + "\n4. Return to Main Menu");
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
        List<Monster>? monsters = null;

        if (criteria == null)
        {
            monsters = _monsterDao.GetAllMonsters();
        }
        else if (criteria.Equals("Threat Level", StringComparison.OrdinalIgnoreCase))
        {
            string[] threatLevels = ["Low", "Medium", "High", "Elite", "Boss"];

            for (int i = 0; i < threatLevels.Length; i++)
            {
                _outputManager.WriteLine($"{i + 1} :  {threatLevels[i]}");
            }

            int selected = _inputManager.ReadInt("Select a Threat Level: ", threatLevels.Length) - 1;

            monsters = _monsterDao.GetMonstersByThreatLevel(selected);
        }
        else if (criteria.Equals("Damage Type", StringComparison.OrdinalIgnoreCase))
        {
            string[] damageTypes = ["Martial", "Magical", "Hybrid"];

            for (int i = 0; i < damageTypes.Length; i++)
            {
                _outputManager.WriteLine($"{i + 1}: {damageTypes[i]}");
            }

            int selected = _inputManager.ReadInt("Select a Damage Type: ", damageTypes.Length) - 1;

            monsters = _monsterDao.GetMonstersByDamageType(selected);
        }
        else
        {
            _outputManager.WriteLine("Invalid criteria provided.", ConsoleColor.Red);
            return;
        }

        if (monsters == null || monsters.Count == 0)
        {
            _outputManager.WriteLine("No monsters found.", ConsoleColor.Red);
            return;
        }

        _inputManager.PaginateList(monsters);
    }
}
