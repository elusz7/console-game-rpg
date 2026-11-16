using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Models.Attributes;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Factories;

[TestClass]
public class MonsterFactoryAdventureTests
{
    private readonly Mock<IMonsterDao> _monsterDaoMock = new();
    private readonly Mock<ISkillDao> _skillDaoMock = new();
    private readonly Mock<IMonsterSkillSelector> _skillSelectorMock = new();
    private readonly Mock<IRandomProvider> _randomProviderMock = new();
    
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
        var elementIndex = 0;

        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>()))
            .Returns((int max) => elementIndex++ % max);

        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => min);

        return new MonsterFactory(
            _monsterDaoMock.Object,
            _skillDaoMock.Object,
            _skillSelectorMock.Object,
            _randomProviderMock.Object);
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

    [TestMethod]
    public void GenerateMonsters_NonCampaignMode_GeneratesAvailableMonsters()
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
        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        // Assert
        Assert.IsTrue(monsters.Count > 0);
        Assert.IsTrue(monsters.All(m => m.Name.StartsWith("Orc")));

        foreach (var monster in monsters)
        {
            if (monster.ThreatLevel == ThreatLevel.Low)
            {
                Assert.IsNull(monster.AttackElement);
                Assert.IsNull(monster.ElementalPower);
                Assert.IsNull(monster.Vulnerability);
                Assert.IsNull(monster.Resilience);
            }
            else
            {
                Assert.IsTrue(monster.AttackElement.HasValue);
                Assert.IsTrue(ValidElements.Contains(monster.AttackElement.Value));
                var expectedPower = Math.Max(1, (int)Math.Round(monster.AttackPower * 0.15, MidpointRounding.AwayFromZero));
                Assert.AreEqual(expectedPower, monster.ElementalPower,
                    $"ElementalPower {monster.ElementalPower} should be {expectedPower} for AttackPower {monster.AttackPower}");

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
        }
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
        Assert.ThrowsException<InvalidOperationException>(() => factory.GenerateMonsters(level, campaign: false, roomCount: 10));
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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.Count > 0);
        Assert.IsTrue(monsters.All(m =>
            m.Skills.Any(s => s.Name == "Heal")));

        // Ensure the assigned skill is a clone, not the DAO instance.
        Assert.IsFalse(monsters.Any(m =>
            m.Skills.Any(s => ReferenceEquals(s, skill))));
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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.AttackElement.HasValue &&
            ValidElements.Contains(m.AttackElement.Value)));
    }
    
    [TestMethod]
    public void GenerateMonsters_AssignsValidVulnerability()
    {
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.High);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.Vulnerability.HasValue &&
            ValidElements.Contains(m.Vulnerability.Value)));
    }
    
    [TestMethod]
    public void GenerateMonsters_AssignsValidResilience()
    {
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.Elite);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.Resilience.HasValue &&
            ValidElements.Contains(m.Resilience.Value)));
    }
    
    [TestMethod]
    public void GenerateMonsters_AttackElementAndVulnerabilityAreDifferent()
    {
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.High);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.AttackElement != m.Vulnerability));
    }
    
    [TestMethod]
    public void GenerateMonsters_ResilienceElementAndVulnerabilityAreDifferent()
    {
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.Elite);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.Resilience != m.Vulnerability));
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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.ElementalPower.HasValue &&
            m.ElementalPower.Value == Math.Max(
                1,
                (int)Math.Round(
                    m.AttackPower * 0.15,
                    MidpointRounding.AwayFromZero))));
    }
    
    [TestMethod]
    public void GenerateMonsters_LowThreat_HasNoElementalProperties()
    {
        var level = 3;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            !m.AttackElement.HasValue &&
            !m.ElementalPower.HasValue &&
            !m.Vulnerability.HasValue &&
            !m.Resilience.HasValue));
    }
    
    [TestMethod]
    public void GenerateMonsters_MediumThreat_HasNoVulnerabilityOrResilience()
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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.AttackElement.HasValue &&
            m.ElementalPower.HasValue &&
            !m.Vulnerability.HasValue &&
            !m.Resilience.HasValue));
    }
    
    [TestMethod]
    public void GenerateMonsters_HighThreat_HasNoResilience()
    {
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.High);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.AttackElement.HasValue &&
            m.ElementalPower.HasValue &&
            m.Vulnerability.HasValue &&
            !m.Resilience.HasValue));
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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.OfType<MagicSkill>().Any()));

        Assert.IsFalse(monsters.Any(m =>
            m.Skills.OfType<MartialSkill>().Any()));
    }
    
    [TestMethod]
    public void GenerateMonsters_HybridMonsterReceivesAttackSkills()
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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsFalse(monsters
            .SelectMany(m => m.Skills)
            .Any(s => s is MartialSkill || s is MagicSkill));
    }
    
    private static UltimateSkill CreateUltimateSkill(string name) =>
        new() { Name = name, RequiredLevel = 1 };
    
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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            m.Skills.OfType<UltimateSkill>().Count() == 1));
        
        var generatedElite = monsters.OfType<EliteMonster>().Single();

        Assert.AreEqual(
            1,
            generatedElite.Skills.OfType<UltimateSkill>().Count());
    }
    
    [TestMethod]
    public void GenerateMonsters_EliteWithOneUltimateDoesNotReceiveAnother()
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

        var monsters = factory.GenerateMonsters(
            level, campaign: false, roomCount: 10);

        var generatedElite = monsters.OfType<EliteMonster>().Single();

        var ultimateSkills = generatedElite.Skills
            .OfType<UltimateSkill>()
            .ToList();

        Assert.AreEqual(1, ultimateSkills.Count);
        Assert.AreEqual("Apocalypse", ultimateSkills[0].Name);
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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

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

        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        Assert.IsTrue(monsters.All(m =>
            !m.Skills.OfType<UltimateSkill>().Any()));
    }
    
    [TestMethod]
    public void GenerateMonsters_ElementalPowerIs15PercentOfAttackPower()
    {
        // Arrange
        var level = 20;
        var factory = CreateFactory();

        var attackPowers = new[] { 10, 20, 50, 100 };
        var expectedPowers = new List<int>();

        foreach (var attackPower in attackPowers)
        {
            var monster = CreateMonster("Orc", level, ThreatLevel.Medium);
            monster.AttackPower = attackPower;

            _monsterDaoMock
                .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
                .Returns([monster]);

            _skillDaoMock
                .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
                .Returns([]);

            var generated = factory.GenerateMonsters(
                level, campaign: false, roomCount: 10);

            var expectedPower = Math.Max(
                1,
                (int)Math.Round(
                    attackPower * 0.15,
                    MidpointRounding.AwayFromZero));

            expectedPowers.Add(expectedPower);

            Assert.IsTrue(
                generated.All(m => m.ElementalPower == expectedPower),
                $"All monsters with AttackPower {attackPower} should have ElementalPower {expectedPower}");
        }

        // Assert
        for (var i = 1; i < expectedPowers.Count; i++)
        {
            Assert.IsTrue(
                expectedPowers[i] > expectedPowers[i - 1],
                $"ElementalPower for AttackPower {attackPowers[i]} ({expectedPowers[i]}) should be greater than for AttackPower {attackPowers[i - 1]} ({expectedPowers[i - 1]})");
        }
    }

    [TestMethod]
    public void GenerateMonsters_LowAttackPower_ElementalPowerIsAtLeast1()
    {
        // Arrange: Use Medium threat so monsters have ElementalPower
        var level = 1;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Medium);
        monster.AttackPower = 1;

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();
        var monsters = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        // Act & Assert: Even with very low AttackPower, ElementalPower should be at least 1
        Assert.IsTrue(monsters.All(m => m.ElementalPower!.Value == 1),
            "ElementalPower should be at least 1 for low-level monsters with elemental properties");
    }
    
    [TestMethod]
    public void GenerateMonsters_AttackElementCanEqualResilience()
    {
        // Arrange
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.Elite);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        // Candidate selection uses Next(1), so it must not consume
        // the sequence used for selecting elemental properties.
        var elementSequence = new[] { 0, 1, 0 };
        var elementIndex = 0;

        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>()))
            .Returns((int max) =>
            {
                if (max == 6)
                    return elementSequence[elementIndex++ % elementSequence.Length];

                return 0;
            });

        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => min);

        var factory = new MonsterFactory(
            _monsterDaoMock.Object,
            _skillDaoMock.Object,
            _skillSelectorMock.Object,
            _randomProviderMock.Object);

        // Act
        var generated = factory.GenerateMonsters(
            level, campaign: false, roomCount: 1);

        // Assert
        var elite = generated.Single();

        Assert.AreEqual(ElementType.Fire, elite.AttackElement);
        Assert.AreEqual(ElementType.Ice, elite.Vulnerability);
        Assert.AreEqual(ElementType.Fire, elite.Resilience);

        Assert.AreEqual(elite.AttackElement, elite.Resilience);
    }

    [TestMethod]
    public void GenerateMonsters_BaseSkillsAreClonedIndependently()
    {
        // Arrange: A base monster with a skill should have that skill cloned
        // so the generated monster's skill is an independent copy.
        var level = 2;
        var baseSkill = new SupportSkill
        {
            Name = "Regenerate",
            RequiredLevel = 1
        };
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);
        monster.Skills.Add(baseSkill);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        // Act: Generate two batches, each should have independent clones of the base skill
        var firstBatch = factory.GenerateMonsters(level, campaign: false, roomCount: 10);
        var secondBatch = factory.GenerateMonsters(level, campaign: false, roomCount: 10);

        var firstBaseSkills = firstBatch.SelectMany(m => m.Skills)
            .Where(s => s.Name == "Regenerate").ToList();
        var secondBaseSkills = secondBatch.SelectMany(m => m.Skills)
            .Where(s => s.Name == "Regenerate").ToList();

        // Assert: Each generated monster should have a clone, not the original
        Assert.IsFalse(firstBaseSkills.Any(s => ReferenceEquals(s, baseSkill)),
            "Generated monster should have a clone of the base skill, not the original");
        Assert.IsFalse(secondBaseSkills.Any(s => ReferenceEquals(s, baseSkill)),
            "Second batch should also have clones, not the original");

        // Assert: The two batches should have independent clones (different references)
        foreach (var s1 in firstBaseSkills)
        {
            Assert.IsTrue(secondBaseSkills.All(s2 => !ReferenceEquals(s1, s2)),
                "Clones from different batches should be independent references");
        }
    }
    
    [TestMethod]
    public void GenerateMonsters_DoesNotExceedTwiceTheRoomCount()
    {
        // Arrange
        var level = 10;
        var roomCount = 2;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        // Act
        var generated = factory.GenerateMonsters(
            level, campaign: false, roomCount);

        // Assert
        Assert.AreEqual(roomCount * 2, generated.Count);
    }
    
    [DataTestMethod]
    [DataRow(1, 1)]
    [DataRow(9, 1)]
    [DataRow(10, 1)]
    [DataRow(20, 2)]
    public void GenerateMonsters_EliteCountDoesNotExceedRoomLimit(
        int roomCount,
        int expectedEliteCount)
    {
        // Arrange
        var level = 10;
        var elite = CreateEliteMonster(level);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([elite]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        // Act
        var generated = factory.GenerateMonsters(
            level, campaign: false, roomCount);

        // Assert
        Assert.AreEqual(
            expectedEliteCount,
            generated.Count(m => m.ThreatLevel == ThreatLevel.Elite));

        Assert.AreEqual(expectedEliteCount, generated.Count);
    }
    
    
    [TestMethod]
    public void GenerateMonsters_StopsWhenNoCandidateFitsRemainingBudget()
    {
        // Arrange
        var level = 1;
        var roomCount = 1;

        var monster = CreateMonster("Orc", level, ThreatLevel.Low);

        monster.AttackPower = 5;
        monster.DefensePower = 1;
        monster.Resistance = 1;

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        // Act
        var generated = factory.GenerateMonsters(
            level, campaign: false, roomCount);

        // Assert
        Assert.AreEqual(1, generated.Count);
        Assert.IsTrue(generated[0].Name.StartsWith("Orc"));
    }
}