using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public class MonsterFactory(MonsterDao monsterDao, SkillDao skilldao, MonsterSkillSelector skillSelector)
{
    private static readonly Random _rng = Random.Shared;
    private readonly MonsterDao _monsterDao = monsterDao;
    private readonly SkillDao _skillDao = skilldao;
    private readonly MonsterSkillSelector _skillSelector = skillSelector;

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

    public Monster CreateMonster(Monster monsterBase, int level)
    {
        Monster monster;

        if (monsterBase is BossMonster)
        {
            monster = new BossMonster(_skillSelector)
            {
                Name = GetMonsterName(monsterBase.Name),
                Level = monsterBase.Level,
                ThreatLevel = monsterBase.ThreatLevel,
                MonsterType = monsterBase.MonsterType,
                Description = monsterBase.Description,
                Skills = monsterBase.Skills
            };
        }
        else if (monsterBase is EliteMonster)
        {
            monster = new EliteMonster(_skillSelector, (MonsterBehaviorType)_rng.Next(1, 6))
            {
                Name = GetMonsterName(monsterBase.Name),
                Level = monsterBase.Level,
                ThreatLevel = monsterBase.ThreatLevel,
                MonsterType = monsterBase.MonsterType,
                Description = monsterBase.Description,
                Strategy = Monster.GetStrategyFor((MonsterBehaviorType)_rng.Next(1, 6), _skillSelector),
                Skills = monsterBase.Skills
            };
        }
        else
        {
            monster = new Monster
            {
                Name = GetMonsterName(monsterBase.Name),
                Level = monsterBase.Level,
                ThreatLevel = monsterBase.ThreatLevel,
                MonsterType = monsterBase.MonsterType,
                Description = monsterBase.Description,
                Strategy = Monster.GetStrategyFor((MonsterBehaviorType)_rng.Next(1, 6), _skillSelector),
                Skills = monsterBase.Skills
            };
        }

        monster.SetLevel(level);

        return monster;
    }
    private static string GetMonsterName(string baseName)
    {
        string[] romanNumerals = ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"];
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
        var bossSkills = skills.Where(s => s is BossSkill).ToList();

        foreach (var monster in monsters)
        {
            var selectedSkills = new List<Skill>();

            var currentAttackSkillCount = monster.Skills.Count(s => s is MartialSkill || s is MagicSkill);
            var currentSupportSkillCount = monster.Skills.Count(s => s is SupportSkill);
            var currentUltimateSkillCount = monster.Skills.Count(s => s is UltimateSkill);
            var currentBossSkillCount = monster.Skills.Count(s => s is BossSkill);

            int attackSkillCount = monster.Level + (monster.Level / 2); //1 + half the level # of skills 
            int supportSkillCount = monster.Level / 2; //1 skill per 2 levels
            int attackSkillDeficit = attackSkillCount - currentAttackSkillCount;
            int supportSkillDeficit = supportSkillCount - currentSupportSkillCount;

            if (monster is BossMonster)
            {
                var bossSkillCount = 3;

                var deficitBossSkillCount = bossSkillCount - currentBossSkillCount;

                if (deficitBossSkillCount < 1)
                    continue;

                var usableBossSkills = bossSkills.Where(s => s.RequiredLevel <= monster.Level).ToList();

                for (int i = 0; i < deficitBossSkillCount && usableBossSkills.Count > 0; i++)
                {
                    var randomSkill = usableBossSkills[_rng.Next(usableBossSkills.Count)];
                    randomSkill.InitializeSkill(monster.Level);
                    selectedSkills.Add(randomSkill);
                    bossSkills.Remove(randomSkill);
                }
            }

            if (monster is EliteMonster)
            {
                if (currentUltimateSkillCount > 1)
                {
                    continue;
                }
                var usableUltimateSkills = ultimateSkills.Where(s => s.RequiredLevel <= monster.Level).ToList();
                if (usableUltimateSkills.Count > 0)
                {
                    var randomSkill = usableUltimateSkills[_rng.Next(usableUltimateSkills.Count)];
                    randomSkill.InitializeSkill(monster.Level);
                    selectedSkills.Add(randomSkill);
                }                
            }           

            if (attackSkillDeficit > 0)
            {
                var usableAttackSkills = monster.DamageType == DamageType.Martial ?
                    martialSkills.Where(martialSkills => martialSkills.RequiredLevel <= monster.Level).ToList() :
                    [.. magicSkills.Where(magicSkills => magicSkills.RequiredLevel <= monster.Level)];

                for (int i = 0; i < attackSkillDeficit && usableAttackSkills.Count > 0; i++)
                {
                    var randomSkill = usableAttackSkills[_rng.Next(usableAttackSkills.Count)];
                    randomSkill.InitializeSkill(monster.Level);
                    selectedSkills.Add(randomSkill);
                    usableAttackSkills.Remove(randomSkill);
                }
            }

            if (supportSkillDeficit > 0)
            {
                var usuableSupportSkills = supportSkills.Where(s => s.RequiredLevel <= monster.Level).ToList();

                for (int i = 0; i < supportSkillDeficit && usuableSupportSkills.Count > 0; i++)
                {
                    var randomSkill = usuableSupportSkills[_rng.Next(usuableSupportSkills.Count)];
                    randomSkill.InitializeSkill(monster.Level);
                    selectedSkills.Add(randomSkill);
                    usuableSupportSkills.Remove(randomSkill);
                }
            }           

            monster.Skills.AddRange(selectedSkills);
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

