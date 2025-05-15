using ConsoleGame.GameDao;
using ConsoleGame.Managers;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public class MonsterFactory(MonsterDao monsterDao, SkillDao skilldao)
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
    private readonly MonsterDao _monsterDao = monsterDao;
    private readonly SkillDao _skillDao = skilldao;

    private static readonly Dictionary<ThreatLevel, double> _monsterThreatWeights = new()
        {
            { ThreatLevel.Low, 0.5 },
            { ThreatLevel.Medium, 1.0 },
            { ThreatLevel.High, 1.5 },
            { ThreatLevel.Elite, 2.5 }
        };

    // Dictionary to hold the number of monsters for each threat level in each campaign level
    private readonly Dictionary<int, Dictionary<ThreatLevel, int>> _campaignMonsters = Enumerable.Range(1, 20)
        .ToDictionary(level => level, level => GenerateMonstersForLevel(level));

    private static readonly Dictionary<string, int> NameCounts = [];

    public List<Monster> GenerateMonsters(int level, bool campaign)
    {
        var selectedMonsters = new List<Monster>();

        if (campaign)
        {
            if (_campaignMonsters.TryGetValue(level, out var monstersForLevel))
            {
                foreach (var kv in monstersForLevel)
                {
                    var threatLevel = kv.Key;
                    var monsterCount = kv.Value;
                    for (int i = 0; i < monsterCount; i++)
                    {
                        // Select a random monster and create a fresh instance
                        var availableMonsters = _monsterDao.GetMonstersByMaxLevelAndThreatLevel(level, (int)threatLevel);
                        var randomMonsterBase = availableMonsters[_rng.Next(availableMonsters.Count)];
                        var randomMonster = CreateMonster(randomMonsterBase, level);

                        selectedMonsters.Add(randomMonster);
                    }
                }
            }
        }
        else
        {
            var availableMonsters = _monsterDao.GetNonBossMonstersByMaxLevel(level);

            int targetMonsterCount = (int)Math.Round(level * 3.5);
            int currentCount = 0;

            while (currentCount < targetMonsterCount)
            {
                // Select a random monster and create a fresh instance
                var randomMonsterBase = availableMonsters[_rng.Next(availableMonsters.Count)];
                var randomMonster = CreateMonster(randomMonsterBase, level);                

                // Calculate weight based on monster threat level and relative level to player
                var weight = CalculateMonsterWeight(randomMonster, level);

                // Add the monster and increase the monster count by the calculated weight
                selectedMonsters.Add(randomMonster);
                currentCount += (int)Math.Round(weight);
            }
        }
        
        SetMonsterSkills(level, selectedMonsters);

        return selectedMonsters;
    }

    public static Monster CreateMonster(Monster monsterBase, int level)
    {
        var monster = new Monster
        {
            Name = GetMonsterName(monsterBase.Name),
            Level = monsterBase.Level,
            MaxHealth = monsterBase.MaxHealth,
            ThreatLevel = monsterBase.ThreatLevel,
            AggressionLevel = monsterBase.AggressionLevel,
            DefensePower = monsterBase.DefensePower,
            AttackPower = monsterBase.AttackPower,
            DamageType = monsterBase.DamageType,
            Resistance = monsterBase.Resistance,
            MonsterType = monsterBase.MonsterType,
            Description = monsterBase.Description,
            Strategy = Monster.GetStrategyFor((MonsterBehaviorType)_rng.Next(1, 6))
        };

        if (monster.Level < level)
        {
            monster.LevelUp(level);
        }

        monster.CurrentHealth = monster.MaxHealth;

        return monster;
    }
    private static string GetMonsterName(string baseName)
    {
        string[] romanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
        string[] modifiers =
        [
            "Fierce",
            "Arcane",
            "Twisted",
            "Burning",
            "Rotting",
            "Grizzled",
            "Cackling",
            "Frostbitten",
            "Shadowed",
            "Lucky",
            "Silent",
            "Enraged",
            "Giggling",
            "Eldritch",
            "Cracked",
            "Slippery",
            "Vicious",
            "Bloody",
            "Ancient",
            "Frantic"
        ];

        if (!NameCounts.ContainsKey(baseName))
            NameCounts[baseName] = 0;

        var newName = $"{baseName} {romanNumerals[NameCounts[baseName] % romanNumerals.Length]} the {modifiers[_rng.Next(modifiers.Length)]}";
        NameCounts[baseName]++;

        return newName;
    }
    private void SetMonsterSkills(int level, List<Monster> monsters)
    {
        var skills = _skillDao.GetUnassignedSkillsByMaxLevel(level);

        if (skills.Count == 0)
        {
            return;
        }

        var martialSkills = skills.Where(s => s is MartialSkill).ToList();
        var magicSkills = skills.Where(s => s is MagicSkill).ToList();
        var supportSkills = skills.Where(s => s is SupportSkill).ToList();
        var ultimateSkills = skills.Where(s => s is UltimateSkill).ToList();

        foreach (var monster in monsters)
        {
            if (monster is BossMonster)
                continue;

            int attackSkillCount = monster.Level + (monster.Level / 2); //1 + half the level # of skills 
            int supportSkillCount = monster.Level / 2; //1 skill per 2 levels

            var selectedSkills = new List<Skill>();
            
            var usableAttackSkills = monster.DamageType == DamageType.Martial ?
                martialSkills.Where(martialSkills => martialSkills.RequiredLevel <= monster.Level).ToList() :
                magicSkills.Where(magicSkills => magicSkills.RequiredLevel <= monster.Level).ToList();

            for (int i = 0; i < attackSkillCount; i++)
            {
                if (usableAttackSkills.Count == 0)
                {
                    break;
                }
                var randomSkill = usableAttackSkills[_rng.Next(usableAttackSkills.Count)];
                randomSkill.InitializeSkill(monster.Level);
                selectedSkills.Add(randomSkill);
                usableAttackSkills.Remove(randomSkill);
            }

            var usuableSupportSkills = supportSkills.Where(s => s.RequiredLevel <= monster.Level).ToList();

            for (int i = 0; i < supportSkillCount; i++)
            {
                if (usuableSupportSkills.Count == 0)
                {
                    break;
                }
                var randomSkill = usuableSupportSkills[_rng.Next(usuableSupportSkills.Count)];
                randomSkill.InitializeSkill(monster.Level);
                selectedSkills.Add(randomSkill);
                usuableSupportSkills.Remove(randomSkill);
            }

            if (monster.ThreatLevel == ThreatLevel.Elite)
            {
                var usableUltimateSkills = ultimateSkills.Where(s => s.RequiredLevel <= monster.Level).ToList();
                if (usableUltimateSkills.Count > 0)
                {
                    var randomSkill = usableUltimateSkills[_rng.Next(usableUltimateSkills.Count)];
                    randomSkill.InitializeSkill(monster.Level);
                    selectedSkills.Add(randomSkill);
                }
            }

            monster.Skills = selectedSkills;
        }
    }
    private static Dictionary<ThreatLevel, int> GenerateMonstersForLevel(int level)
    {
        var result = new Dictionary<ThreatLevel, int>();

        // Base values with level-based scaling logic
        if (level < 10)
        {
            result[ThreatLevel.Low] = Math.Min(10, 2 + (level / 2));
            result[ThreatLevel.Medium] = level >= 2 ? 1 + (level / 4) : 0;
            result[ThreatLevel.High] = level >= 3 ? 1 + (level - 3) / 4 : 0;
            result[ThreatLevel.Elite] = level >= 6 ? (level - 5) / 5 : 0;
        }
        else if (level == 10)
        {
            result[ThreatLevel.Boss] = 1;
        }
        else
        {
            result[ThreatLevel.Low] = 10 - Math.Min(5, (level - 10) / 2);  // taper off
            result[ThreatLevel.Medium] = 2 + (level - 10) / 3;
            result[ThreatLevel.High] = 1 + (level - 10) / 3;
            result[ThreatLevel.Elite] = 1 + (level - 10) / 4;
            result[ThreatLevel.Boss] = (level % 5 == 0) ? 1 : 0;  // Boss every 5 levels
        }

        // Remove zero entries for cleanliness
        return result.Where(kv => kv.Value > 0)
                     .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
    private static double CalculateMonsterWeight(Monster monster, int level)
    {
        double threatWeight = _monsterThreatWeights[monster.ThreatLevel];
        double levelRatio = Math.Clamp(level / (double)monster.Level, 0.5, 2.0);

        return threatWeight * levelRatio;
    }
}

