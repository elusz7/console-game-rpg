using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Factories;

[TestClass]
public class MonsterFactoryTests
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

    private EliteMonster CreateEliteMonster(int level, params Skill[] skills)
    {
        return new EliteMonster(
            _skillSelectorMock.Object, (MonsterBehaviorType)1)
        {
            Name = "Elite Goblin",
            Level = level,
            ThreatLevel = ThreatLevel.Elite,
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
        Assert.IsTrue(monster.AttackElement.HasValue);
        Assert.IsTrue(ValidElements.Contains(monster.AttackElement.Value));

        Assert.IsTrue(monster.Vulnerability.HasValue);
        Assert.IsTrue(ValidElements.Contains(monster.Vulnerability.Value));

        Assert.AreNotEqual(monster.AttackElement, monster.Vulnerability);
        Assert.IsTrue(monster.ElementalPower is 1 or 2);
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
    public void GenerateMonsters_NonCampaignMode_ReturnsExpectedMonsters()
    {
        // Arrange
        var level = 3;
        var monster1 = CreateMonster("Orc", level, ThreatLevel.Medium);
        var availableMonsters = new List<Monster>
        {
            monster1
        };


        _monsterDaoMock.Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns(availableMonsters);

        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory(); // make sure this uses _monsterDaoMock.Object

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: false);

        // Assert
        Assert.IsTrue(monsters.Count > 0);
        Assert.IsTrue(monsters.All(m => m.Name.StartsWith("Orc")));

        foreach (var monster in monsters)
            AssertValidElementalProperties(monster);
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
    public void GenerateMonsters_NonCampaignMode_ThrowsIfNoMonsters()
    {
        // Arrange
        var level = 1;
        _monsterDaoMock.Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => factory.GenerateMonsters(level, campaign: false));
    }

    [TestMethod]
    public void GenerateMonsters_AssignsAvailableSupportSkills()
    {
        var level = 5;
        var monster = CreateMonster("Slime", level, ThreatLevel.Low);
        var skill = new SupportSkill
        {
            Name = "Heal",
            RequiredLevel = 1
        };

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([skill]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.Count > 0);
        Assert.IsTrue(monsters.All(m =>
            m.Skills.Any(s => s.Name == "Heal")));

        // Ensure the assigned skill is a clone, not the DAO instance.
        Assert.IsFalse(monsters.Any(m =>
            m.Skills.Any(s => ReferenceEquals(s, skill))));
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
    public void GenerateMonsters_AssignsValidAttackElement()
    {
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.Medium);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.AttackElement.HasValue &&
            ValidElements.Contains(m.AttackElement.Value)));
    }
    
    [TestMethod]
    public void GenerateMonsters_AssignsValidVulnerability()
    {
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.Medium);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.Vulnerability.HasValue &&
            ValidElements.Contains(m.Vulnerability.Value)));
    }
    
    [TestMethod]
    public void GenerateMonsters_AttackElementAndVulnerabilityAreDifferent()
    {
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.Medium);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.AttackElement != m.Vulnerability));
    }
    
    [TestMethod]
    public void GenerateMonsters_AssignsValidElementalPower()
    {
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.Medium);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.ElementalPower is 1 or 2));
    }
    
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
    public void GenerateMonsters_MartialMonsterReceivesMartialSkills()
    {
        var level = 2;
        var monster = CreateMonster(
            "Goblin", level, ThreatLevel.Medium, DamageType.Martial);

        var martialSkill = new MartialSkill
        {
            Name = "Slash",
            RequiredLevel = 1
        };

        var magicSkill = new MagicSkill
        {
            Name = "Fireball",
            RequiredLevel = 1
        };

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([martialSkill, magicSkill]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.OfType<MartialSkill>().Any()));

        Assert.IsFalse(monsters.Any(m =>
            m.Skills.OfType<MagicSkill>().Any()));
    }
    
    [TestMethod]
    public void GenerateMonsters_MagicalMonsterReceivesMagicSkills()
    {
        var level = 2;
        var monster = CreateMonster(
            "Mage", level, ThreatLevel.Medium, DamageType.Magical);

        var martialSkill = new MartialSkill
        {
            Name = "Slash",
            RequiredLevel = 1
        };

        var magicSkill = new MagicSkill
        {
            Name = "Fireball",
            RequiredLevel = 1
        };

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([martialSkill, magicSkill]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.OfType<MagicSkill>().Any()));

        Assert.IsFalse(monsters.Any(m =>
            m.Skills.OfType<MartialSkill>().Any()));
    }
    
    [TestMethod]
    public void GenerateMonsters_HybridMonsterReceivesMartialAndMagicSkills()
    {
        var level = 2;
        var monster = CreateMonster(
            "Chimera", level, ThreatLevel.Medium, DamageType.Hybrid);

        var skills = new List<Skill>
        {
            new MartialSkill { Name = "Slash", RequiredLevel = 1 },
            new MartialSkill { Name = "Strike", RequiredLevel = 1 },
            new MagicSkill { Name = "Fireball", RequiredLevel = 1 },
            new MagicSkill { Name = "Ice Shard", RequiredLevel = 1 }
        };

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns(skills);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.Any(s => s is MartialSkill || s is MagicSkill)));
    }
    
    [TestMethod]
    public void GenerateMonsters_DoesNotAssignAttackSkillsToUnsupportedDamageType()
    {
        var level = 2;
        var monster = CreateMonster("Oddity", level, ThreatLevel.Medium, (DamageType)999);

        var skills = new List<Skill>
        {
            new MartialSkill { Name = "Slash", RequiredLevel = 1 },
            new MagicSkill { Name = "Fireball", RequiredLevel = 1 }
        };

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns(skills);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            !m.Skills.Any(s => s is MartialSkill || s is MagicSkill)));
    }
    
    private static BossSkill CreateBossSkill(string name) =>
        new() { Name = name, RequiredLevel = 1 };

    private static UltimateSkill CreateUltimateSkill(string name) =>
        new() { Name = name, RequiredLevel = 1 };
    
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
    
    [TestMethod]
    public void GenerateMonsters_EliteWithoutUltimateReceivesUltimateSkill()
    {
        var level = 2;
        var elite = CreateEliteMonster(level);

        var ultimate = CreateUltimateSkill("Apocalypse");

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([elite]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([ultimate]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.OfType<UltimateSkill>().Count() == 1));
    }
    
    [TestMethod]
    public void GenerateMonsters_EliteWithOneUltimateReceivesAnother()
    {
        var level = 2;

        var existingUltimate = CreateUltimateSkill("Apocalypse");
        var elite = CreateEliteMonster(level, existingUltimate);

        var availableUltimate = CreateUltimateSkill("Armageddon");

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([elite]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([availableUltimate]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.OfType<UltimateSkill>().Count() == 2));
    }
    
    [TestMethod]
    public void GenerateMonsters_EliteWithTwoUltimatesDoesNotReceiveAnother()
    {
        var level = 2;

        var elite = CreateEliteMonster(
            level,
            CreateUltimateSkill("Apocalypse"),
            CreateUltimateSkill("Armageddon"));

        var availableUltimate = CreateUltimateSkill("Oblivion");

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([elite]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([availableUltimate]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.OfType<UltimateSkill>().Count() == 2));
    }
    
    [TestMethod]
    public void GenerateMonsters_NonEliteDoesNotReceiveUltimateSkills()
    {
        var level = 2;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Medium);

        var ultimate = CreateUltimateSkill("Apocalypse");

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([ultimate]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            !m.Skills.OfType<UltimateSkill>().Any()));
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
}