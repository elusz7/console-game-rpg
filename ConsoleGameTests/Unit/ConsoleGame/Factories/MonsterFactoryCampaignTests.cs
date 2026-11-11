using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Factories;

[TestClass]
public class MonsterFactoryCampaignTests
{
    private readonly Mock<IMonsterDao> _monsterDaoMock = new();
    private readonly Mock<ISkillDao> _skillDaoMock = new();
    private readonly Mock<IMonsterSkillSelector> _skillSelectorMock = new();
    
    private static readonly ElementType[] ValidElements =
    [
        ElementType.Fire,
        ElementType.Ice,
        ElementType.Lightning,
        ElementType.Nature,
        ElementType.Radiance,
        ElementType.Abyssal
    ];

    private MonsterFactory CreateFactory()
    {
        return new MonsterFactory(_monsterDaoMock.Object, _skillDaoMock.Object, _skillSelectorMock.Object);
    }

    private static Monster CreateMonster(string name, int level, ThreatLevel threat, DamageType damageType = DamageType.Martial)
    {
        return new Monster
        {
            Name = name,
            Level = level,
            ThreatLevel = threat,
            MonsterType = "TestType",
            Description = "Test",
            Skills = [],
            AttackPower = 10,
            DefensePower = 5,
            Resistance = 2,
            AggressionLevel = 1,
            MaxHealth = 100,
            DamageType = damageType
        };
    }
    
    private BossMonster CreateBossMonster(int level, params Skill[] skills)
    {
        return new BossMonster(_skillSelectorMock.Object)
        {
            Name = "Dragon",
            Level = level,
            ThreatLevel = ThreatLevel.Boss,
            MonsterType = "TestType",
            Description = "Test",
            Skills = [.. skills],
            AttackPower = 10,
            DefensePower = 5,
            Resistance = 2,
            AggressionLevel = 1,
            MaxHealth = 100,
            DamageType = DamageType.Martial
        };
    }

    private static void AssertValidElementalProperties(Monster monster)
    {
        // Low threat: no elemental properties
        if (monster.ThreatLevel == ThreatLevel.Low)
        {
            Assert.IsNull(monster.AttackElement);
            Assert.IsNull(monster.ElementalPower);
            Assert.IsNull(monster.Vulnerability);
            Assert.IsNull(monster.Resilience);
            return;
        }

        // Medium+: AttackElement and ElementalPower
        Assert.IsTrue(monster.AttackElement.HasValue);
        Assert.IsTrue(ValidElements.Contains(monster.AttackElement.Value));
        var expectedPower = Math.Max(1, (int)Math.Round(monster.AttackPower * 0.15, MidpointRounding.AwayFromZero));
        Assert.AreEqual(expectedPower, monster.ElementalPower,
            $"ElementalPower {monster.ElementalPower} should be {expectedPower} for AttackPower {monster.AttackPower}");

        // High+: Vulnerability
        if (monster.ThreatLevel >= ThreatLevel.High)
        {
            Assert.IsTrue(monster.Vulnerability.HasValue);
            Assert.IsTrue(ValidElements.Contains(monster.Vulnerability.Value));
            Assert.AreNotEqual(monster.AttackElement, monster.Vulnerability);
        }
        else
        {
            Assert.IsNull(monster.Vulnerability);
        }

        // Elite/Boss: Resilience
        if (monster.ThreatLevel >= ThreatLevel.Elite)
        {
            Assert.IsTrue(monster.Resilience.HasValue);
            Assert.IsTrue(ValidElements.Contains(monster.Resilience.Value));
            Assert.AreNotEqual(monster.Resilience, monster.Vulnerability);
        }
        else
        {
            Assert.IsNull(monster.Resilience);
        }
    }

