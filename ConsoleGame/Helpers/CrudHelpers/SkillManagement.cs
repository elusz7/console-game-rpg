using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers.CrudHelpers;

public class SkillManagement
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly SkillDao _skillDao;
    private readonly MonsterDao _monsterDao;
    private readonly ArchetypeDao _archetypeDao;

    public SkillManagement(InputManager inputManager, OutputManager outputManager, SkillDao skillDao, MonsterDao monsterDao, ArchetypeDao archetypeDao)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _skillDao = skillDao;
        _monsterDao = monsterDao;
        _archetypeDao = archetypeDao;
    }

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Skill Management Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Add Skill"
                + "\n2. Edit Skill"
                + "\n3. Assign Skills"
                + "\n4. Unassign Skills"
                + "\n5. Delete Skill"
                + "\n6. Back to Skill Menu");

            var choice = _inputManager.ReadMenuKey(6);
            switch (choice)
            {
                case 1:
                    AddSkill();
                    break;
                case 2:
                    EditSkill();
                    break;
                case 3:
                    AssignSkill();
                    break;
                case 4:
                    UnassignSkill();
                    break;
                case 5:
                    DeleteSkill();
                    break;
                case 6:
                    return;
            }
        }
    }

    private void AddSkill()
    {
        do
        {
            CreateSkill();
        } while (_inputManager.ConfirmAction("create"));
    }

    private void CreateSkill()
    {
        _outputManager.WriteLine("=== Create Skill ===", ConsoleColor.Cyan);

        var skillTypes = new[] { "MartialSkill", "MagicSkill", "SupportSkill", "UltimateSkill", "BossSkill" };
        var skillType = _inputManager.PaginateList(skillTypes.ToList(), "skill type", "create", true, false);
        if (skillType == null)
        {
            _outputManager.WriteLine("No skill type selected. Returning to menu.", ConsoleColor.Red);
            return;
        }

        var skillCategory = skillType switch
        {
            "Support Skill" => SkillCategory.Support,
            "Ultimate Skill" => SkillCategory.Ultimate,
            "Boss Skill" => SkillCategory.Ultimate,
            _ => SkillCategory.Basic
        };

        DamageType? damageType = skillType switch
        {
            "Martial Skill" => DamageType.Martial,
            "Magic Skill" => DamageType.Magical,
            "Ultimate Skill" => DamageType.Hybrid,
            _ => null
        };

        var name = _inputManager.ReadString("Enter skill name: ");
        var description = _inputManager.ReadString("Enter skill description: ");
        var level = _inputManager.ReadInt("Enter skill level: ", 10);
        var cost = _inputManager.ReadInt("Enter skill resource (mana/stamina) cost: ");
        var cooldown = _inputManager.ReadInt("Enter skill cooldown: ");

        var targetTypes = Enum.GetValues(typeof(TargetType)).Cast<TargetType>().ToList();
        TargetType? targetType;
        do
        {
            targetType = skillType.Equals("BossSkill") ? TargetType.SingleEnemy : _inputManager.PaginateList(targetTypes, "target type", "create", true, false);
            if (targetType == null)
            {
                _outputManager.WriteLine("No target type selected. Returning to menu.", ConsoleColor.Red);
                return;
            }
            else if (targetType == TargetType.Self && !skillType.Equals("SupportSkill"))
            {
                _outputManager.WriteLine("Self target type is only valid for Support Skills. Please select a different target type.", ConsoleColor.Red);
            }
            else
            {
                break;
            }
        } while (true);

        var prompt = skillType == "SupportSkill" ? "Enter stat boost/reduction power: " : "Enter skill damage: ";
        var power = _inputManager.ReadInt(prompt);

        int duration;
        StatType? statEffected;
        SupportEffectType? supportEffect;

        if (skillType == "SupportSkill")
        {
            duration = _inputManager.ReadInt("Enter skill duration: ");
            var stats = Enum.GetValues(typeof(StatType)).Cast<StatType>().ToList();
            do
            {
                statEffected = _inputManager.PaginateList(stats, "stat type", "create", true, false);

                if (statEffected == null)
                {
                    _outputManager.WriteLine("No stat type selected. Returning to menu.", ConsoleColor.Red);
                    return;
                }
                else if (statEffected == StatType.Health && targetType != TargetType.Self)
                {
                    _outputManager.WriteLine("Health is not a valid option for a support skill targeting enemies. Please select a different stat type.", ConsoleColor.Red);
                }
                else
                {
                    break;
                }
            } while (true);
            supportEffect = targetType == TargetType.Self ? SupportEffectType.Boost : SupportEffectType.Reduce;
        }
        else
        {
            duration = 0;
            statEffected = 0;
            supportEffect = 0;
        }

        var skill = skillType switch
        {
            "SupportSkill" => new SupportSkill
            {
                Name = name,
                SkillCategory = skillCategory,
                SkillType = skillType,
                Description = description,
                RequiredLevel = level,
                Cost = cost,
                Cooldown = cooldown,
                TargetType = (TargetType)targetType,
                StatAffected = (StatType)statEffected,
                SupportEffect = (int)supportEffect,
                Duration = duration
            },
            "UltimateSkill" => new UltimateSkill
            {
                Name = name,
                SkillCategory = skillCategory,
                SkillType = skillType,
                Description = description,
                RequiredLevel = level,
                Cost = cost,
                Cooldown = cooldown,
                TargetType = (TargetType)targetType,
                DamageType = damageType,
                Power = power
            },
            "BossSkill" => new BossSkill
            {
                Name = name,
                SkillCategory = skillCategory,
                SkillType = skillType,
                Description = description,
                RequiredLevel = level,
                Cost = cost,
                Cooldown = cooldown,
                TargetType = (TargetType)targetType,
                DamageType = damageType,
                Power = power
            },
            _ => new Skill
            {
                Name = name,
                SkillCategory = skillCategory,
                SkillType = skillType,
                Description = description,
                RequiredLevel = level,
                Cost = cost,
                Cooldown = cooldown,
                TargetType = (TargetType)targetType,
                DamageType = damageType,
                Power = power
            },
        };

        _skillDao.AddSkill(skill);
        _outputManager.WriteLine($"Skill [{name}] added successfully!", ConsoleColor.Green);
    }

    private void EditSkill()
    {
        var skills = _skillDao.GetAllNonCoreSkills();

        if (skills.Count == 0)
        {
            _outputManager.WriteLine("No skills available for editing.", ConsoleColor.Red);
            return;
        }

        var skill = _inputManager.PaginateList(skills, "skill", "edit", true, false);

        if (skill == null)
        {
            _outputManager.WriteLine("\nNo skill selected for editing.\n", ConsoleColor.Red);
            return;
        }

        var propertyActions = new Dictionary<string, Action>
        {
            { "Name", () => skill.Name = _inputManager.ReadString("\nEnter new value for Name: ") },
            { "Level", () => skill.RequiredLevel = _inputManager.ReadInt("\nEnter new value for required Level: ") },
            { "Power", () => skill.Power = _inputManager.ReadInt("\nEnter new value for skill power: ") },
            { "Cost", () => skill.Cost = _inputManager.ReadInt("\nEnter new value for skill cost: ") },
            { "Cooldown", () => skill.Cooldown = _inputManager.ReadInt("\nEnter new value for skill cooldown: ") },
            { "Description", () => skill.Description = _inputManager.ReadString("\nEnter new value for skill description: ") },
            { "Target Type", () =>
                {
                    var targetType = _inputManager.PaginateList(Enum.GetValues(typeof(TargetType)).Cast<TargetType>().ToList(), "target type", "edit", true, false);
                    if (targetType == null)
                    {
                        _outputManager.WriteLine("No target type selected.", ConsoleColor.Red);
                    }
                    else if (targetType == TargetType.Self && skill.SkillType != "SupportSkill")
                    {
                        _outputManager.WriteLine("Self target type is only valid for Support Skills. Please select a different target type.", ConsoleColor.Red);
                    }
                    else
                    {
                        skill.TargetType = targetType;
                    }
                }},
            { "Support Stat Affected", () =>
                {
                    var statType = _inputManager.PaginateList(Enum.GetValues(typeof(StatType)).Cast<StatType>().ToList(), "stat type", "edit", true, false);

                    if (statType == null)
                    {
                        _outputManager.WriteLine("No stat type selected.", ConsoleColor.Red);
                    }
                    else if (statType == StatType.Health && skill.TargetType != TargetType.Self)
                    {
                        _outputManager.WriteLine("Health is not a valid option for a support skill targeting enemies. Please select a different stat type.", ConsoleColor.Red);
                    }
                    else
                    {
                        ((SupportSkill)skill).StatAffected = statType;
                    }

                }},
            { "Support Effect Type", () => ((SupportSkill)skill).SupportEffect = (int)_inputManager.PaginateList(Enum.GetValues(typeof(SupportEffectType)).Cast<SupportEffectType>().ToList(), "support effect", "edit", true, false) }
        };

        while (true)
        {
            _outputManager.WriteLine($"\nEditing skill: {skill.Name}", ConsoleColor.Cyan);
            _outputManager.WriteLine($"{skill}\n", ConsoleColor.Green);

            int option = DisplayEditMenu(propertyActions.Keys.ToList());

            if (option == propertyActions.Count + 1)
            {
                _skillDao.UpdateSkill(skill);
                _outputManager.WriteLine($"\nExiting. Any changes made have been successfully saved to {skill.Name}\n", ConsoleColor.Green);
                return;
            }

            string selectedProperty = propertyActions.Keys.ElementAt(option - 1);
            propertyActions[selectedProperty].Invoke();
        }
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
    private void AssignSkill()
    {
        do
        {
            var skills = _skillDao.GetUnassignedNonCoreSkills();
            if (skills.Count == 0)
            {
                _outputManager.WriteLine("No skills available for assignment.", ConsoleColor.Red);
                return;
            }
        
            var skill = _inputManager.PaginateList(skills, "skill", "assign", true, false);
            if (skill == null)
            {
                _outputManager.WriteLine("\nNo skill selected for assignment.\n", ConsoleColor.Red);
                return;
            }

            var assignment = _inputManager.ReadString("Assign skill to monster or archetype? (m/a): ", ["m", "a"]).ToLower();

            var target = assignment switch
            {
                "m" => _inputManager.PaginateList(_monsterDao.GetAllMonsters(), "monster", "assign", true, false),
                "a" => _inputManager.PaginateList(_archetypeDao.GetAllArchetypes(), "archetype", "assign", true, false),
                _ => null as object
            };

            if (target == null)
            {
                _outputManager.WriteLine("\nNo target selected for assignment.\n", ConsoleColor.Red);
                return;
            }

            if (target is Monster m)
            {
                skill.MonsterId = m.Id;
                _skillDao.UpdateSkill(skill);
            }
            else if (target is Archetype a)
            {
                skill.ArchetypeId = a.Id;
                _skillDao.UpdateSkill(skill);
            }

        } while(_inputManager.LoopAgain("assign"));
    }
    private void UnassignSkill()
    {
        var skills = _skillDao.GetAllAssignedNonCoreSkills();

        if (skills.Count == 0)
        {
            _outputManager.WriteLine("No skills available for unassignment.", ConsoleColor.Red);
            return;
        }

        var skill = _inputManager.PaginateList(skills, "skill", "unassign", true, false);
        if (skill == null)
        {
            _outputManager.WriteLine("\nNo skill selected for unassignment.\n", ConsoleColor.Red);
            return;
        }

        if (!_inputManager.ConfirmAction($"unassignment"))
        {
            _outputManager.WriteLine("Unassignment cancelled.", ConsoleColor.Red);
            return;
        }

        skill.ArchetypeId = null;
        skill.Archetype = null;
        skill.MonsterId = null;
        skill.Monster = null;

        _skillDao.UpdateSkill(skill);
        _outputManager.WriteLine($"\n{skill.Name} has been unassigned successfully.\n", ConsoleColor.Green);
    }
    private void DeleteSkill()
    {
        var skills = _skillDao.GetAllNonCoreSkills();
        if (skills.Count == 0)
        {
            _outputManager.WriteLine("No skills available for deletion.", ConsoleColor.Red);
            return;
        }
        var skill = _inputManager.PaginateList(skills, "skill", "delete", true, false);
        if (skill == null)
        {
            _outputManager.WriteLine("\nNo skill selected for deletion.\n", ConsoleColor.Red);
            return;
        }

        if (skill.MonsterId != null || skill.ArchetypeId != null)
        {
            var assigned = skill.MonsterId == null ? skill.Archetype.Name : skill.Monster.Name;
            _outputManager.WriteLine($"warning: skill is currently assigned to {assigned}.", ConsoleColor.Red);
            return;
        }

        if (!_inputManager.ConfirmAction($"deletion"))
        {
            _outputManager.WriteLine("Deletion cancelled.", ConsoleColor.Red);
            return;
        }
        _skillDao.DeleteSkill(skill);
        _outputManager.WriteLine($"\n{skill.Name} has been deleted successfully.\n", ConsoleColor.Green);
    }
}
