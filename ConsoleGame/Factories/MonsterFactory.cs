using System.Text;
using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public class MonsterFactory(IMonsterDao monsterDao, ISkillDao skilldao, IMonsterSkillSelector skillSelector) : IMonsterFactory
{
    private static readonly Random _rng = Random.Shared;
    private readonly IMonsterDao _monsterDao = monsterDao;
    private readonly ISkillDao _skillDao = skilldao;
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;

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

    public List<Monster> GenerateMonsters(int level, bool campaign, int numRooms)
    {
        NameCounts.Clear();
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

                        if (availableMonsters.Count == 0)
                            throw new InvalidOperationException("No available monsters found for the given level and threat.");

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

            if (availableMonsters.Count == 0)
                throw new InvalidOperationException("No available monsters found for the given level.");

            int targetMonsterCount = (int)Math.Round(level * 3.5); //weighted monster score
            int maxMonsterCount = (int)Math.Round((numRooms - 2) * 1.5); //unweighted monster score

            int weightedMonsterCount = 0;
            int flatMonsterCount = 0;
            int eliteCount = 0;

            if (level < 3)
            {
                availableMonsters = [.. availableMonsters
                    .Where(m => m.ThreatLevel == ThreatLevel.Low 
                        || m.ThreatLevel == ThreatLevel.Medium)];
            }
            else if (level < 5)
            {
                availableMonsters = [.. availableMonsters
                    .Where(m => m.ThreatLevel != ThreatLevel.Elite)];
            }

            while (weightedMonsterCount < targetMonsterCount && flatMonsterCount < maxMonsterCount)
            {
                if (eliteCount > (level / 5)) //remove elitemonsters if their count passes the threshold
                {
                    availableMonsters.RemoveAll(m => m is EliteMonster);
                }

                // Select a random monster and create a fresh instance
                var randomMonsterBase = availableMonsters[_rng.Next(availableMonsters.Count)];
                var randomMonster = CreateMonster(randomMonsterBase, level);
                if (randomMonster is EliteMonster) eliteCount++;

                // Calculate weight based on monster threat level and relative level to player
                var weight = CalculateMonsterWeight(randomMonster, level);

                // Add the monster and increase the monster count by the calculated weight
                selectedMonsters.Add(randomMonster);
                weightedMonsterCount += (int)Math.Round(weight);
                flatMonsterCount += 1;
            }
        }

        SetMonsterSkills(level, selectedMonsters);

        if (selectedMonsters.Count == 0) throw new InvalidOperationException("No monsters selected for combat");

        return selectedMonsters;
    }

    private Monster CreateMonster(Monster monsterBase, int level)
    {
        var newMonster = monsterBase switch
        {
            BossMonster => SetBaseProperties(new BossMonster(_skillSelector), monsterBase),

            EliteMonster => SetBaseProperties(new EliteMonster(_skillSelector, (MonsterBehaviorType)_rng.Next(1, 6)), monsterBase),

            _ => SetBaseProperties(new Monster
            {
                Strategy = Monster.GetStrategyFor((MonsterBehaviorType)_rng.Next(1, 6), _skillSelector)
            }, monsterBase)
        };

        newMonster.SetLevel(level);

        return newMonster;
    }

    private static T SetBaseProperties<T>(T monster, Monster monsterBase) where T : Monster
    {
        monster.Name = GetMonsterName(monsterBase.Name);
        monster.Level = monsterBase.Level;
        monster.ThreatLevel = monsterBase.ThreatLevel;
        monster.MonsterType = monsterBase.MonsterType;
        monster.Description = monsterBase.Description;
        monster.Skills = [.. monsterBase.Skills.Select(skill => skill.Clone())];
        monster.AttackPower = monsterBase.AttackPower;
        monster.DefensePower = monsterBase.DefensePower;
        monster.Resistance = monsterBase.Resistance;
        monster.AggressionLevel = monsterBase.AggressionLevel;
        monster.MaxHealth = monsterBase.MaxHealth;

        //set up random elements so that even if the base monster is pulled several times, the elements will be different
        var elements = Enumerable.Range(0, 7);
        var attackElement = elements.ElementAt(_rng.Next(elements.Count())) switch
        {
            0 => ElementType.Fire,
            1 => ElementType.Ice,
            2 => ElementType.Lightning,
            3 => ElementType.Nature,
            4 => ElementType.Radiance,
            5 => ElementType.Abyssal,
            _ => (ElementType?)null
        };
        var vulnerability = elements.ElementAt(_rng.Next(elements.Count())) switch
        {
            0 => ElementType.Fire,
            1 => ElementType.Ice,
            2 => ElementType.Lightning,
            3 => ElementType.Nature,
            4 => ElementType.Radiance,
            5 => ElementType.Abyssal,
            _ => (ElementType?)null
        };
        while (attackElement == vulnerability)
        {
            vulnerability = elements.ElementAt(_rng.Next(elements.Count())) switch
            {
                0 => ElementType.Fire,
                1 => ElementType.Ice,
                2 => ElementType.Lightning,
                3 => ElementType.Nature,
                4 => ElementType.Radiance,
                5 => ElementType.Abyssal,
                _ => (ElementType?)null
            };
        }
        int? elementalPower = attackElement == null ? null : _rng.Next(1, 3); 

        monster.AttackElement = attackElement;
        monster.ElementalPower = elementalPower;
        monster.Vulnerability = vulnerability;

        return monster;
    }

    private static string GetMonsterName(string baseName)
    {
        string[] modifiers =
        [
            "Fierce", "Arcane", "Twisted", "Burning", "Rotting",
        "Grizzled", "Cackling", "Frostbitten", "Shadowed", "Lucky",
        "Silent", "Enraged", "Giggling", "Eldritch", "Cracked",
        "Slippery", "Vicious", "Bloody", "Ancient", "Frantic"
        ];

        // Ensure we always start at 1
        if (!NameCounts.TryGetValue(baseName, out int count))
            count = 0;

        count++; // increment before using
        NameCounts[baseName] = count;

        var numeral = count <= 3999 ? ToRoman(count) : count.ToString();
        var modifier = modifiers[_rng.Next(modifiers.Length)];

        return $"{baseName} {numeral} the {modifier}";
    }

    private static string ToRoman(int number)
    {
        if (number < 1 || number > 3999)
            throw new ArgumentOutOfRangeException(nameof(number), "Value must be between 1 and 3999.");

        var romanNumerals = new[]
            {
            (1000, "M"),
            (900, "CM"),
            (500, "D"),
            (400, "CD"),
            (100, "C"),
            (90, "XC"),
            (50, "L"),
            (40, "XL"),
            (10, "X"),
            (9, "IX"),
            (5, "V"),
            (4, "IV"),
            (1, "I")
        };

        var result = new StringBuilder();

        foreach (var (value, numeral) in romanNumerals)
        {
            while (number >= value)
            {
                result.Append(numeral);
                number -= value;
            }
        }

        return result.ToString();
    }
    private void SetMonsterSkills(int level, List<Monster> monsters)
    {
        var allSkills = _skillDao.GetUnassignedSkillsByMaxLevel(level);

        if (allSkills.Count == 0)
            return;

        foreach (var monster in monsters)
        {
            var selectedSkills = new List<Skill>();

            var currentAttackSkillCount = monster.Skills.Count(s => s is MartialSkill || s is MagicSkill);
            var currentSupportSkillCount = monster.Skills.Count(s => s is SupportSkill);
            var currentUltimateSkillCount = monster.Skills.Count(s => s is UltimateSkill);
            var currentBossSkillCount = monster.Skills.Count(s => s is BossSkill);

            int attackSkillCount = monster.Level + (monster.Level / 2);
            int supportSkillCount = monster.Level / 2;
            int attackSkillDeficit = attackSkillCount - currentAttackSkillCount;
            int supportSkillDeficit = supportSkillCount - currentSupportSkillCount;

            // Filter skills not already assigned to this monster
            var martialSkills = allSkills.Where(s => s is MartialSkill && s.RequiredLevel <= monster.Level && !monster.Skills.Any(ms => ms.Name == s.Name)).ToList();
            var magicSkills = allSkills.Where(s => s is MagicSkill && s.RequiredLevel <= monster.Level && !monster.Skills.Any(ms => ms.Name == s.Name)).ToList();
            var supportSkills = allSkills.OfType<SupportSkill>().Where(s => s.RequiredLevel <= monster.Level && !monster.Skills.Any(ms => ms.Name == s.Name)).ToList();
            var ultimateSkills = allSkills.OfType<UltimateSkill>().Where(s => s.RequiredLevel <= monster.Level && !monster.Skills.Any(ms => ms.Name == s.Name)).ToList();
            var bossSkills = allSkills.OfType<BossSkill>().Where(s => s.RequiredLevel <= monster.Level && !monster.Skills.Any(ms => ms.Name == s.Name)).ToList();

            if (monster is BossMonster)
            {
                int bossSkillDeficit = 3 - currentBossSkillCount;

                for (int i = 0; i < bossSkillDeficit && bossSkills.Count > 0; i++)
                {
                    var selected = bossSkills[_rng.Next(bossSkills.Count)];
                    var clone = selected.Clone();
                    clone.InitializeSkill(monster.Level);
                    selectedSkills.Add(clone);
                    bossSkills.Remove(selected);
                }
            }

            if (monster is EliteMonster && currentUltimateSkillCount <= 1 && ultimateSkills.Count > 0)
            {
                var selected = ultimateSkills[_rng.Next(ultimateSkills.Count)];
                var clone = selected.Clone();
                clone.InitializeSkill(monster.Level);
                selectedSkills.Add(clone);
            }

            if (attackSkillDeficit > 0)
            {
                var usableAttackSkills = monster.DamageType switch
                {
                    DamageType.Martial => martialSkills,
                    DamageType.Magical => magicSkills,
                    DamageType.Hybrid => [.. martialSkills, .. magicSkills],
                    _ => []
                };

                for (int i = 0; i < attackSkillDeficit && usableAttackSkills.Count > 0; i++)
                {
                    var selected = usableAttackSkills[_rng.Next(usableAttackSkills.Count)];
                    var clone = selected.Clone();
                    clone.InitializeSkill(monster.Level);
                    selectedSkills.Add(clone);
                    usableAttackSkills.Remove(selected);
                }
            }

            if (supportSkillDeficit > 0)
            {
                for (int i = 0; i < supportSkillDeficit && supportSkills.Count > 0; i++)
                {
                    var selected = supportSkills[_rng.Next(supportSkills.Count)];
                    var clone = selected.Clone();
                    clone.InitializeSkill(monster.Level);
                    selectedSkills.Add(clone);
                    supportSkills.Remove(selected);
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
        double baseWeight = monster.ThreatLevel switch
        {
            ThreatLevel.Low => 0.75,
            ThreatLevel.Medium => 1.5,
            ThreatLevel.High => 2.0,
            ThreatLevel.Elite => 3.5
        };

        return baseWeight * (1 + level * 0.05); // gradually increases
    }
}

