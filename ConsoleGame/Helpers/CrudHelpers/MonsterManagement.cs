using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers.CrudHelpers;

public class MonsterManagement(IOutputManager outputManager, IInputManager inputManager, IMonsterDao monsterDao)
{
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IInputManager _inputManager = inputManager;
    private readonly IMonsterDao _monsterDao = monsterDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Monster Management Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Add Monster"
                + "\n2. Edit Monster"
                + "\n3. Delete Monster"
                + "\n4. Return to Monster Menu");
            var input = _inputManager.ReadMenuKey(4);
            switch (input)
            {
                case 1:
                    AddMonster();
                    break;
                case 2:
                    EditMonster();
                    break;
                case 3:
                    DeleteMonster();
                    break;
                case 4:
                    _outputManager.Clear();
                    return;
            }
        }
    }

    private void AddMonster()
    {
        do
        {
            _outputManager.Clear();
            _outputManager.WriteLine("=== Creating Monster ===", ConsoleColor.Cyan);
            string name = _inputManager.ReadString("Enter monster name: ");

            string description = _inputManager.ReadString("Enter monster description: ");

            int level = _inputManager.ReadInt("Enter monster level: ");

            int health = _inputManager.ReadInt("Enter monster health: ");

            int attackPower = _inputManager.ReadInt("Enter monster attack power: ");

            int defensePower = _inputManager.ReadInt("Enter monster defense power: ");

            int resistancePower = _inputManager.ReadInt("Enter monster resistance power: ");

            int speed = _inputManager.ReadInt("Enter monster speed: ");

            ThreatLevel threatLevel = _inputManager.GetEnumChoice<ThreatLevel>("Select a Threat Level");

            string monsterType = threatLevel switch
            {
                ThreatLevel.Elite => "EliteMonster",
                ThreatLevel.Boss => "BossMonster",
                _ => "Monster"
            };

            DamageType damageType = _inputManager.GetEnumChoice<DamageType>("Select a Damage Type");

            var monster = new Monster
            {
                Name = name,
                Description = description,
                Level = level,
                MaxHealth = health,
                AttackPower = attackPower,
                DefensePower = defensePower,
                Resistance = resistancePower,
                AggressionLevel = speed,
                ThreatLevel = threatLevel,
                MonsterType = monsterType,
                DamageType = damageType
            };

            _monsterDao.AddMonster(monster);
        } while (_inputManager.LoopAgain("create"));
    }

    private void EditMonster()
    {
        var monsters = _monsterDao.GetAllNonCoreMonsters();

        if (monsters == null || monsters.Count == 0)
        {
            _outputManager.WriteLine("\nNo monsters available to edit.\n", ConsoleColor.Red);
            return;
        }

        var monsterToEdit =
            _inputManager.Selector(
                monsters,
                m => ColorfulToStringHelper.MonsterToString(m),
                "Select a monster to edit",
                t => ColorfulToStringHelper.GetMonsterColor(t));

        if (monsterToEdit == null)
        {
            _outputManager.WriteLine("\nNo monster selected for editing.\n", ConsoleColor.Red);
            return;
        }

        var propertyActions = new Dictionary<string, Action>
        {
            { "Name", () => monsterToEdit.Name = _inputManager.ReadString("\nEnter new value for Name: ") },
            { "Description", () => monsterToEdit.Description = _inputManager.ReadString("\nEnter new value for Description: ") },
            { "Level", () => monsterToEdit.Level = _inputManager.ReadInt("\nEnter new value for Level: ") },
            { "Attack Power", () => monsterToEdit.AttackPower = _inputManager.ReadInt("\nEnter new value for Attack Power: ") },
            { "Defense Power", () => monsterToEdit.DefensePower = _inputManager.ReadInt("\nEnter new value for Defense Power: ") },
            { "Resistance Power", () => monsterToEdit.Resistance = _inputManager.ReadInt("\nEnter new value for Resistance Power: ") },
            { "Speed", () => monsterToEdit.AggressionLevel = _inputManager.ReadInt("\nEnter new value for Speed: ") },
            { "Threat Level", () => {
                    monsterToEdit.ThreatLevel = _inputManager.GetEnumChoice<ThreatLevel>("Select the new Threat Level");
                    monsterToEdit.MonsterType = monsterToEdit.ThreatLevel switch
                    {
                        ThreatLevel.Elite => "EliteMonster",
                        ThreatLevel.Boss => "BossMonster",
                        _ => "Monster"
                    };
                }},
            { "Damage Type", () => monsterToEdit.DamageType = _inputManager.GetEnumChoice<DamageType>("Select the new DamageType") },
            { "Health", () => monsterToEdit.MaxHealth = _inputManager.ReadInt("\nEnter new value for Health: ") }
        };

        while (true)
        {
            _outputManager.Clear();
            _outputManager.WriteLine($"Editing Monster: {monsterToEdit.Name}", ConsoleColor.Cyan);
            _outputManager.WriteLine($"{monsterToEdit}\n", ConsoleColor.Green);
            int option = _inputManager.DisplayEditMenu([.. propertyActions.Keys]);

            if (option == propertyActions.Count + 1)
            {
                _monsterDao.UpdateMonster(monsterToEdit);
                _outputManager.WriteLine($"\nExiting. Any changes made have been successfully saved to {monsterToEdit.Name}\n", ConsoleColor.Green);
                return;
            }

            string selectedProperty = propertyActions.Keys.ElementAt(option - 1);
            propertyActions[selectedProperty].Invoke();
        }
    }
    private void DeleteMonster()
    {
        var monsters = _monsterDao.GetAllNonCoreMonsters();
        if (monsters == null || monsters.Count == 0)
        {
            _outputManager.WriteLine("\nNo monsters available to delete.\n", ConsoleColor.Red);
            return;
        }
        var monsterToDelete =
            _inputManager.Selector(
                monsters,
                m => ColorfulToStringHelper.MonsterToString(m),
                "Select a monster to delete",
                t => ColorfulToStringHelper.GetMonsterColor(t));

        if (monsterToDelete == null)
        {
            _outputManager.WriteLine("\nNo monster selected for deletion.\n", ConsoleColor.Red);
            return;
        }

        if (!_inputManager.ConfirmAction($"deletion"))
        {
            _outputManager.WriteLine("\nDeletion cancelled.\n", ConsoleColor.Red);
            return;
        }
        _monsterDao.DeleteMonster(monsterToDelete);
        _outputManager.WriteLine($"\n{monsterToDelete.Name} has been deleted successfully.\n", ConsoleColor.Green);
    }
}