    [TestMethod]
    public void GenerateMonsters_CampaignMode_ReturnsExpectedMonsters()
    {
        // Arrange
        var level = 2;
        var threat = ThreatLevel.Low;
        var monster = CreateMonster("Goblin", level, threat);
        _monsterDaoMock
            .Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level,  It.IsAny<ThreatLevel>()))
            .Returns([monster]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: true);

        // Assert
        Assert.IsTrue(monsters.Count > 0);
        Assert.IsTrue(monsters.All(m => m.Level == level));
        Assert.IsTrue(monsters.All(m => m.MonsterType == "TestType"));

        foreach (var m in monsters)
            AssertValidElementalProperties(m);
    }

    [TestMethod]
    public void GenerateMonsters_CampaignMode_ThrowsIfNoMonsters()
    {
        // Arrange
        var level = 1;
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, (int)ThreatLevel.Low))
            .Returns([]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => factory.GenerateMonsters(level, campaign: true));
    }

    [TestMethod]
    public void GenerateMonsters_NamesDuplicateBaseMonstersSequentially()
    {
        var level = 1;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);

        _monsterDaoMock
            .Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(
                level, It.IsAny<ThreatLevel>()))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: true);

        Assert.AreEqual(2, monsters.Count);
        Assert.IsTrue(monsters.Any(m => m.Name.StartsWith("Goblin I the ")));
        Assert.IsTrue(monsters.Any(m => m.Name.StartsWith("Goblin II the ")));
    }
    
    [TestMethod]
    public void GenerateMonsters_NamesDifferentBaseMonstersIndependently()
    {
        var level = 2;

        var goblin = CreateMonster("Goblin", level, ThreatLevel.Low);
        var orc = CreateMonster("Orc", level, ThreatLevel.Medium);

        _monsterDaoMock
            .Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(
                level, ThreatLevel.Low))
            .Returns([goblin]);

        _monsterDaoMock
            .Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(
                level, ThreatLevel.Medium))
            .Returns([orc]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: true);

        Assert.AreEqual(4, monsters.Count);

        Assert.IsTrue(monsters.Any(m => m.Name.StartsWith("Goblin I the ")));
        Assert.IsTrue(monsters.Any(m => m.Name.StartsWith("Goblin II the ")));
        Assert.IsTrue(monsters.Any(m => m.Name.StartsWith("Goblin III the ")));

        Assert.IsTrue(monsters.Any(m => m.Name.StartsWith("Orc I the ")));
    }
    
    [TestMethod]
    public void GenerateMonsters_BossReceivesUpToThreeBossSkills()
    {
        var level = 10;
        var boss = CreateBossMonster(level);

        var skills = new List<Skill>
        {
            CreateBossSkill("Dragon Breath"),
            CreateBossSkill("Tail Swipe"),
            CreateBossSkill("Flame Storm"),
            CreateBossSkill("Meteor")
        };

        _monsterDaoMock
            .Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(
                level, ThreatLevel.Boss))
            .Returns([boss]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns(skills);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: true);

        var generatedBoss = monsters.Single();

        Assert.AreEqual(3, generatedBoss.Skills.OfType<BossSkill>().Count());
    }
    
    [TestMethod]
    public void GenerateMonsters_BossDoesNotReceiveMoreThanThreeBossSkills()
    {
        var level = 10;

        var existingSkills = new Skill[]
        {
            CreateBossSkill("Dragon Breath"),
            CreateBossSkill("Tail Swipe")
        };

        var boss = CreateBossMonster(level, existingSkills);

        var availableSkills = new List<Skill>
        {
            CreateBossSkill("Flame Storm"),
            CreateBossSkill("Meteor")
        };

        _monsterDaoMock
            .Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(
                level, ThreatLevel.Boss))
            .Returns([boss]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns(availableSkills);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: true);

        var bossSkills = monsters.Single().Skills.OfType<BossSkill>().ToList();

        Assert.AreEqual(3, bossSkills.Count);
        Assert.IsTrue(bossSkills.Any(s => s.Name == "Dragon Breath"));
        Assert.IsTrue(bossSkills.Any(s => s.Name == "Tail Swipe"));
    }
    
    private static BossSkill CreateBossSkill(string name) =>
        new() { Name = name, RequiredLevel = 1 };

    private List<Monster> GenerateCampaignMonsters(int level, Monster monster, List<Skill> availableSkills)
    {
        _monsterDaoMock
            .Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(
                level, It.IsAny<ThreatLevel>()))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns(availableSkills);

        var factory = CreateFactory();
        return factory.GenerateMonsters(level, campaign: true);
    }
    
    [TestMethod]
    public void GenerateMonsters_DoesNotAssignSkillsAboveMonsterLevel()
    {
        var level = 1;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);

        var skill = new SupportSkill
        {
            Name = "Greater Heal",
            RequiredLevel = 2
        };

        var monsters = GenerateCampaignMonsters(level, monster, [skill]);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.All(s => s.Name != skill.Name)));
    }
    
    [TestMethod]
    public void GenerateMonsters_DoesNotAssignDuplicateSkillNames()
    {
        var level = 1;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);

        monster.Skills.Add(new SupportSkill
        {
            Name = "Heal",
            RequiredLevel = 1
        });

        var duplicateSkill = new SupportSkill
        {
            Name = "Heal",
            RequiredLevel = 1
        };

        var monsters = GenerateCampaignMonsters(level, monster, [duplicateSkill]);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.Count(s => s.Name == "Heal") == 1));
    }
    
    [TestMethod]
    public void GenerateMonsters_AssignsClonedSkills()
    {
        var level = 2;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);

        var skill = new SupportSkill
        {
            Name = "Heal",
            RequiredLevel = 1
        };

        var monsters = GenerateCampaignMonsters(level, monster, [skill]);

        var assignedSkills = monsters
            .SelectMany(m => m.Skills)
            .Where(s => s.Name == skill.Name)
            .ToList();

        Assert.IsTrue(assignedSkills.Count > 0);
        Assert.IsTrue(assignedSkills.All(s => !ReferenceEquals(s, skill)));
        Assert.AreEqual(assignedSkills.Count,
            assignedSkills.Distinct(ReferenceEqualityComparer.Instance).Count());
    }
    
    [TestMethod]
    public void GenerateMonsters_InsufficientAvailableSkills_DoesNotThrow()
    {
        var level = 5;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);

        var availableSkills = new List<Skill>
        {
            new SupportSkill { Name = "Heal", RequiredLevel = 1 }
        };

        var monsters = GenerateCampaignMonsters(level, monster, availableSkills);

        Assert.IsNotNull(monsters);
        Assert.IsTrue(monsters.Count > 0);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.Count(s => s.Name == "Heal") == 1));
    }
    
    [TestMethod]
    public void GenerateMonsters_NoAvailableSkills_GeneratesSuccessfully()
    {
        var level = 5;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);

        var availableSkills = new List<Skill>();

        var monsters = GenerateCampaignMonsters(level, monster, availableSkills);

        Assert.IsNotNull(monsters);
        Assert.IsTrue(monsters.Count > 0);

        Assert.IsTrue(monsters.All(m => m.Skills.Count == 0));
    }

    // --- GenerateMonstersForLevel level range tests ---

    [TestMethod]
    public void GenerateMonsters_InvalidLevel_ReturnsEmpty()
    {
        // Arrange
        _monsterDaoMock
            .Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(It.IsAny<int>(), It.IsAny<ThreatLevel>()))
            .Returns([]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(It.IsAny<int>())).Returns([]);

        var factory = CreateFactory();

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => factory.GenerateMonsters(0, campaign: true));
        Assert.ThrowsException<InvalidOperationException>(() => factory.GenerateMonsters(-1, campaign: true));
        Assert.ThrowsException<InvalidOperationException>(() => factory.GenerateMonsters(21, campaign: true));
    }

    [TestMethod]
    public void GenerateMonsters_Level3_IntroducesHighThreat()
    {
        // Arrange
        var level = 3;
        // Expected: Low = 2 + (3/2) = 3, Medium = 1 + (3/4) = 1, High = 1 + (3-3)/4 = 1
        var lowMonster = CreateMonster("Goblin", level, ThreatLevel.Low);
        var mediumMonster = CreateMonster("Orc", level, ThreatLevel.Medium);
        var highMonster = CreateMonster("Troll", level, ThreatLevel.High);

        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Low)).Returns([lowMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Medium)).Returns([mediumMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.High)).Returns([highMonster]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: true);

        // Assert
        Assert.AreEqual(3, monsters.Count(m => m.ThreatLevel == ThreatLevel.Low));
        Assert.AreEqual(1, monsters.Count(m => m.ThreatLevel == ThreatLevel.Medium));
        Assert.AreEqual(1, monsters.Count(m => m.ThreatLevel == ThreatLevel.High));
        Assert.AreEqual(0, monsters.Count(m => m.ThreatLevel == ThreatLevel.Elite));
        Assert.AreEqual(0, monsters.Count(m => m.ThreatLevel == ThreatLevel.Boss));
    }

    [TestMethod]
    public void GenerateMonsters_Level6_NoEliteYet()
    {
        // Arrange
        var level = 6;
        // Expected: Low = 2 + (6/2) = 5, Medium = 1 + (6/4) = 2, High = 1 + (6-3)/4 = 1, Elite = (6-5)/5 = 0 (integer division)
        var lowMonster = CreateMonster("Goblin", level, ThreatLevel.Low);
        var mediumMonster = CreateMonster("Orc", level, ThreatLevel.Medium);
        var highMonster = CreateMonster("Troll", level, ThreatLevel.High);

        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Low)).Returns([lowMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Medium)).Returns([mediumMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.High)).Returns([highMonster]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: true);

        // Assert
        Assert.AreEqual(5, monsters.Count(m => m.ThreatLevel == ThreatLevel.Low));
        Assert.AreEqual(2, monsters.Count(m => m.ThreatLevel == ThreatLevel.Medium));
        Assert.AreEqual(1, monsters.Count(m => m.ThreatLevel == ThreatLevel.High));
        Assert.AreEqual(0, monsters.Count(m => m.ThreatLevel == ThreatLevel.Elite));
        Assert.AreEqual(0, monsters.Count(m => m.ThreatLevel == ThreatLevel.Boss));
    }

    [TestMethod]
    public void GenerateMonsters_Level9_MaxPreBossComposition()
    {
        // Arrange
        var level = 9;
        // Expected: Low = Min(10, 2 + 9/2) = 6, Medium = 1 + 9/4 = 3, High = 1 + (9-3)/4 = 2, Elite = (9-5)/5 = 0 (integer division)
        var lowMonster = CreateMonster("Goblin", level, ThreatLevel.Low);
        var mediumMonster = CreateMonster("Orc", level, ThreatLevel.Medium);
        var highMonster = CreateMonster("Troll", level, ThreatLevel.High);

        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Low)).Returns([lowMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Medium)).Returns([mediumMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.High)).Returns([highMonster]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: true);

        // Assert
        Assert.AreEqual(6, monsters.Count(m => m.ThreatLevel == ThreatLevel.Low));
        Assert.AreEqual(3, monsters.Count(m => m.ThreatLevel == ThreatLevel.Medium));
        Assert.AreEqual(2, monsters.Count(m => m.ThreatLevel == ThreatLevel.High));
        Assert.AreEqual(0, monsters.Count(m => m.ThreatLevel == ThreatLevel.Elite));
        Assert.AreEqual(0, monsters.Count(m => m.ThreatLevel == ThreatLevel.Boss));
    }

    [TestMethod]
    public void GenerateMonsters_Level11_FirstPostBossLevel()
    {
        // Arrange
        var level = 11;
        // Expected: Low = 10 - Min(5, (11-10)/2) = 10 - 0 = 10, Medium = 2 + (11-10)/3 = 2,
        // High = 1 + (11-10)/3 = 1, Elite = 1 + (11-10)/4 = 1, Boss = (11 % 5 == 0) ? 0
        var lowMonster = CreateMonster("Goblin", level, ThreatLevel.Low);
        var mediumMonster = CreateMonster("Orc", level, ThreatLevel.Medium);
        var highMonster = CreateMonster("Troll", level, ThreatLevel.High);
        var eliteMonster = CreateMonster("Warlord", level, ThreatLevel.Elite);

        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Low)).Returns([lowMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Medium)).Returns([mediumMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.High)).Returns([highMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Elite)).Returns([eliteMonster]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: true);

        // Assert
        Assert.AreEqual(10, monsters.Count(m => m.ThreatLevel == ThreatLevel.Low));
        Assert.AreEqual(2, monsters.Count(m => m.ThreatLevel == ThreatLevel.Medium));
        Assert.AreEqual(1, monsters.Count(m => m.ThreatLevel == ThreatLevel.High));
        Assert.AreEqual(1, monsters.Count(m => m.ThreatLevel == ThreatLevel.Elite));
        Assert.AreEqual(0, monsters.Count(m => m.ThreatLevel == ThreatLevel.Boss));
    }

    [TestMethod]
    public void GenerateMonsters_Level15_IncludesBoss()
    {
        // Arrange
        var level = 15;
        // Expected: Low = 10 - Min(5, (15-10)/2) = 10 - 2 = 8, Medium = 2 + (15-10)/3 = 3,
        // High = 1 + (15-10)/3 = 2, Elite = 1 + (15-10)/4 = 2, Boss = (15 % 5 == 0) ? 1
        var lowMonster = CreateMonster("Goblin", level, ThreatLevel.Low);
        var mediumMonster = CreateMonster("Orc", level, ThreatLevel.Medium);
        var highMonster = CreateMonster("Troll", level, ThreatLevel.High);
        var eliteMonster = CreateMonster("Warlord", level, ThreatLevel.Elite);
        var bossMonster = CreateBossMonster(level);

        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Low)).Returns([lowMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Medium)).Returns([mediumMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.High)).Returns([highMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Elite)).Returns([eliteMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Boss)).Returns([bossMonster]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: true);

        // Assert
        Assert.AreEqual(8, monsters.Count(m => m.ThreatLevel == ThreatLevel.Low));
        Assert.AreEqual(3, monsters.Count(m => m.ThreatLevel == ThreatLevel.Medium));
        Assert.AreEqual(2, monsters.Count(m => m.ThreatLevel == ThreatLevel.High));
        Assert.AreEqual(2, monsters.Count(m => m.ThreatLevel == ThreatLevel.Elite));
        Assert.AreEqual(1, monsters.Count(m => m.ThreatLevel == ThreatLevel.Boss));
    }

    [TestMethod]
    public void GenerateMonsters_Level20_MaxLevelComposition()
    {
        // Arrange
        var level = 20;
        // Expected: Low = 10 - Min(5, (20-10)/2) = 10 - 5 = 5, Medium = 2 + (20-10)/3 = 5,
        // High = 1 + (20-10)/3 = 4, Elite = 1 + (20-10)/4 = 3, Boss = (20 % 5 == 0) ? 1
        var lowMonster = CreateMonster("Goblin", level, ThreatLevel.Low);
        var mediumMonster = CreateMonster("Orc", level, ThreatLevel.Medium);
        var highMonster = CreateMonster("Troll", level, ThreatLevel.High);
        var eliteMonster = CreateMonster("Warlord", level, ThreatLevel.Elite);
        var bossMonster = CreateBossMonster(level);

        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Low)).Returns([lowMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Medium)).Returns([mediumMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.High)).Returns([highMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Elite)).Returns([eliteMonster]);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, ThreatLevel.Boss)).Returns([bossMonster]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: true);

        // Assert
        Assert.AreEqual(5, monsters.Count(m => m.ThreatLevel == ThreatLevel.Low));
        Assert.AreEqual(5, monsters.Count(m => m.ThreatLevel == ThreatLevel.Medium));
        Assert.AreEqual(4, monsters.Count(m => m.ThreatLevel == ThreatLevel.High));
        Assert.AreEqual(3, monsters.Count(m => m.ThreatLevel == ThreatLevel.Elite));
        Assert.AreEqual(1, monsters.Count(m => m.ThreatLevel == ThreatLevel.Boss));
    }
}
