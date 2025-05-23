using System.Reflection;
using System.Text.RegularExpressions;
using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.ConsoleGame.Factories;

[TestClass]
public class MonsterFactoryTests
{
    private readonly Mock<IMonsterDao> _monsterDaoMock = new();
    private readonly Mock<ISkillDao> _skillDaoMock = new();
    private readonly Mock<IMonsterSkillSelector> _skillSelectorMock = new();

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

    [TestMethod]
    public void GenerateMonsters_CampaignMode_ReturnsExpectedMonsters()
    {
        // Arrange
        var level = 2;
        var threat = ThreatLevel.Low;
        var monster = CreateMonster("Goblin", level, threat);
        _monsterDaoMock.Setup(m => m.GetMonstersByMaxLevelAndThreatLevel(level, It.IsAny<int>()))
            .Returns([monster]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level)).Returns([]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: true);

        // Assert
        Assert.IsTrue(monsters.Count > 0);
        Assert.IsTrue(monsters.All(m => m.Level == level));
        Assert.IsTrue(monsters.All(m => m.MonsterType == "TestType"));
    }

    [TestMethod]
    public void GenerateMonsters_NonCampaignMode_ReturnsExpectedMonsters()
    {
        // Arrange
        var level = 3;
        var monster1 = CreateMonster("Orc", level, ThreatLevel.Medium);
        var monster2 = CreateMonster("Rat", level, ThreatLevel.Low);
        var availableMonsters = new List<Monster>
        {
            monster1
        };


        _monsterDaoMock.Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns(availableMonsters);

        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns(new List<Skill>());

        var factory = CreateFactory(); // make sure this uses _monsterDaoMock.Object

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: false);

        // Assert
        Assert.IsTrue(monsters.Count > 0);
        Assert.IsTrue(monsters.All(m => m.Name.StartsWith("Orc")));
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

    /*[TestMethod]
    public void GenerateMonsters_AssignsSkillsToMonsters()
    {
        // Arrange
        var level = 5;
        var monster = CreateMonster("Slime", level, ThreatLevel.Low);
        var skill = new SupportSkill { Name = "Heal", RequiredLevel = 1 };
        _monsterDaoMock.Setup(m => m.GetNonBossMonstersByMaxLevel(level))
            .Returns([monster]);
        _skillDaoMock.Setup(s => s.GetUnassignedSkillsByMaxLevel(level))
            .Returns([skill]);

        var factory = CreateFactory();

        // Act
        var monsters = factory.GenerateMonsters(level, campaign: false);

        // Assert
        Assert.IsTrue(monsters.Any(m => m.Skills.Any(s => s.Name == "Heal")));
    }*/
}