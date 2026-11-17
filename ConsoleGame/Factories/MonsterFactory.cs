using System.Text;
using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Factories;

public class MonsterFactory(IMonsterDao monsterDao, ISkillDao skillDao, IMonsterSkillSelector skillSelector,
    IRandomProvider randomProvider)
    : IMonsterFactory
{
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly IMonsterDao _monsterDao = monsterDao;
    private readonly ISkillDao _skillDao = skillDao;
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;
    
    private static readonly ElementType[] ElementTypes =
    [
        ElementType.Fire,
        ElementType.Ice,
        ElementType.Lightning,
        ElementType.Nature,
        ElementType.Radiance,
        ElementType.Abyssal
    ];
    
    const double Tolerance = 0.0001;

    // Dictionary to hold the number of monsters for each threat level in each campaign level
    private readonly Dictionary<int, Dictionary<ThreatLevel, int>> _campaignMonsters = Enumerable.Range(1, 20)
        .ToDictionary(level => level, GenerateMonstersForLevel);

    public List<Monster> GenerateMonsters(int level, bool campaign, int roomCount)
    {
        if (level < 1)
            throw new ArgumentOutOfRangeException(nameof(level));
        
        if (campaign && level > 20)
            throw new ArgumentOutOfRangeException(
                nameof(level),
                "Campaign levels cannot exceed 20.");
        
        var nameCounts = new Dictionary<string, int>();
        
        var selectedMonsters = campaign
            ? GenerateCampaignMonsters(level, nameCounts)
            : GenerateRandomMonsters(level, nameCounts, roomCount);
        
        if (selectedMonsters.Count == 0)
            throw new InvalidOperationException("No monsters selected for combat");

        SetMonsterSkills(level, selectedMonsters);
        
        //MonsterDiagnosticLoop();

        return selectedMonsters;
    }

    public void MonsterDiagnosticLoop()
    {
        for (int i = 1; i < 21; i++)
        {
            var testMonsters = GenerateRandomMonsters(i, new Dictionary<string, int>(), 5 * i);

            var threatCounts = testMonsters
                .GroupBy(m => m.ThreatLevel)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();

            System.Diagnostics.Debug.WriteLine(
                $"Level {i} ({5 * i} rooms) - Total: {testMonsters.Count} | " +
                string.Join(", ", threatCounts));
        }
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
                    availableMonsters[_randomProvider.Next(availableMonsters.Count)];

                monsters.Add(CreateMonster(randomMonsterBase, level, nameCounts));
            }
        }

        return monsters;
    }
    
   private List<Monster> GenerateRandomMonsters(int level, Dictionary<string, int> nameCounts, int roomCount)
    {
        if (roomCount < 1)
            throw new ArgumentOutOfRangeException(nameof(roomCount));

        var availableMonsters = _monsterDao.GetNonBossMonstersByMaxLevel(level);

        if (availableMonsters.Count == 0)
            throw new InvalidOperationException(
                "No available monsters found for the given level.");

        var targetWeight = level * roomCount * 3.5;
        var maxMonsterCount = roomCount * 2;
        var maxEliteCount = Math.Max(1, roomCount / 10);

        var monsters = new List<Monster>();
        var currentWeight = 0.0;
        var eliteCount = 0;

        while (currentWeight < targetWeight &&
               monsters.Count < maxMonsterCount)
        {
            var remainingWeight = targetWeight - currentWeight;

            // Once the Elite limit is reached, exclude Elite monsters.
            var eligibleBases = availableMonsters
                .Where(m => m is not EliteMonster || eliteCount < maxEliteCount)
                .ToList();

            if (eligibleBases.Count == 0)
                break;

            // Create leveled candidates so their actual generated stats
            // are used when calculating weight.
            var candidates = eligibleBases.Select(monsterBase =>
            {
                var candidateNameCounts = new Dictionary<string, int>(nameCounts);

                var monster = CreateMonster(
                    monsterBase, level, candidateNameCounts);

                return (Monster: monster, NameCounts: candidateNameCounts);
            })
            .Select(candidate => (
                candidate.Monster,
                candidate.NameCounts,
                Weight: CalculateMonsterWeight(candidate.Monster)))
            .Where(candidate => candidate.Weight <= remainingWeight)
            .ToList();

            if (candidates.Count == 0)
                break;

            var selected = candidates[
                _randomProvider.Next(candidates.Count)];

            monsters.Add(selected.Monster);
            currentWeight += selected.Weight;

            if (selected.Monster is EliteMonster)
                eliteCount++;

            // Only retain the naming changes for the selected monster.
            nameCounts.Clear();

            foreach (var (name, count) in selected.NameCounts)
                nameCounts[name] = count;
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
                new EliteMonster(_skillSelector, (MonsterBehaviorType)_randomProvider.Next(1, 6)),
                monsterBase, nameCounts),

            _ => SetBaseProperties(new Monster
            {
                Strategy = Monster.GetStrategyFor(
                    (MonsterBehaviorType)_randomProvider.Next(1, 6), _skillSelector)
            }, monsterBase, nameCounts)
        };

        newMonster.SetLevel(level);
        SetElementalProperties(newMonster);

        return newMonster;
    }

    private T SetBaseProperties<T>(T monster, Monster monsterBase, Dictionary<string, int> nameCounts) where T : Monster
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

    private void SetElementalProperties(Monster monster)
    {
        //set up random elements so that even if the base monster is pulled several times, the elements will be different
        //the only difference requirements: vulnerability can't equal resilience or attack element
        var attackElement = GetRandomElement();

        var vulnerability = GetRandomElement();
        while (attackElement == vulnerability)
        {
            vulnerability = GetRandomElement(); //vulnerability can't equal attack element
        }

        var resilience = GetRandomElement();
        while (vulnerability == resilience)
        {
            resilience = GetRandomElement(); //resilience can't equal vulnerability, but attack element is fine
        }
        
        // ElementalPower = 10-20% of AttackPower, minimum 1
        var elementalPower = Math.Max(
            1,
            (int)Math.Round(monster.AttackPower * 0.15,
                MidpointRounding.AwayFromZero));

        //add elemental properties to monsters based on their threat level
        switch (monster.ThreatLevel)
        {
            case ThreatLevel.Boss:
            case ThreatLevel.Elite:
                monster.Resilience = resilience;
                goto case ThreatLevel.High;

            case ThreatLevel.High:
                monster.Vulnerability = vulnerability;
                goto case ThreatLevel.Medium;

            case ThreatLevel.Medium:
                monster.AttackElement = attackElement;
                monster.ElementalPower = elementalPower;
                break;

            case ThreatLevel.Low:
            default:
                break;
        }
    }
    
    private ElementType GetRandomElement()
    {
        return ElementTypes[_randomProvider.Next(ElementTypes.Length)];
    }
    
    private string GetMonsterName(string baseName, Dictionary<string, int> nameCounts)
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
        var modifier = modifiers[_randomProvider.Next(modifiers.Length)];

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
    
    private void AssignSkillsToMonster(Monster monster, List<Skill> allSkills)
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

        int attackSkillDeficit = Math.Max(
            0, monster.Level + (monster.Level / 2) - currentAttackSkillCount);

        int supportSkillDeficit = Math.Max(
            0, monster.Level / 2 - currentSupportSkillCount);

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
    
    private void AssignBossSkills(Monster monster, List<Skill> bossSkills, int currentBossSkillCount, List<Skill> selectedSkills)
    {
        if (monster is not BossMonster)
            return;

        int deficit = 3 - currentBossSkillCount;

        AddRandomSkills(bossSkills, deficit, monster, selectedSkills);
    }

    private void AssignUltimateSkill(Monster monster, List<Skill> ultimateSkills, int currentUltimateSkillCount, List<Skill> selectedSkills)
    {
        if (monster is not EliteMonster || currentUltimateSkillCount >= 1)
            return;

        AddRandomSkills(ultimateSkills, 1, monster, selectedSkills);
    }
    
    private void AssignAttackSkills(Monster monster, List<Skill> martialSkills, List<Skill> magicSkills, int deficit, List<Skill> selectedSkills)
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

    private void AssignSupportSkills(Monster monster, List<Skill> supportSkills, int deficit, List<Skill> selectedSkills)
    {
        AddRandomSkills(supportSkills, deficit, monster, selectedSkills);
    }
    
    private void AddRandomSkills(List<Skill> availableSkills, int count, Monster monster, List<Skill> selectedSkills)
    {
        for (int i = 0; i < count && availableSkills.Count > 0; i++)
        {
            var selected = availableSkills[_randomProvider.Next(availableSkills.Count)];

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
        return (monster.AttackPower * 0.40)
               + (monster.DefensePower * 0.30)
               + (monster.Resistance * 0.30);
    }
}