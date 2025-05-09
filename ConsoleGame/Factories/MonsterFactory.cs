using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public class MonsterFactory(MonsterDao monsterDao)
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
    private readonly MonsterDao _monsterDao = monsterDao;

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

        return selectedMonsters;
    }

    public static Monster CreateMonster(Monster monsterBase, int level)
    {
        var monster = new Monster
        {
            Name = monsterBase.Name,
            Level = monsterBase.Level,
            MaxHealth = monsterBase.MaxHealth,
            CurrentHealth = monsterBase.MaxHealth,
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

        if (monster.Level < (level / 3))
        {
            monster.LevelUp(level);
        }

        return monster;
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

