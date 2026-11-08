using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Integration.ConsoleGame.GameDao;

[TestClass]
public class MonsterDaoTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private MonsterDao _monsterDao = null!;

    //private static readonly string[] AddSkillNames = ["Power Strike", "Shield Bash"];
    //private static readonly string[] UpdateSkillNames = ["Power Strike", "Whirlwind"];
    //private static readonly string[] MartialArchetypeNames = ["Warrior", "Knight"];
    //private static readonly string[] MagicalArchetypeNames = ["Mage", "Sorcerer"];

    [TestInitialize]
    public void Initialize()
    {
        // Create and open an isolated in-memory SQLite database.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // Configure the real GameContext to use SQLite.
        var options = new DbContextOptionsBuilder<GameContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new GameContext(options);

        // Create the database schema.
        _context.Database.EnsureCreated();

        // Use the real DAO
        _monsterDao = new MonsterDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void AddMonster_PersistsMonster()
    {
        // Arrange
        var monster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);

        // Act
        _monsterDao.AddMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsTrue(persisted.Id > 0);
        Assert.AreEqual(monster.Name, persisted.Name);
    }

    [TestMethod]
    public void AddMonster_PersistsAllMonsterProperties()
    {
        // Arrange
        var monster = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);

        // Act
        _monsterDao.AddMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Single(m => m.Id == monster.Id);

        // Assert
        AssertMonsterPropertiesAreEqual(monster, persisted);
    }

    [TestMethod]
    public void AddMonster_PersistsMonsterWithSkills()
    {
        // Arrange
        var monster = CreateMonster("Goblin Warrior", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Boss);

        var skill1 = CreateSkill("Power Strike", TestSkillType.Martial);
        var skill2 = CreateSkill("Whirlwind", TestSkillType.Ultimate);

        monster.Skills = [skill1, skill2];

        // Act
        _monsterDao.AddMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Include(m => m.Skills)
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.AreEqual(2, persisted.Skills.Count);
        CollectionAssert.AreEquivalent(
            new[] { skill1.Name, skill2.Name },
            persisted.Skills.Select(s => s.Name).ToArray());
    }

    [TestMethod]
    public void AddMonster_PersistsMonsterWithTreasure()
    {
        // Arrange
        var monster = CreateMonster("Treasure Guardian", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);
        var treasure = CreateItem("Guardian's Sword", 5, 20, TestItemType.Weapon);

        monster.SetLoot(treasure);

        // Act
        _monsterDao.AddMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Include(m => m.Treasure)
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsNotNull(persisted.Treasure);
        Assert.AreEqual(treasure.Name, persisted.Treasure.Name);
        Assert.AreEqual(treasure.Id, persisted.Treasure.Id);
    }


    [TestMethod]
    public void AddMonster_PersistsMonsterWithRoom()
    {
        // Arrange
        var room = CreateRoom("Dragon's Lair");
        _context.Rooms.Add(room);
        _context.SaveChanges();

        var monster = CreateMonster("Cave Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);
        monster.Room = room;
        monster.RoomId = room.Id;

        // Act
        _monsterDao.AddMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Include(m => m.Room)
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsNotNull(persisted.Room);
        Assert.AreEqual(room.Id, persisted.Room.Id);
        Assert.AreEqual(room.Name, persisted.Room.Name);
        Assert.AreEqual(room.Id, persisted.RoomId);
    }


    [TestMethod]
    public void AddMonster_PersistsIsCore()
    {
        // Arrange
        var monster = CreateMonster("Core Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Elite);
        monster.IsCore = true;

        // Act
        _monsterDao.AddMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsTrue(persisted.IsCore);
    }

    [TestMethod]
    public void AddMonster_PersistsEliteMonster()
    {
        // Arrange
        var monster = CreateMonster("Elite Goblin", ThreatLevel.Elite, DamageType.Martial, TestMonsterType.Elite);

        // Act
        _monsterDao.AddMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsInstanceOfType<EliteMonster>(persisted);
        AssertMonsterPropertiesAreEqual(monster, persisted);
    }

    [TestMethod]
    public void AddMonster_PersistsBossMonster()
    {
        // Arrange
        var monster = CreateMonster("Ancient Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);

        // Act
        _monsterDao.AddMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsInstanceOfType<BossMonster>(persisted);
        AssertMonsterPropertiesAreEqual(monster, persisted);
    }

    private static void AssertMonsterPropertiesAreEqual(Monster expected, Monster actual)
    {
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.Description, actual.Description);
        Assert.AreEqual(expected.Level, actual.Level);
        Assert.AreEqual(expected.MaxHealth, actual.MaxHealth);
        Assert.AreEqual(expected.ThreatLevel, actual.ThreatLevel);
        Assert.AreEqual(expected.AggressionLevel, actual.AggressionLevel);
        Assert.AreEqual(expected.MonsterType, actual.MonsterType);
        Assert.AreEqual(expected.IsCore, actual.IsCore);
        Assert.AreEqual(expected.DefensePower, actual.DefensePower);
        Assert.AreEqual(expected.AttackPower, actual.AttackPower);
        Assert.AreEqual(expected.DamageType, actual.DamageType);
        Assert.AreEqual(expected.Resistance, actual.Resistance);
    }

    [TestMethod]
    public void UpdateMonster_PersistsUpdatedProperties()
    {
        // Arrange
        var monster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        _monsterDao.AddMonster(monster);

        monster.Name = "Updated Goblin";
        monster.Description = "An updated monster description.";
        monster.Level = 5;
        monster.MaxHealth = 250;
        monster.ThreatLevel = ThreatLevel.High;
        monster.AggressionLevel = 9;
        monster.DefensePower = 20;
        monster.AttackPower = 30;
        monster.DamageType = DamageType.Magical;
        monster.Resistance = 15;

        // Act
        _monsterDao.UpdateMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Single(m => m.Id == monster.Id);

        // Assert
        AssertMonsterPropertiesAreEqual(monster, persisted);
    }

    [TestMethod]
    public void UpdateMonster_PersistsUpdatedIsCore()
    {
        // Arrange
        var monster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        _monsterDao.AddMonster(monster);

        monster.IsCore = true;

        // Act
        _monsterDao.UpdateMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsTrue(persisted.IsCore);
    }

    [TestMethod]
    public void UpdateMonster_PersistsUpdatedSkills()
    {
        // Arrange
        var monster = CreateMonster("Goblin", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);

        var originalSkill = CreateSkill("Power Strike", TestSkillType.Martial);
        monster.Skills = [originalSkill];

        _monsterDao.AddMonster(monster);

        var updatedSkill = CreateSkill("Whirlwind", TestSkillType.Ultimate);
        monster.Skills = [updatedSkill];

        // Act
        _monsterDao.UpdateMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Include(m => m.Skills)
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.AreEqual(1, persisted.Skills.Count);
        Assert.AreEqual(updatedSkill.Name, persisted.Skills.Single().Name);

        var persistedOriginalSkill = _context.Skills
            .Single(s => s.Id == originalSkill.Id);

        Assert.IsNull(persistedOriginalSkill.MonsterId);
    }

    [TestMethod]
    public void UpdateMonster_PersistsUpdatedTreasure()
    {
        // Arrange
        var monster = CreateMonster("Treasure Guardian", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);

        var originalTreasure = CreateItem("Old Sword", 3, 10, TestItemType.Weapon);
        monster.SetLoot(originalTreasure);

        _monsterDao.AddMonster(monster);

        var updatedTreasure = CreateItem("New Sword", 5, 25, TestItemType.Weapon);
        monster.SetLoot(updatedTreasure);

        // Act
        _monsterDao.UpdateMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Include(m => m.Treasure)
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsNotNull(persisted.Treasure);
        Assert.AreEqual(updatedTreasure.Name, persisted.Treasure.Name);
        Assert.AreEqual(updatedTreasure.Id, persisted.Treasure.Id);
    }

    [TestMethod]
    public void UpdateMonster_PersistsUpdatedRoom()
    {
        // Arrange
        var originalRoom = CreateRoom("Original Room");
        var updatedRoom = CreateRoom("Updated Room");

        _context.Rooms.AddRange(originalRoom, updatedRoom);
        _context.SaveChanges();

        var monster = CreateMonster("Cave Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);
        monster.Room = originalRoom;
        monster.RoomId = originalRoom.Id;

        _monsterDao.AddMonster(monster);

        monster.Room = updatedRoom;
        monster.RoomId = updatedRoom.Id;

        // Act
        _monsterDao.UpdateMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Include(m => m.Room)
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsNotNull(persisted.Room);
        Assert.AreEqual(updatedRoom.Id, persisted.Room.Id);
        Assert.AreEqual(updatedRoom.Name, persisted.Room.Name);
        Assert.AreEqual(updatedRoom.Id, persisted.RoomId);
    }

    [TestMethod]
    public void UpdateMonster_PersistsEliteMonsterProperties()
    {
        // Arrange
        var monster = CreateMonster("Elite Goblin", ThreatLevel.Elite, DamageType.Martial, TestMonsterType.Elite);
        _monsterDao.AddMonster(monster);

        monster.Name = "Updated Elite Goblin";
        monster.Level = 10;
        monster.MaxHealth = 500;
        monster.AttackPower = 75;
        monster.DefensePower = 50;

        // Act
        _monsterDao.UpdateMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsInstanceOfType<EliteMonster>(persisted);
        AssertMonsterPropertiesAreEqual(monster, persisted);
    }

    [TestMethod]
    public void UpdateMonster_PersistsBossMonsterProperties()
    {
        // Arrange
        var monster = CreateMonster("Ancient Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);
        _monsterDao.AddMonster(monster);

        monster.Name = "Updated Ancient Dragon";
        monster.Level = 25;
        monster.MaxHealth = 5000;
        monster.AttackPower = 250;
        monster.DefensePower = 200;

        // Act
        _monsterDao.UpdateMonster(monster);

        _context.ChangeTracker.Clear();

        var persisted = _context.Monsters
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsInstanceOfType<BossMonster>(persisted);
        AssertMonsterPropertiesAreEqual(monster, persisted);
    }

    [TestMethod]
    public void DeleteMonster_RemovesNonCoreMonster()
    {
        // Arrange
        var monster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        _monsterDao.AddMonster(monster);

        // Act
        _monsterDao.DeleteMonster(monster);

        _context.ChangeTracker.Clear();

        // Assert
        Assert.IsFalse(_context.Monsters.Any(m => m.Id == monster.Id));
    }

    [TestMethod]
    public void DeleteMonster_ThrowsWhenMonsterIsCore()
    {
        // Arrange
        var monster = CreateMonster("Core Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        monster.IsCore = true;

        _monsterDao.AddMonster(monster);

        // Act
        Assert.ThrowsException<InvalidOperationException>(() => _monsterDao.DeleteMonster(monster));

        _context.ChangeTracker.Clear();

        // Assert
        Assert.IsTrue(_context.Monsters.Any(m => m.Id == monster.Id));
    }

    [TestMethod]
    public void DeleteMonster_PreservesAssociatedSkills()
    {
        // Arrange
        var monster = CreateMonster("Goblin Warrior", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);

        var skill1 = CreateSkill("Power Strike", TestSkillType.Martial);
        var skill2 = CreateSkill("Whirlwind", TestSkillType.Ultimate);

        monster.Skills = [skill1, skill2];

        _monsterDao.AddMonster(monster);

        // Act
        _monsterDao.DeleteMonster(monster);

        _context.ChangeTracker.Clear();

        var persistedSkills = _context.Skills
            .Where(s => s.Id == skill1.Id || s.Id == skill2.Id)
            .ToList();

        // Assert
        Assert.AreEqual(2, persistedSkills.Count);
        Assert.IsTrue(persistedSkills.All(s => s.MonsterId == null));
    }

    [TestMethod]
    public void DeleteMonster_PreservesAssociatedTreasure()
    {
        // Arrange
        var monster = CreateMonster("Treasure Guardian", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);
        var treasure = CreateItem("Guardian's Sword", 5, 20, TestItemType.Weapon);

        monster.SetLoot(treasure);

        _monsterDao.AddMonster(monster);

        // Act
        _monsterDao.DeleteMonster(monster);

        _context.ChangeTracker.Clear();

        var persistedTreasure = _context.Items
            .Single(i => i.Id == treasure.Id);

        // Assert
        Assert.AreEqual(treasure.Name, persistedTreasure.Name);
        Assert.AreEqual(treasure.Id, persistedTreasure.Id);
    }

    [TestMethod]
    public void DeleteMonster_ClearsTreasureMonsterAssociation()
    {
        // Arrange
        var monster = CreateMonster("Treasure Guardian", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);
        var treasure = CreateItem("Guardian's Sword", 5, 20, TestItemType.Weapon);

        monster.SetLoot(treasure);
        _monsterDao.AddMonster(monster);

        // Act
        _monsterDao.DeleteMonster(monster);
        _context.ChangeTracker.Clear();

        var persistedTreasure = _context.Items.Include(i => i.Monster)
            .Single(i => i.Id == treasure.Id);

        // Assert
        Assert.IsNull(persistedTreasure.MonsterId);
        Assert.IsNull(persistedTreasure.Monster);
    }

    [TestMethod]
    public void DeleteMonster_PreservesAssociatedRoom()
    {
        // Arrange
        var room = CreateRoom("Dragon's Lair");
        _context.Rooms.Add(room);
        _context.SaveChanges();

        var monster = CreateMonster("Cave Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);
        monster.Room = room;
        monster.RoomId = room.Id;

        _monsterDao.AddMonster(monster);

        // Act
        _monsterDao.DeleteMonster(monster);

        _context.ChangeTracker.Clear();

        var persistedRoom = _context.Rooms
            .Include(r => r.Monsters)
            .Single(r => r.Id == room.Id);

        // Assert
        Assert.AreEqual(room.Name, persistedRoom.Name);
        Assert.AreEqual(0, persistedRoom.Monsters.Count);
    }

    [TestMethod]
    public void GetAllMonsters_ReturnsAllMonsters()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);
        var monster3 = CreateMonster("Orc", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Elite);

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);
        _monsterDao.AddMonster(monster3);

        // Act
        var monsters = _monsterDao.GetAllMonsters();

        // Assert
        Assert.AreEqual(3, monsters.Count);
        CollectionAssert.AreEquivalent(
            new[] { monster1.Id, monster2.Id, monster3.Id },
            monsters.Select(m => m.Id).ToArray());
    }

    [TestMethod]
    public void GetAllMonsters_IncludesCoreAndNonCoreMonsters()
    {
        // Arrange
        var coreMonster = CreateMonster("Core Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        coreMonster.IsCore = true;

        var nonCoreMonster =
            CreateMonster("Custom Goblin", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);

        _monsterDao.AddMonster(coreMonster);
        _monsterDao.AddMonster(nonCoreMonster);

        // Act
        var monsters = _monsterDao.GetAllMonsters();

        // Assert
        Assert.AreEqual(2, monsters.Count);
        Assert.IsTrue(monsters.Any(m => m.Id == coreMonster.Id && m.IsCore));
        Assert.IsTrue(monsters.Any(m => m.Id == nonCoreMonster.Id && !m.IsCore));
    }

    [TestMethod]
    public void GetAllMonsters_IncludesDifferentMonsterTypes()
    {
        // Arrange
        var commonMonster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var eliteMonster = CreateMonster("Elite Goblin", ThreatLevel.Elite, DamageType.Martial, TestMonsterType.Elite);
        var bossMonster = CreateMonster("Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);

        _monsterDao.AddMonster(commonMonster);
        _monsterDao.AddMonster(eliteMonster);
        _monsterDao.AddMonster(bossMonster);

        // Act
        var monsters = _monsterDao.GetAllMonsters();

        // Assert
        Assert.AreEqual(3, monsters.Count);
        Assert.IsInstanceOfType<Monster>(monsters.Single(m => m.Id == commonMonster.Id));
        Assert.IsInstanceOfType<EliteMonster>(monsters.Single(m => m.Id == eliteMonster.Id));
        Assert.IsInstanceOfType<BossMonster>(monsters.Single(m => m.Id == bossMonster.Id));
    }

    [TestMethod]
    public void GetAllMonsters_ReturnsEmptyWhenNoMonstersExist()
    {
        // Act
        var monsters = _monsterDao.GetAllMonsters();

        // Assert
        Assert.AreEqual(0, monsters.Count);
    }

    [TestMethod]
    public void GetMonstersByName_ReturnsMatchingMonsters()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin Warrior", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Fire Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);
        var monster3 = CreateMonster("Forest Troll", ThreatLevel.High, DamageType.Martial, TestMonsterType.Elite);

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);
        _monsterDao.AddMonster(monster3);

        // Act
        var monsters = _monsterDao.GetMonstersByName("Dragon");

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(monster2.Id, monsters.Single().Id);
    }

    [TestMethod]
    public void GetMonstersByName_IsCaseInsensitive()
    {
        // Arrange
        var monster = CreateMonster("Fire Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);
        _monsterDao.AddMonster(monster);

        // Act
        var monsters = _monsterDao.GetMonstersByName("fIrE dRaGoN");

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(monster.Id, monsters.Single().Id);
    }

    [TestMethod]
    public void GetMonstersByName_ReturnsPartialMatches()
    {
        // Arrange
        var monster = CreateMonster("Goblin Warrior", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);
        _monsterDao.AddMonster(monster);

        // Act
        var monsters = _monsterDao.GetMonstersByName("War");

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(monster.Id, monsters.Single().Id);
    }

    [TestMethod]
    public void GetMonstersByName_ReturnsMultipleMatches()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin Warrior", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Goblin Archer", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var monster3 = CreateMonster("Forest Troll", ThreatLevel.High, DamageType.Martial, TestMonsterType.Elite);

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);
        _monsterDao.AddMonster(monster3);

        // Act
        var monsters = _monsterDao.GetMonstersByName("Goblin");

        // Assert
        Assert.AreEqual(2, monsters.Count);
        CollectionAssert.AreEquivalent(
            new[] { monster1.Id, monster2.Id },
            monsters.Select(m => m.Id).ToArray());
    }

    [TestMethod]
    public void GetMonstersByName_ReturnsEmptyWhenNoMatches()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin Warrior", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Fire Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);

        // Act
        var monsters = _monsterDao.GetMonstersByName("Skeleton");

        // Assert
        Assert.AreEqual(0, monsters.Count);
    }

    [TestMethod]
    public void GetAllNonCoreMonsters_ReturnsOnlyNonCoreMonsters()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);
        var monster3 = CreateMonster("Core Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);

        monster3.IsCore = true;

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);
        _monsterDao.AddMonster(monster3);

        // Act
        var monsters = _monsterDao.GetAllNonCoreMonsters();

        // Assert
        Assert.AreEqual(2, monsters.Count);
        Assert.IsTrue(monsters.All(m => !m.IsCore));

        CollectionAssert.AreEquivalent(
            new[] { monster1.Id, monster2.Id },
            monsters.Select(m => m.Id).ToArray());
    }

    [TestMethod]
    public void GetAllNonCoreMonsters_ExcludesCoreMonsters()
    {
        // Arrange
        var coreMonster1 = CreateMonster("Core Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var coreMonster2 = CreateMonster("Core Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);

        coreMonster1.IsCore = true;
        coreMonster2.IsCore = true;

        var nonCoreMonster =
            CreateMonster("Custom Troll", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);

        _monsterDao.AddMonster(coreMonster1);
        _monsterDao.AddMonster(coreMonster2);
        _monsterDao.AddMonster(nonCoreMonster);

        // Act
        var monsters = _monsterDao.GetAllNonCoreMonsters();

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(nonCoreMonster.Id, monsters.Single().Id);
        Assert.IsFalse(monsters.Single().IsCore);
    }

    [TestMethod]
    public void GetAllNonCoreMonsters_ReturnsEmptyWhenNoNonCoreMonstersExist()
    {
        // Arrange
        var coreMonster1 = CreateMonster("Core Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var coreMonster2 = CreateMonster("Core Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);

        coreMonster1.IsCore = true;
        coreMonster2.IsCore = true;

        _monsterDao.AddMonster(coreMonster1);
        _monsterDao.AddMonster(coreMonster2);

        // Act
        var monsters = _monsterDao.GetAllNonCoreMonsters();

        // Assert
        Assert.AreEqual(0, monsters.Count);
    }

    [TestMethod]
    public void GetMonstersByThreatLevel_ReturnsOnlyMatchingThreatLevel()
    {
        // Arrange
        var lowMonster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Troll", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);
        var highMonster = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);

        _monsterDao.AddMonster(lowMonster);
        _monsterDao.AddMonster(mediumMonster);
        _monsterDao.AddMonster(highMonster);

        // Act
        var monsters = _monsterDao.GetMonstersByThreatLevel(ThreatLevel.Medium);

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(mediumMonster.Id, monsters.Single().Id);
        Assert.AreEqual(ThreatLevel.Medium, monsters.Single().ThreatLevel);
    }

    [TestMethod]
    public void GetMonstersByThreatLevel_ReturnsMultipleMatchingMonsters()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);
        var monster3 = CreateMonster("Troll", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);
        _monsterDao.AddMonster(monster3);

        // Act
        var monsters = _monsterDao.GetMonstersByThreatLevel(ThreatLevel.High);

        // Assert
        Assert.AreEqual(2, monsters.Count);
        Assert.IsTrue(monsters.All(m => m.ThreatLevel == ThreatLevel.High));
        CollectionAssert.AreEquivalent(
            new[] { monster1.Id, monster2.Id },
            monsters.Select(m => m.Id).ToArray());
    }

    [TestMethod]
    public void GetMonstersByThreatLevel_ReturnsEmptyWhenNoMatches()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);

        // Act
        var monsters = _monsterDao.GetMonstersByThreatLevel(ThreatLevel.Boss);

        // Assert
        Assert.AreEqual(0, monsters.Count);
    }

    [TestMethod]
    public void GetMonstersByDamageType_ReturnsOnlyMatchingDamageType()
    {
        // Arrange
        var martialMonster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var magicalMonster = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);
        var hybridMonster = CreateMonster("Chimera", ThreatLevel.Elite, DamageType.Hybrid, TestMonsterType.Common);

        _monsterDao.AddMonster(martialMonster);
        _monsterDao.AddMonster(magicalMonster);
        _monsterDao.AddMonster(hybridMonster);

        // Act
        var monsters = _monsterDao.GetMonstersByDamageType(DamageType.Magical);

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(magicalMonster.Id, monsters.Single().Id);
        Assert.AreEqual(DamageType.Magical, monsters.Single().DamageType);
    }

    [TestMethod]
    public void GetMonstersByDamageType_ReturnsMultipleMatchingMonsters()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);
        var monster3 = CreateMonster("Lich", ThreatLevel.Elite, DamageType.Magical, TestMonsterType.Common);

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);
        _monsterDao.AddMonster(monster3);

        // Act
        var monsters = _monsterDao.GetMonstersByDamageType(DamageType.Magical);

        // Assert
        Assert.AreEqual(2, monsters.Count);
        Assert.IsTrue(monsters.All(m => m.DamageType == DamageType.Magical));
        CollectionAssert.AreEquivalent(
            new[] { monster2.Id, monster3.Id },
            monsters.Select(m => m.Id).ToArray());
    }

    [TestMethod]
    public void GetMonstersByDamageType_ReturnsEmptyWhenNoMatches()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);

        // Act
        var monsters = _monsterDao.GetMonstersByDamageType(DamageType.Hybrid);

        // Assert
        Assert.AreEqual(0, monsters.Count);
    }

    [TestMethod]
    public void GetNonBossMonstersByMaxLevel_ReturnsMonstersAtOrBelowLevel()
    {
        // Arrange
        var lowMonster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Troll", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);
        lowMonster.Level = 2;
        mediumMonster.Level = 5;

        _monsterDao.AddMonster(lowMonster);
        _monsterDao.AddMonster(mediumMonster);

        // Act
        var monsters = _monsterDao.GetNonBossMonstersByMaxLevel(5);

        // Assert
        Assert.AreEqual(2, monsters.Count);
        Assert.IsTrue(monsters.All(m => m.Level <= 5));
        CollectionAssert.AreEquivalent(
            new[] { lowMonster.Id, mediumMonster.Id },
            monsters.Select(m => m.Id).ToArray());
    }

    [TestMethod]
    public void GetNonBossMonstersByMaxLevel_ExcludesMonstersAboveLevel()
    {
        // Arrange
        var eligibleMonster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var highLevelMonster = CreateMonster("Troll", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);
        eligibleMonster.Level = 3;
        highLevelMonster.Level = 8;

        _monsterDao.AddMonster(eligibleMonster);
        _monsterDao.AddMonster(highLevelMonster);

        // Act
        var monsters = _monsterDao.GetNonBossMonstersByMaxLevel(5);

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(eligibleMonster.Id, monsters.Single().Id);
        Assert.IsTrue(monsters.All(m => m.Level <= 5));
    }

    [TestMethod]
    public void GetNonBossMonstersByMaxLevel_ExcludesBossMonsters()
    {
        // Arrange
        var commonMonster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);
        var bossMonster = CreateMonster("Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);
        commonMonster.Level = 5;
        bossMonster.Level = 3;

        _monsterDao.AddMonster(commonMonster);
        _monsterDao.AddMonster(bossMonster);

        // Act
        var monsters = _monsterDao.GetNonBossMonstersByMaxLevel(5);

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(commonMonster.Id, monsters.Single().Id);
        Assert.IsFalse(monsters.Any(m => m.ThreatLevel == ThreatLevel.Boss));
    }

    [TestMethod]
    public void GetNonBossMonstersByMaxLevel_ThrowsWhenLevelIsZero()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _monsterDao.GetNonBossMonstersByMaxLevel(0));
    }

    [TestMethod]
    public void GetNonBossMonstersByMaxLevel_ThrowsWhenLevelIsNegative()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _monsterDao.GetNonBossMonstersByMaxLevel(-1));
    }

    [TestMethod]
    public void GetNonBossMonstersByMaxLevel_ReturnsEmptyWhenNoMatches()
    {
        // Arrange
        var highLevelMonster = CreateMonster("Troll", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);
        var bossMonster = CreateMonster("Dragon", ThreatLevel.Boss, DamageType.Magical, TestMonsterType.Boss);
        highLevelMonster.Level = 8;
        bossMonster.Level = 3;

        _monsterDao.AddMonster(highLevelMonster);
        _monsterDao.AddMonster(bossMonster);

        // Act
        var monsters = _monsterDao.GetNonBossMonstersByMaxLevel(5);

        // Assert
        Assert.AreEqual(0, monsters.Count);
    }

    [TestMethod]
    public void GetMonstersByMaxLevelAndThreatLevel_ReturnsMatchingMonsters()
    {
        // Arrange
        var monster1 = CreateMonster("Goblin", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);
        var monster2 = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);
        var monster3 = CreateMonster("Troll", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);

        monster1.Level = 3;
        monster2.Level = 5;
        monster3.Level = 4;

        _monsterDao.AddMonster(monster1);
        _monsterDao.AddMonster(monster2);
        _monsterDao.AddMonster(monster3);

        // Act
        var monsters = _monsterDao.GetMonstersByMaxLevelAndThreatLevel(5, ThreatLevel.High);

        // Assert
        Assert.AreEqual(2, monsters.Count);
        Assert.IsTrue(monsters.All(m => m is { Level: <= 5, ThreatLevel: ThreatLevel.High }));
        CollectionAssert.AreEquivalent(
            new[] { monster1.Id, monster2.Id },
            monsters.Select(m => m.Id).ToArray());
    }

    [TestMethod]
    public void GetMonstersByMaxLevelAndThreatLevel_ExcludesMonstersAboveLevel()
    {
        // Arrange
        var eligibleMonster = CreateMonster("Goblin", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);
        var highLevelMonster = CreateMonster("Fire Drake", ThreatLevel.High, DamageType.Magical, TestMonsterType.Elite);

        eligibleMonster.Level = 3;
        highLevelMonster.Level = 8;

        _monsterDao.AddMonster(eligibleMonster);
        _monsterDao.AddMonster(highLevelMonster);

        // Act
        var monsters = _monsterDao.GetMonstersByMaxLevelAndThreatLevel(5, ThreatLevel.High);

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(eligibleMonster.Id, monsters.Single().Id);
        Assert.IsTrue(monsters.All(m => m.Level <= 5));
    }

    [TestMethod]
    public void GetMonstersByMaxLevelAndThreatLevel_ExcludesOtherThreatLevels()
    {
        // Arrange
        var matchingMonster = CreateMonster("Goblin", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);
        var differentThreatMonster =
            CreateMonster("Troll", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);

        matchingMonster.Level = 4;
        differentThreatMonster.Level = 3;

        _monsterDao.AddMonster(matchingMonster);
        _monsterDao.AddMonster(differentThreatMonster);

        // Act
        var monsters = _monsterDao.GetMonstersByMaxLevelAndThreatLevel(5, ThreatLevel.High);

        // Assert
        Assert.AreEqual(1, monsters.Count);
        Assert.AreEqual(matchingMonster.Id, monsters.Single().Id);
        Assert.IsTrue(monsters.All(m => m.ThreatLevel == ThreatLevel.High));
    }

    [TestMethod]
    public void GetMonstersByMaxLevelAndThreatLevel_ReturnsEmptyWhenNoMatches()
    {
        // Arrange
        var highLevelMonster = CreateMonster("Goblin", ThreatLevel.High, DamageType.Martial, TestMonsterType.Common);
        var differentThreatMonster =
            CreateMonster("Troll", ThreatLevel.Medium, DamageType.Martial, TestMonsterType.Common);

        highLevelMonster.Level = 8;
        differentThreatMonster.Level = 3;

        _monsterDao.AddMonster(highLevelMonster);
        _monsterDao.AddMonster(differentThreatMonster);

        // Act
        var monsters = _monsterDao.GetMonstersByMaxLevelAndThreatLevel(5, ThreatLevel.High);

        // Assert
        Assert.AreEqual(0, monsters.Count);
    }

    [TestMethod]
    public void GetMonstersByMaxLevelAndThreatLevel_ThrowsWhenLevelIsZero()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            _monsterDao.GetMonstersByMaxLevelAndThreatLevel(0, ThreatLevel.High));
    }

    [TestMethod]
    public void GetMonstersByMaxLevelAndThreatLevel_ThrowsWhenLevelIsNegative()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            _monsterDao.GetMonstersByMaxLevelAndThreatLevel(-1, ThreatLevel.High));
    }
}