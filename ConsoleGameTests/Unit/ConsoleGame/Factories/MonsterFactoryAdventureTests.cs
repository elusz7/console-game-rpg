using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
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
        var monster = CreateMonster("Orc", level, ThreatLevel.High);

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

        var monsters = factory.GenerateMonsters(level, campaign: false);

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

        var monsters = factory.GenerateMonsters(level, campaign: false);

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

        var monsters = factory.GenerateMonsters(level, campaign: false);

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

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
        {
            if (m.ElementalPower == null) return true;
            var expectedPower = Math.Max(1, (int)Math.Round(m.AttackPower * 0.15, MidpointRounding.AwayFromZero));
            return m.ElementalPower == expectedPower;
        }));
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

        var monsters = factory.GenerateMonsters(level, campaign: false);

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

        var monsters = factory.GenerateMonsters(level, campaign: false);

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

        var monsters = factory.GenerateMonsters(level, campaign: false);

        Assert.IsTrue(monsters.All(m =>
            m.AttackElement.HasValue &&
            m.ElementalPower.HasValue &&
            m.Vulnerability.HasValue &&
            !m.Resilience.HasValue));
    }
    
    [TestMethod]
    public void GenerateMonsters_SameBaseMonster_DifferentElementalProperties()
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

        // Generate the same base monster multiple times
        var first = factory.GenerateMonsters(level, campaign: false)[0];
        var second = factory.GenerateMonsters(level, campaign: false)[0];

        // At least one elemental property should differ between generations
        var different = first.AttackElement != second.AttackElement ||
                        first.Vulnerability != second.Vulnerability ||
                        first.Resilience != second.Resilience;

        Assert.IsTrue(different,
            "Repeated generation of the same base monster should produce different elemental properties");
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
    public void GenerateMonsters_ElementalPowerScalesWithAttackPower()
    {
        // Arrange: Use Medium threat so monsters have ElementalPower
        // ElementalPower is calculated as 15% of AttackPower, so it scales with AttackPower
        var factory = CreateFactory();

        var attackPowers = new[] { 10, 20, 50, 100 };
        var expectedPowers = new List<int>();

        foreach (var attackPower in attackPowers)
        {
            var monster = CreateMonster("Orc", 1, ThreatLevel.Medium);
            monster.AttackPower = attackPower;

            _monsterDaoMock
                .Setup(m => m.GetNonBossMonstersByMaxLevel(1))
                .Returns([monster]);

            _skillDaoMock
                .Setup(s => s.GetUnassignedSkillsByMaxLevel(1))
                .Returns([]);

            var generated = factory.GenerateMonsters(1, campaign: false);
            var powers = generated.Where(m => m.ElementalPower.HasValue)
                .Select(m => m.ElementalPower!.Value);
            var expectedPower = Math.Max(1, (int)Math.Round(attackPower * 0.15, MidpointRounding.AwayFromZero));
            expectedPowers.Add(expectedPower);

            Assert.IsTrue(powers.All(p => p == expectedPower),
                $"All monsters with AttackPower {attackPower} should have ElementalPower {expectedPower}");
        }

        // Act & Assert: Higher AttackPower should yield higher ElementalPower
        for (var i = 1; i < expectedPowers.Count; i++)
        {
            Assert.IsTrue(
                expectedPowers[i] > expectedPowers[i - 1],
                $"ElementalPower for AttackPower {attackPowers[i]} ({expectedPowers[i]}) should be greater than for AttackPower {attackPowers[i - 1]} ({expectedPowers[i - 1]})");
        }
    }

    [TestMethod]
    public void GenerateMonsters_ElementalPowerSameForSameAttackPower()
    {
        // Arrange: ElementalPower depends only on AttackPower (15%), not on threat level
        // So monsters with the same AttackPower should have the same ElementalPower regardless of threat
        var level = 10;
        var threats = new[] { ThreatLevel.Medium, ThreatLevel.High, ThreatLevel.Elite };
        var attackPower = 10; // fixed AttackPower from CreateMonster
        var expectedPower = Math.Max(1, (int)Math.Round(attackPower * 0.15, MidpointRounding.AwayFromZero));

        foreach (var threat in threats)
        {
            var monster = CreateMonster("Orc", level, threat);

            _monsterDaoMock
                .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
                .Returns([monster]);

            _skillDaoMock
                .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
                .Returns([]);

            var factory = CreateFactory();
            var generated = factory.GenerateMonsters(level, campaign: false);

            Assert.IsTrue(generated.All(m => m.ElementalPower == expectedPower),
                $"All {threat} monsters with AttackPower {attackPower} should have ElementalPower {expectedPower}");
        }
    }

    [TestMethod]
    public void GenerateMonsters_ElementalPowerRemainsProportionateToAttackPower()
    {
        // Arrange: Use Medium+ threat so monsters have ElementalPower
        var level = 10;
        var monster = CreateMonster("Orc", level, ThreatLevel.Medium);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();
        var monsters = factory.GenerateMonsters(level, campaign: false);

        // Act & Assert: ElementalPower should be roughly 10-20% of AttackPower
        foreach (var m in monsters)
        {
            if (!m.ElementalPower.HasValue) continue;
            var ratio = (double)m.ElementalPower.Value / m.AttackPower;
            Assert.IsTrue(
                ratio >= 0.05 && ratio <= 0.25,
                $"ElementalPower ({m.ElementalPower}) / AttackPower ({m.AttackPower}) = {ratio:F2} is outside acceptable range [0.05, 0.25]");
        }
    }

    [TestMethod]
    public void GenerateMonsters_LowAttackPower_ElementalPowerIsAtLeast1()
    {
        // Arrange: Use Medium threat so monsters have ElementalPower
        var level = 1;
        var monster = CreateMonster("Goblin", level, ThreatLevel.Medium);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();
        var monsters = factory.GenerateMonsters(level, campaign: false);

        // Act & Assert: Even with very low AttackPower, ElementalPower should be at least 1
        Assert.IsTrue(monsters.All(m => m.ElementalPower!.Value >= 1),
            "ElementalPower should be at least 1 for low-level monsters with elemental properties");
    }

    [TestMethod]
    public void GenerateMonsters_LargeSample_ElementalPowerConsistency()
    {
        // Arrange: Use Medium threat so monsters have ElementalPower
        // With fixed AttackPower, all monsters should have the same ElementalPower
        var level = 10;
        var monster = CreateMonster("Orc", level, ThreatLevel.Medium);
        var expectedPower = Math.Max(1, (int)Math.Round(monster.AttackPower * 0.15, MidpointRounding.AwayFromZero));

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        // Generate 100 monsters to check consistency
        var allPowers = new List<int>();
        for (var i = 0; i < 100; i++)
        {
            var generated = factory.GenerateMonsters(level, campaign: false);
            allPowers.AddRange(generated.Where(m => m.ElementalPower.HasValue)
                .Select(m => m.ElementalPower!.Value));
        }

        // Act & Assert: All monsters with the same AttackPower should have the same ElementalPower
        Assert.IsTrue(allPowers.All(p => p == expectedPower),
            $"All monsters with AttackPower {monster.AttackPower} should have ElementalPower {expectedPower}");

        Assert.IsTrue(allPowers.All(p => p >= 1),
            "All ElementalPower values should be at least 1");
    }

    [TestMethod]
    public void GenerateMonsters_AttackElementCanEqualResilience()
    {
        // Arrange: The factory ensures attackElement != vulnerability and vulnerability != resilience,
        // but does NOT guarantee attackElement != resilience. This test verifies that behavior.
        var level = 3;
        var monster = CreateMonster("Orc", level, ThreatLevel.Elite);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        // Act: Generate many batches to find at least one case where AttackElement == Resilience
        bool foundMatch = false;
        for (var i = 0; i < 100; i++)
        {
            var generated = factory.GenerateMonsters(level, campaign: false);
            foreach (var m in generated)
            {
                if (m.AttackElement == m.Resilience)
                {
                    foundMatch = true;
                    break;
                }
            }
            if (foundMatch) break;
        }

        // Assert: With 6 elements, the probability of at least one match in 100 batches is ~99.99%
        Assert.IsTrue(foundMatch,
            "AttackElement and Resilience are not constrained to be different; a match should occur in 100 generations");
    }

    [TestMethod]
    public void GenerateMonsters_ElementalPowerUsesRandomizedAttackPower()
    {
        // Arrange: SetLevel randomizes AttackPower based on threat level and level,
        // then SetElementalProperties calculates ElementalPower from the randomized AttackPower.
        // The base monster has Level=1, so SetLevel(10) will trigger randomization.
        var level = 10;
        var monster = CreateMonster("Orc", 1, ThreatLevel.Medium);
        // CreateMonster sets AttackPower = 10 and Level = 1, SetLevel(10) will randomize it to [30, 60]

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        // Act: Generate multiple monsters and collect their AttackPower/ElementalPower pairs
        var pairs = new List<(int AttackPower, int ElementalPower)>();
        for (var i = 0; i < 50; i++)
        {
            var generated = factory.GenerateMonsters(level, campaign: false);
            foreach (var m in generated)
            {
                if (m.ElementalPower.HasValue)
                {
                    pairs.Add((m.AttackPower, m.ElementalPower.Value));
                }
            }
        }

        // Assert: ElementalPower should be calculated from the randomized AttackPower,
        // not the base AttackPower of 10. Since SetLevel randomizes to [30, 60] for Medium at level 10,
        // the AttackPower values should vary and ElementalPower should match each one.
        Assert.IsTrue(pairs.Count > 0, "Should have generated monsters with ElementalPower");

        // AttackPower should vary due to SetLevel randomization (not all 10)
        var distinctAttackPowers = pairs.Select(p => p.AttackPower).Distinct().Count();
        Assert.IsTrue(distinctAttackPowers > 1,
            $"SetLevel should randomize AttackPower; found {distinctAttackPowers} distinct values");

        // Each ElementalPower should match its corresponding randomized AttackPower
        foreach (var (attackPower, elementalPower) in pairs)
        {
            var expectedPower = Math.Max(1, (int)Math.Round(attackPower * 0.15, MidpointRounding.AwayFromZero));
            Assert.AreEqual(expectedPower, elementalPower,
                $"ElementalPower {elementalPower} should be {expectedPower} for randomized AttackPower {attackPower}");
        }
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
        var firstBatch = factory.GenerateMonsters(level, campaign: false);
        var secondBatch = factory.GenerateMonsters(level, campaign: false);

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
    public void GenerateMonsters_NonCampaign_LowThreatProducesMoreMonsters()
    {
        // Arrange: Low-threat monsters contribute weight 0.5, so more are needed to reach the target.
        // With only Low-threat monsters available, the count should be roughly double the target weight.
        var level = 2;
        var targetCount = (int)Math.Round(level * 3.5); // = 7 weight units
        var monster = CreateMonster("Goblin", level, ThreatLevel.Low);

        _monsterDaoMock
            .Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);

        _skillDaoMock
            .Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: false);

        // Assert: Each Low monster contributes 0.5 weight, so we need ~14 monsters for 7 weight units
        // Allow some tolerance due to the while loop stopping when currentCount >= target
        var expectedMin = targetCount / 0.5 - 1; // ~13
        var expectedMax = targetCount / 0.5 + 1; // ~15
        Assert.IsTrue(monsters.Count >= expectedMin && monsters.Count <= expectedMax,
            $"Low-threat monsters (weight 0.5) should produce ~{targetCount / 0.5} monsters for target weight {targetCount}, got {monsters.Count}");
    }
}