using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Managers.CrudHelpers;

public class MonsterManagement(OutputManager outputManager, InputManager inputManager, MonsterDao monsterDao)
{
    private readonly OutputManager _outputManager = outputManager;
    private readonly InputManager _inputManager = inputManager;
    private readonly MonsterDao _monsterDao = monsterDao;

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

            var threatLevels = Enum.GetValues(typeof(ThreatLevel)).Cast<ThreatLevel>().ToList();
            foreach (var threat in threatLevels)
            {
                _outputManager.WriteLine($"{(int)threat + 1}: {threat}");
            }
            int threatLevel = _inputManager.ReadInt("Select a Threat Level: ", threatLevels.Max(t => (int)t)) - 1;

            string monsterType = threatLevel switch
            {
                3 => "EliteMonster",
                4 => "BossMonster",
                _ => "Monster"
            };

            var damageTypes = Enum.GetValues(typeof(DamageType)).Cast<DamageType>().ToList();
            foreach (var type in damageTypes)
            {
                _outputManager.WriteLine($"{(int)type + 1}: {type}");
            }
            int damage = _inputManager.ReadInt("Select a Damage Type: ", damageTypes.Max(d => (int)d)) - 1;

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
                ThreatLevel = (ThreatLevel)threatLevel,
                MonsterType = monsterType,
                DamageType = (DamageType)damage
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

        var monsterToEdit = _inputManager.PaginateList(monsters, "monster", "edit", true, false);

        if (monsterToEdit == null)
        {
            _outputManager.WriteLine("\nNo monster selected for editing.\n", ConsoleColor.Red);
            return;
        }

        var threatLevels = Enum.GetValues(typeof(ThreatLevel)).Cast<ThreatLevel>().ToList();
        var damageTypes = Enum.GetValues(typeof(DamageType)).Cast<DamageType>().ToList();

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
                    monsterToEdit.ThreatLevel =
                        (ThreatLevel)_inputManager.ReadInt("\nEnter new value for Threat Level: ", threatLevels.Max(t => (int)t)) - 1;
                    monsterToEdit.MonsterType = monsterToEdit.ThreatLevel switch
                    {
                        ThreatLevel.Elite => "EliteMonster",
                        ThreatLevel.Boss => "BossMonster",
                        _ => "Monster"
                    };
                }},
            { "Damage Type", () => monsterToEdit.DamageType = (DamageType)_inputManager.ReadInt("\nEnter new value for Damage Type: ", damageTypes.Max(d => (int)d)) - 1 },
            { "Health", () => monsterToEdit.MaxHealth = _inputManager.ReadInt("\nEnter new value for Health: ") }
        };

        while (true)
        {
            _outputManager.Clear();
            _outputManager.WriteLine($"Editing Monster: {monsterToEdit.Name}", ConsoleColor.Cyan);
            _outputManager.WriteLine($"{monsterToEdit}\n", ConsoleColor.Green);
            int option = DisplayEditMenu([.. propertyActions.Keys]);

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
        var monsterToDelete = _inputManager.PaginateList(monsters, "monster", "delete", true, false);
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
    private int DisplayEditMenu(List<string> properties)
    {
        for (int i = 0; i < properties.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. Change {properties[i]}");
        }
        _outputManager.WriteLine($"{properties.Count + 1}. Exit", ConsoleColor.Red);

        return _inputManager.ReadInt("\nWhat property would you like to edit? ", properties.Count + 1);
    }
}
