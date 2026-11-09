using System.Text;
using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public class MonsterFactory(IMonsterDao monsterDao, ISkillDao skilldao, IMonsterSkillSelector skillSelector)
    : IMonsterFactory
{
    private static readonly Random Random = Random.Shared;
    private readonly IMonsterDao _monsterDao = monsterDao;
    private readonly ISkillDao _skillDao = skilldao;
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;

    private static readonly Dictionary<ThreatLevel, double> MonsterThreatWeights = new()
    {
        { ThreatLevel.Low, 0.5 },
        { ThreatLevel.Medium, 1.0 },
        { ThreatLevel.High, 1.5 },
        { ThreatLevel.Elite, 2.5 }
    };
    
    private static readonly ElementType[] ElementTypes =
    [
        ElementType.Fire,
        ElementType.Ice,
        ElementType.Lightning,
        ElementType.Nature,
        ElementType.Radiance,
        ElementType.Abyssal
    ];

    // Dictionary to hold the number of monsters for each threat level in each campaign level
    private readonly Dictionary<int, Dictionary<ThreatLevel, int>> _campaignMonsters = Enumerable.Range(1, 20)
        .ToDictionary(level => level, GenerateMonstersForLevel);

    public List<Monster> GenerateMonsters(int level, bool campaign)
    {
        var nameCounts = new Dictionary<string, int>();

        var selectedMonsters = campaign
            ? GenerateCampaignMonsters(level, nameCounts)
            : GenerateRandomMonsters(level, nameCounts);

        SetMonsterSkills(level, selectedMonsters);

        if (selectedMonsters.Count == 0)
            throw new InvalidOperationException("No monsters selected for combat");

        return selectedMonsters;
    }

    private List<Monster> GenerateCampaignMonsters(int level, Dictionary<string, int> nameCounts)
    {
        var monsters = new List<Monster>();

        if (!_campaignMonsters.TryGetValue(level, out var monstersForLevel))
            return monsters;

        foreach (var (threatLevel, monsterCount) in monstersForLevel)
        {
            var availableMonsters =
                _monsterDao.GetMonstersByMaxLevelAndThreatLevel(level, threatLevel);

            if (availableMonsters.Count == 0)
                throw new InvalidOperationException(
                    "No available monsters found for the given level and threat.");

            for (int i = 0; i < monsterCount; i++)
            {
                var randomMonsterBase =
                    availableMonsters[Random.Next(availableMonsters.Count)];

                monsters.Add(CreateMonster(randomMonsterBase, level, nameCounts));
            }
        }

        return monsters;
    }
    
    private List<Monster> GenerateRandomMonsters(int level, Dictionary<string, int> nameCounts)
    {
        var availableMonsters = _monsterDao.GetNonBossMonstersByMaxLevel(level);

        if (availableMonsters.Count == 0)
            throw new InvalidOperationException(
                "No available monsters found for the given level.");

        var targetMonsterCount = (int)Math.Round(level * 3.5);
        var currentCount = 0.0;

        var monsters = new List<Monster>();

        while (currentCount < targetMonsterCount)
        {
            var randomMonsterBase =
                availableMonsters[Random.Next(availableMonsters.Count)];

            var monster = CreateMonster(randomMonsterBase, level, nameCounts);
            var weight = CalculateMonsterWeight(monster);

            monsters.Add(monster);
            currentCount += weight;
        }

        return monsters;
    }
    
    private Monster CreateMonster(Monster monsterBase, int level, Dictionary<string, int> nameCounts)
    {
        var newMonster = monsterBase switch
        {
            BossMonster => SetBaseProperties(
                new BossMonster(_skillSelector), monsterBase, nameCounts),

            EliteMonster => SetBaseProperties(
                new EliteMonster(_skillSelector, (MonsterBehaviorType)Random.Next(1, 6)),
                monsterBase, nameCounts),

            _ => SetBaseProperties(new Monster
            {
                Strategy = Monster.GetStrategyFor(
                    (MonsterBehaviorType)Random.Next(1, 6), _skillSelector)
            }, monsterBase, nameCounts)
        };

        newMonster.SetLevel(level);
        SetElementalProperties(newMonster);

        return newMonster;
    }

    private static T SetBaseProperties<T>(T monster, Monster monsterBase, Dictionary<string, int> nameCounts) where T : Monster
    {
        monster.Name = GetMonsterName(monsterBase.Name, nameCounts);
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
        monster.DamageType = monsterBase.DamageType;
        monster.IsCore = monsterBase.IsCore;

        return monster;
    }

    private static void SetElementalProperties(Monster monster)
    {
        //set up random elements so that even if the base monster is pulled several times, the elements will be different
        var attackElement = GetRandomElement();

        var vulnerability = GetRandomElement();
        while (attackElement == vulnerability)
        {
            vulnerability = GetRandomElement();
        }
        
        var elementalPower = Random.Next(1, 3); 

        monster.AttackElement = attackElement;
        monster.ElementalPower = elementalPower;
        monster.Vulnerability = vulnerability;
    }
    
    private static ElementType GetRandomElement()
    {
        return ElementTypes[Random.Next(ElementTypes.Length)];
    }
    
    private static string GetMonsterName(string baseName, Dictionary<string, int> nameCounts)
    {
        string[] modifiers =
        [
            "Fierce", "Arcane", "Twisted", "Burning", "Rotting",
            "Grizzled", "Cackling", "Frostbitten", "Shadowed", "Lucky",
            "Silent", "Enraged", "Giggling", "Eldritch", "Cracked",
            "Slippery", "Vicious", "Bloody", "Ancient", "Frantic"
        ];

        // Ensure we always start at 1
        int count = nameCounts.GetValueOrDefault(baseName, 0);

        count++; // increment before using
        nameCounts[baseName] = count;

        var numeral = count <= 3999 ? ToRoman(count) : count.ToString();
        var modifier = modifiers[Random.Next(modifiers.Length)];

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
            AssignSkillsToMonster(monster, allSkills);
        }
    }
    
    private static void AssignSkillsToMonster(Monster monster, List<Skill> allSkills)
    {
        var selectedSkills = new List<Skill>();

        int currentAttackSkillCount =
            monster.Skills.Count(s => s is MartialSkill || s is MagicSkill);

        int currentSupportSkillCount =
            monster.Skills.Count(s => s is SupportSkill);

        int currentUltimateSkillCount =
            monster.Skills.Count(s => s is UltimateSkill);

        int currentBossSkillCount =
            monster.Skills.Count(s => s is BossSkill);

        int attackSkillDeficit =
            monster.Level + (monster.Level / 2) - currentAttackSkillCount;

        int supportSkillDeficit =
            monster.Level / 2 - currentSupportSkillCount;

        var martialSkills = GetAvailableSkills<MartialSkill>(allSkills, monster);
        var magicSkills = GetAvailableSkills<MagicSkill>(allSkills, monster);
        var supportSkills = GetAvailableSkills<SupportSkill>(allSkills, monster);
        var ultimateSkills = GetAvailableSkills<UltimateSkill>(allSkills, monster);
        var bossSkills = GetAvailableSkills<BossSkill>(allSkills, monster);

        AssignBossSkills(monster, bossSkills, currentBossSkillCount, selectedSkills);
        AssignUltimateSkill(monster, ultimateSkills, currentUltimateSkillCount, selectedSkills);
        AssignAttackSkills(monster, martialSkills, magicSkills, attackSkillDeficit, selectedSkills);
        AssignSupportSkills(monster, supportSkills, supportSkillDeficit, selectedSkills);

        monster.Skills.AddRange(selectedSkills);
    }
    
    private static List<Skill> GetAvailableSkills<TSkill>(List<Skill> allSkills, Monster monster) where TSkill : Skill
    {
        return allSkills
            .OfType<TSkill>()
            .Where(skill =>
                skill.RequiredLevel <= monster.Level &&
                monster.Skills.All(existing => existing.Name != skill.Name))
            .Cast<Skill>()
            .ToList();
    }
    
    private static void AssignBossSkills(Monster monster, List<Skill> bossSkills, int currentBossSkillCount, List<Skill> selectedSkills)
    {
        if (monster is not BossMonster)
            return;

        int deficit = 3 - currentBossSkillCount;

        AddRandomSkills(bossSkills, deficit, monster, selectedSkills);
    }

    private static void AssignUltimateSkill(Monster monster, List<Skill> ultimateSkills, int currentUltimateSkillCount, List<Skill> selectedSkills)
    {
        if (monster is not EliteMonster || currentUltimateSkillCount > 1)
            return;

        AddRandomSkills(ultimateSkills, 1, monster, selectedSkills);
    }
    
    private static void AssignAttackSkills(Monster monster, List<Skill> martialSkills, List<Skill> magicSkills, int deficit, List<Skill> selectedSkills)
    {
        if (deficit <= 0)
            return;

        var usableSkills = monster.DamageType switch
        {
            DamageType.Martial => martialSkills,
            DamageType.Magical => magicSkills,
            DamageType.Hybrid => [.. martialSkills, .. magicSkills],
            _ => []
        };

        AddRandomSkills(usableSkills, deficit, monster, selectedSkills);
    }

    private static void AssignSupportSkills(Monster monster, List<Skill> supportSkills, int deficit, List<Skill> selectedSkills)
    {
        AddRandomSkills(supportSkills, deficit, monster, selectedSkills);
    }
    
    private static void AddRandomSkills(List<Skill> availableSkills, int count, Monster monster, List<Skill> selectedSkills)
    {
        for (int i = 0; i < count && availableSkills.Count > 0; i++)
        {
            var selected = availableSkills[Random.Next(availableSkills.Count)];

            var clone = selected.Clone();
            clone.InitializeSkill(monster.Level);

            selectedSkills.Add(clone);
            availableSkills.Remove(selected);
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
            result[ThreatLevel.Low] = 10 - Math.Min(5, (level - 10) / 2); // taper off
            result[ThreatLevel.Medium] = 2 + (level - 10) / 3;
            result[ThreatLevel.High] = 1 + (level - 10) / 3;
            result[ThreatLevel.Elite] = 1 + (level - 10) / 4;
            result[ThreatLevel.Boss] = (level % 5 == 0) ? 1 : 0; // Boss every 5 levels
        }

        // Remove zero entries for cleanliness
        return result.Where(kv => kv.Value > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private static double CalculateMonsterWeight(Monster monster)
    {
        //the weight should be calculated by threatWEight * levelRatio but 
            //newMonster.SetLevel(level) makes this meaningless?
        var threatWeight = MonsterThreatWeights[monster.ThreatLevel];
        //double levelRatio = Math.Clamp(level / (double)monster.Level, 0.5, 2.0);

        return threatWeight;
    }
}