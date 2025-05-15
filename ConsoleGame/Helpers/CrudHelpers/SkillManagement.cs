    using System.Reflection.PortableExecutable;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Managers.CrudHelpers;

public class SkillManagement(InputManager inputManager, OutputManager outputManager, SkillDao skillDao, MonsterDao monsterDao, ArchetypeDao archetypeDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly SkillDao _skillDao = skillDao;
    private readonly MonsterDao _monsterDao = monsterDao;
    private readonly ArchetypeDao _archetypeDao = archetypeDao;

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
        _outputManager.WriteLine("=== Create Skill ===", ConsoleColor.Magenta);

        var skillType = GetEnumChoice<SkillType>("Select the type of skill to create");

        var numericValues = new Dictionary<string, int>
        {
            [SkillKeys.CATEGORY] = skillType switch
            {
                SkillType.SupportSkill => (int)SkillCategory.Support,
                SkillType.UltimateSkill or SkillType.BossSkill => (int)SkillCategory.Ultimate,
                _ => (int)SkillCategory.Basic
            },
            [SkillKeys.DAMAGE] = skillType switch
            {
                SkillType.MartialSkill => (int)DamageType.Martial,
                SkillType.MagicSkill => (int)DamageType.Magical,
                SkillType.UltimateSkill or SkillType.BossSkill => (int)DamageType.Hybrid,
                _ => -1
            }
        };

        var name = _inputManager.ReadString("Enter skill name: ").Trim();
        var description = _inputManager.ReadString("Enter skill description: ").Trim();

        GetValidTargetType(skillType, numericValues);

        var powerPrompt = numericValues[SkillKeys.CATEGORY] == (int)SkillCategory.Support
            ? "\nEnter stat boost/reduction power: "
            : "\nEnter skill damage: ";

        numericValues[SkillKeys.POWER] = _inputManager.ReadInt(powerPrompt);

        if (skillType == SkillType.SupportSkill)
        {
            GetValidSupportStat(numericValues);
            numericValues[SkillKeys.EFFECT] = numericValues[SkillKeys.TARGET] == (int)TargetType.Self
                ? (int)SupportEffectType.Boost
                : (int)SupportEffectType.Reduce;
        }

        SkillAdjustment(numericValues, powerPrompt);

        if (!_inputManager.ConfirmAction("creation"))
        {
            _outputManager.WriteLine("\nSkill creation cancelled.\n", ConsoleColor.Red);
            return;
        }    

        var newSkill = CreateSkillObject(skillType, name, description, numericValues);
        _skillDao.AddSkill(newSkill);

        _outputManager.WriteLine($"Skill [{name}] added successfully!", ConsoleColor.Green);
    }
    private void GetValidTargetType(SkillType skillType, Dictionary<string, int> numericValues)
    {
        while (true)
        {
            var target = skillType == SkillType.BossSkill
                ? (int)TargetType.SingleEnemy
                : (int)GetEnumChoice<TargetType>("Select the target of this skill");

            if (target == (int)TargetType.Self && skillType != SkillType.SupportSkill)
            {
                _outputManager.WriteLine("\nSelf target type is only valid for Support Skills. Please select a different target type.", ConsoleColor.Red);
            }
            else
            {
                numericValues[SkillKeys.TARGET] = target;
                break;
            }
        }
    }
    private void GetValidSupportStat(Dictionary<string, int> numericValues)
    {
        while (true)
        {
            StatType stat = GetEnumChoice<StatType>("Select the stat to affect");

            if (stat == StatType.Health && numericValues[SkillKeys.TARGET] != (int)TargetType.Self)
            {
                _outputManager.WriteLine("\nHealth is not a valid option for a support skill targeting enemies. Please select a different stat type.", ConsoleColor.Red);
            }
            else
            {
                numericValues[SkillKeys.STAT] = (int)stat;
                break;
            }
        }
    }
    private T GetEnumChoice<T>(string prompt) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();

        for (int i = 0; i < values.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {values[i]}");
        }

        var choice = _inputManager.ReadInt($"\n{prompt}: ", values.Count);
        return values[choice - 1];
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
            { "Power (Will Also Affect Level, Cost, Cooldown, and Duration) ", () => 
                {
                    var powerPrompt = skill.SkillCategory == SkillCategory.Support ? "\nEnter stat boost/reduction power: " : "\nEnter skill damage: ";
                    var power = _inputManager.ReadInt(powerPrompt);

                    StatType? statEffected = null;
                    if (skill is SupportSkill support)
                        statEffected = support.StatAffected;

                    var adjustments = new Dictionary<string, int>();

                    SkillAdjustment(adjustments, powerPrompt);

                    adjustments.TryGetValue("power", out power);
                    adjustments.TryGetValue("level", out var level);
                    adjustments.TryGetValue("cost", out var cost);
                    adjustments.TryGetValue("cooldown", out var cooldown);
                    adjustments.TryGetValue("duration", out var duration);

                    skill.Power = power;
                    skill.RequiredLevel = level;
                    skill.Cost = cost;
                    skill.Cooldown = cooldown;
                    if (skill is SupportSkill supportSkill)
                        supportSkill.Duration = duration;
                }},
            { "Description", () => skill.Description = _inputManager.ReadString("\nEnter new value for skill description: ") },
            { "Target Type", () =>
                {
                    var targetType = GetEnumChoice<TargetType>("Select the new target");

                    if (skill is SupportSkill supportSkill)
                    {
                        if (targetType == TargetType.Self)
                            supportSkill.SupportEffect = (int)SupportEffectType.Boost;
                        else if (targetType != TargetType.Self && supportSkill.StatAffected == StatType.Health)
                        {
                            _outputManager.WriteLine("\nCannot target enemies with a support skill affected health!\n", ConsoleColor.Red);
                            return;
                        }
                        else
                            supportSkill.SupportEffect |= (int)SupportEffectType.Reduce;

                        skill.TargetType = targetType;
                    }
                    else
                    {
                        if (targetType == TargetType.Self)
                        {
                            _outputManager.WriteLine("\nCannot target yourself with damaging skills!\n", ConsoleColor.Red);
                            return;
                        }

                        skill.TargetType = targetType;
                    }
                }},
            { "Support Stat Affected", () =>
                {                    
                    if (skill is SupportSkill supportSkill)
                    {
                        var statType = GetEnumChoice<StatType>("Select the new stat to affect");

                        if (statType == StatType.Health && skill.TargetType != TargetType.Self)
                        {
                            _outputManager.WriteLine("Health is not a valid option for a support skill targeting enemies. Please select a different stat type.", ConsoleColor.Red);
                            return;
                        }
                        supportSkill.StatAffected = statType;
                    }
                    else
                        _outputManager.WriteLine("This skill is not a SupportSkill!", ConsoleColor.Red);
                }}
        };

        while (true)
        {
            _outputManager.WriteLine($"\nEditing skill: {skill.Name}", ConsoleColor.Cyan);
            _outputManager.WriteLine($"{skill}\n", ConsoleColor.Green);

            int option = DisplayEditMenu([.. propertyActions.Keys]);

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
        _outputManager.WriteLine($"{properties.Count + 1}. Save and Exit", ConsoleColor.Red);

        return _inputManager.ReadInt("\nWhat property would you like to edit? ", properties.Count + 1);
    }
    private void AssignSkill()
    {
        do
        {
            var skills = _skillDao.GetUnassignedNonCoreSkills();
            if (skills.Count == 0)
            {
                _outputManager.WriteLine("\nNo skills available for assignment.\n", ConsoleColor.Red);
                return;
            }

            var skill = _inputManager.PaginateList(skills, "skill", "assign", true, false);
            if (skill == null)
            {
                _outputManager.WriteLine("\nNo skill selected for assignment.\n", ConsoleColor.Red);
                return;
            }

            var assignment = _inputManager.ReadString("\nAssign skill to monster or archetype? (m/a): ", ["m", "a"]).ToLower();

            object? target = assignment switch
            {
                "m" => _inputManager.PaginateList(_monsterDao.GetAllMonsters(), "monster", "assign", true, false),
                "a" => _inputManager.PaginateList(_archetypeDao.GetAllArchetypes(), "archetype", "assign", true, false),
                _ => null
            };

            if (target == null)
            {
                _outputManager.WriteLine("\nNo target selected for assignment.\n", ConsoleColor.Red);
                return;
            }

            if (target is Monster monster)
            {
                if (skill is BossSkill && monster is not BossMonster)
                {
                    _outputManager.WriteLine("\nYou cannot assign a boss-level skill to a non-boss monster.\n", ConsoleColor.Red);
                    return;
                }

                if (skill is UltimateSkill && monster is not EliteMonster)
                {
                    _outputManager.WriteLine("\nYou cannot assign ultimate skills to a non-elite monster.\n", ConsoleColor.Red);
                    return;
                }

                if (skill.TargetType == TargetType.AllEnemies)
                {
                    _outputManager.WriteLine("\nCannot assign a skill that affects multiple targets to a monster.\n", ConsoleColor.Red);
                    return;
                }

                if ((skill.DamageType.Equals("Martial") && monster.DamageType.Equals("Magic")) ||
                    (skill.DamageType.Equals("Magic") && monster.DamageType.Equals("Martial")))
                {
                    _outputManager.WriteLine($"\nYou cannot assign a {skill.DamageType} skill to a {monster.DamageType} monster.\n", ConsoleColor.Red);
                    return;
                }

                skill.MonsterId = monster.Id;
                _skillDao.UpdateSkill(skill);
                _outputManager.WriteLine($"\n{skill.Name} successfully assigned to {monster.Name}!", ConsoleColor.Green);
            }
            else if (target is Archetype archetype)
            {
                if ((skill.DamageType.Equals("Martial") && archetype.ArchetypeType.Equals("Magic")) ||
                    (skill.DamageType.Equals("Magic") && archetype.ArchetypeType.Equals("Martial")))
                {
                    _outputManager.WriteLine($"\nYou cannot assign a {skill.DamageType} skill to a {archetype.ArchetypeType} archetype.\n", ConsoleColor.Red);
                    return;
                }

                skill.ArchetypeId = archetype.Id;
                _skillDao.UpdateSkill(skill);
                _outputManager.WriteLine($"\n{skill.Name} successfully assigned to {archetype.Name}!", ConsoleColor.Green);
            }

        } while (_inputManager.LoopAgain("assign"));
    }
    private void UnassignSkill()
    {
        var skills = _skillDao.GetAllAssignedNonCoreSkills();

        if (skills.Count == 0)
        {
            _outputManager.WriteLine("\nNo skills available for unassignment.\n", ConsoleColor.Red);
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
            _outputManager.WriteLine("\nUnassignment cancelled.\n", ConsoleColor.Red);
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
            var assigned = skill.MonsterId == null ? skill.Archetype?.Name : skill.Monster?.Name;
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
    private void SkillAdjustment(Dictionary<string, int> numericValues, string powerPrompt)
    {
        var power = numericValues[SkillKeys.POWER];

        while (true)
        {
            switch ((SkillCategory)numericValues[SkillKeys.CATEGORY])
            {
                case SkillCategory.Support:
                    ApplySupportScaling(numericValues, power);
                    break;

                case SkillCategory.Ultimate:
                    ApplyUltimateScaling(numericValues, power);
                    break;

                default:
                    ApplyDefaultScaling(numericValues, power);
                    break;
            }

            var output = "\nBased on the provided power, these stats have been automatically generated:\n" +
                         $"Required Level: {numericValues[SkillKeys.LEVEL]}, Resource Cost: {numericValues[SkillKeys.COST]}, Cooldown: {numericValues[SkillKeys.COOLDOWN]}";

            output += numericValues.TryGetValue(SkillKeys.DURATION, out var duration)
                ? $", Duration: {duration}"
                : "";

            output += "\n";
            _outputManager.WriteLine(output, ConsoleColor.Magenta);

            var confirm = _inputManager.ReadString("Would you like to adjust the power (y/n): ", ["y", "n"]).ToLower();
            if (confirm == "n")
            {
                numericValues[SkillKeys.POWER] = power;
                break;
            }

            power = _inputManager.ReadInt(powerPrompt);
        }
    }
    private static void ApplySupportScaling(Dictionary<string, int> values, int power)
    {
        if ((StatType)values[SkillKeys.STAT] == StatType.Health)
        {
            values[SkillKeys.LEVEL] = Math.Max(1, power / 10); // 10 health = 1 level
            values[SkillKeys.COST] = Math.Clamp((int)(power / 10.0 * 3), 2, 50); // 10 health = cost 3
            values[SkillKeys.COOLDOWN] = Math.Clamp((int)(power / 10.0 * 4), 3, 60); // 10 health = cooldown 4
            values.Remove(SkillKeys.DURATION); // No duration for healing
        }
        else
        {
            values[SkillKeys.LEVEL] = Math.Max(1, power / 2); // 2 power = 1 level
            values[SkillKeys.COST] = Math.Clamp((int)(power / 3.0 * 2), 2, 50); // 3 power = cost 2
            values[SkillKeys.COOLDOWN] = Math.Clamp((int)(power / 2.0), 3, 60); // 2 power = cooldown 1
            values[SkillKeys.DURATION] = Math.Clamp((int)(values[SkillKeys.COOLDOWN] * 0.43), 1, values[SkillKeys.COOLDOWN] - 1); // duration ~43% of cooldown
        }
    }
    private static void ApplyUltimateScaling(Dictionary<string, int> values, int power)
    {
        values[SkillKeys.LEVEL] = Math.Max(3, power / 15); // 15 power = level 1
        values[SkillKeys.COST] = Math.Clamp((int)(power / 20.0), 10, 100); // 20 power = cost 1
        values[SkillKeys.COOLDOWN] = Math.Clamp((int)(power / 15.0), 8, 80); // 15 power = cooldown 1
        values.Remove(SkillKeys.DURATION); // No duration for ultimates
    }
    private static void ApplyDefaultScaling(Dictionary<string, int> values, int power)
    {
        values[SkillKeys.LEVEL] = Math.Max(1, power / 10); // 10 power = level 1
        values[SkillKeys.COST] = Math.Clamp((int)(power / 8.0), 3, 50); // 8 power = cost 1
        values[SkillKeys.COOLDOWN] = Math.Clamp((int)(power / 12.0), 3, 50); // 12 power = cooldown 1
        values.Remove(SkillKeys.DURATION); // Default doesn't use duration
    }
    private static Skill CreateSkillObject(SkillType skillType, string name, string description, Dictionary<string, int> numericValues)
    {
        var skillCategory = (SkillCategory)numericValues[SkillKeys.CATEGORY];
        var targetType = (TargetType)numericValues[SkillKeys.TARGET];        
        var power = numericValues[SkillKeys.POWER];
        var level = numericValues[SkillKeys.LEVEL];
        var cost = numericValues[SkillKeys.COST];
        var cooldown = numericValues[SkillKeys.COOLDOWN];

        var damage = numericValues[SkillKeys.DAMAGE];
        DamageType? damageType = damage == -1 ? null : (DamageType)damage;

        StatType statEffected = StatType.Health;
        if (numericValues.TryGetValue(SkillKeys.STAT, out var stat))
            statEffected = (StatType)stat;

        numericValues.TryGetValue(SkillKeys.EFFECT, out var supportEffect);
        numericValues.TryGetValue(SkillKeys.DURATION, out var duration);

        return skillType switch
        {
            SkillType.SupportSkill => new SupportSkill
            {
                Name = name,
                SkillCategory = skillCategory,
                SkillType = skillType.ToString(),
                Description = description,
                RequiredLevel = level,
                Cost = cost,
                Cooldown = cooldown,
                TargetType = targetType,
                StatAffected = statEffected,
                SupportEffect = supportEffect,
                Duration = duration
            },
            SkillType.UltimateSkill => new UltimateSkill
            {
                Name = name,
                SkillCategory = skillCategory,
                SkillType = skillType.ToString(),
                Description = description,
                RequiredLevel = level,
                Cost = cost,
                Cooldown = cooldown,
                TargetType = targetType,
                DamageType = damageType,
                Power = power
            },
            SkillType.BossSkill => new BossSkill
            {
                Name = name,
                SkillCategory = skillCategory,
                SkillType = skillType.ToString(),
                Description = description,
                RequiredLevel = level,
                Cost = cost,
                Cooldown = cooldown,
                TargetType = targetType,
                DamageType = damageType,
                Power = power
            },
            _ => new Skill
            {
                Name = name,
                SkillCategory = skillCategory,
                SkillType = skillType.ToString(),
                Description = description,
                RequiredLevel = level,
                Cost = cost,
                Cooldown = cooldown,
                TargetType = targetType,
                DamageType = damageType,
                Power = power
            },
        };
    }
}
