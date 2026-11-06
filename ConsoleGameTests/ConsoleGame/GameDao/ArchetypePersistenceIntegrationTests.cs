using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGameTests.ConsoleGame.GameDao;

[TestClass]
public class ArchetypePersistenceIntegrationTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private ArchetypeDao _archetypeDao = null!;

    private static readonly string[] AddSkillNames = ["Power Strike", "Shield Bash"];
    private static readonly string[] UpdateSkillNames = ["Power Strike", "Whirlwind"];
    private static readonly string[] MartialArchetypeNames = ["Warrior", "Knight"];
    private static readonly string[] MagicalArchetypeNames = ["Mage", "Sorcerer"];

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
        _archetypeDao = new ArchetypeDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void AddArchetype_PersistsArchetype()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);

        // Act
        _archetypeDao.AddArchetype(archetype);
        _context.ChangeTracker.Clear();

        var result = _context.Archetypes.SingleOrDefault(a => a.Name == "Warrior");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Id > 0);
        Assert.AreEqual("Warrior", result.Name);
    }

    [TestMethod]
    public void AddArchetype_PersistsAllArchetypeProperties()
    {
        // Arrange
        var archetype = CreateArchetype("Test Warrior", ArchetypeType.Martial);
        
        // Act
        _archetypeDao.AddArchetype(archetype);

        _context.ChangeTracker.Clear();

        var result = _context.Archetypes
            .Single(a => a.Id == archetype.Id);

        // Assert
        AssertArchetypePropertiesAreEqual(archetype, result);
    }
    

    [TestMethod]
    public void AddArchetype_PersistsArchetypeWithSkills()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);

        var skill1 = CreateSkill("Power Strike", TestSkillType.Martial);
        var skill2 = CreateSkill("Shield Bash", TestSkillType.Martial);

        _context.Skills.AddRange(skill1, skill2);
        _context.SaveChanges();

        archetype.Skills = [skill1, skill2];

        // Act
        _archetypeDao.AddArchetype(archetype);
        var archetypeId = archetype.Id;

        _context.ChangeTracker.Clear();

        var result = _context.Archetypes
            .Include(a => a.Skills)
            .Single(a => a.Id == archetypeId);

        // Assert
        Assert.AreEqual(2, result.Skills.Count);
        CollectionAssert.AreEquivalent(
            AddSkillNames,
            result.Skills.Select(s => s.Name).ToArray());
    }

    [TestMethod]
    public void UpdateArchetype_PersistsUpdatedProperties()
    {
        // Arrange
        var archetype = CreateArchetype("Test Warrior", ArchetypeType.Martial);
        _archetypeDao.AddArchetype(archetype);

        archetype.Name = "Updated Warrior";
        archetype.Description = "An updated warrior archetype.";
        archetype.IsCore = true;
        archetype.HealthBase = 200;
        archetype.AttackBonus = 35;
        archetype.MagicBonus = 15;
        archetype.DefenseBonus = 40;
        archetype.ResistanceBonus = 25;
        archetype.Speed = 30;
        archetype.ArchetypeType = ArchetypeType.Magical;
        archetype.ResourceName = "Mana";
        archetype.MaxResource = 120;
        archetype.RecoveryRate = 10;
        archetype.RecoveryGrowth = 4;
        archetype.AttackMultiplier = 1.75M;
        archetype.MagicMultiplier = 1.5M;
        archetype.DefenseMultiplier = 1.4M;
        archetype.ResistanceMultiplier = 1.3M;
        archetype.SpeedMultiplier = 1.2M;
        archetype.ResourceMultiplier = 1.6M;

        // Act
        _archetypeDao.UpdateArchetype(archetype);
        _context.ChangeTracker.Clear();

        var result = _context.Archetypes
            .Single(a => a.Id == archetype.Id);

        // Assert
        AssertArchetypePropertiesAreEqual(archetype, result);
    }

    [TestMethod]
    public void UpdateArchetype_PersistsUpdatedIsCore()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        _archetypeDao.AddArchetype(archetype);

        Assert.IsFalse(archetype.IsCore);

        archetype.IsCore = true;

        // Act
        _archetypeDao.UpdateArchetype(archetype);
        var archetypeId = archetype.Id;

        _context.ChangeTracker.Clear();

        var result = _context.Archetypes.Single(a => a.Id == archetypeId);

        // Assert
        Assert.IsTrue(result.IsCore);
    }

    [TestMethod]
    public void UpdateArchetype_PersistsUpdatedSkills()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);

        var skill1 = CreateSkill("Power Strike", TestSkillType.Martial);
        var skill2 = CreateSkill("Shield Bash", TestSkillType.Martial);
        var skill3 = CreateSkill("Whirlwind", TestSkillType.Martial);

        _context.Skills.AddRange(skill1, skill2, skill3);
        _context.SaveChanges();

        archetype.Skills = [skill1, skill2];
        _archetypeDao.AddArchetype(archetype);

        var archetypeId = archetype.Id;

        // Act
        archetype.Skills.Remove(skill2);
        archetype.Skills.Add(skill3);

        _archetypeDao.UpdateArchetype(archetype);

        _context.ChangeTracker.Clear();

        var result = _context.Archetypes
            .Include(a => a.Skills)
            .Single(a => a.Id == archetypeId);

        // Assert
        CollectionAssert.AreEquivalent(
            UpdateSkillNames,
            result.Skills.Select(s => s.Name).ToArray());

        Assert.IsTrue(_context.Skills.Any(s => s.Name == "Shield Bash"));
        Assert.IsTrue(_context.Skills.Any(s => s.Name == "Whirlwind"));
    }

    [TestMethod]
    public void DeleteArchetype_RemovesNonCoreArchetype()
    {
        // Arrange
        var archetype = CreateArchetype("Custom Warrior", ArchetypeType.Martial);
        var skill = CreateSkill("Power Strike", TestSkillType.Martial);

        _context.Skills.Add(skill);
        _context.SaveChanges();

        archetype.Skills.Add(skill);
        _archetypeDao.AddArchetype(archetype);

        var archetypeId = archetype.Id;
        var skillId = skill.Id;

        // Act
        _archetypeDao.DeleteArchetype(archetype);

        _context.ChangeTracker.Clear();

        // Assert
        Assert.IsFalse(_context.Archetypes.Any(a => a.Id == archetypeId));

        // The skill definition must survive archetype deletion.
        Assert.IsTrue(_context.Skills.Any(s => s.Id == skillId));
    }

    [TestMethod]
    public void DeleteArchetype_ThrowsWhenArchetypeIsCore()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        archetype.IsCore = true;

        _archetypeDao.AddArchetype(archetype);

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => _archetypeDao.DeleteArchetype(archetype));
    }

    [TestMethod]
    public void DeleteArchetype_DoesNotRemoveCoreArchetype()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        archetype.IsCore = true;

        _archetypeDao.AddArchetype(archetype);
        var archetypeId = archetype.Id;

        // Act
        Assert.ThrowsException<InvalidOperationException>(() => _archetypeDao.DeleteArchetype(archetype));

        _context.ChangeTracker.Clear();

        var result = _context.Archetypes.SingleOrDefault(a => a.Id == archetypeId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsCore);
    }

    [TestMethod]
    public void GetArchetypesByType_ReturnsOnlyMartialArchetypes()
    {
        // Arrange
        var martial1 = CreateArchetype("Warrior", ArchetypeType.Martial);
        var martial2 = CreateArchetype("Knight", ArchetypeType.Martial);
        var magical = CreateArchetype("Mage", ArchetypeType.Magical);

        _context.Archetypes.AddRange(martial1, martial2, magical);
        _context.SaveChanges();

        // Act
        var result = _archetypeDao.GetArchetypesByType(ArchetypeType.Martial);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(a => a.ArchetypeType == ArchetypeType.Martial));
        CollectionAssert.AreEquivalent(
            MartialArchetypeNames,
            result.Select(a => a.Name).ToArray());
    }

    [TestMethod]
    public void GetArchetypesByType_ReturnsOnlyMagicalArchetypes()
    {
        // Arrange
        var magical1 = CreateArchetype("Mage", ArchetypeType.Magical);
        var magical2 = CreateArchetype("Sorcerer", ArchetypeType.Magical);
        var martial = CreateArchetype("Warrior", ArchetypeType.Martial);

        _context.Archetypes.AddRange(magical1, magical2, martial);
        _context.SaveChanges();

        // Act
        var result = _archetypeDao.GetArchetypesByType(ArchetypeType.Magical);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(a => a.ArchetypeType == ArchetypeType.Magical));
        CollectionAssert.AreEquivalent(
            MagicalArchetypeNames,
            result.Select(a => a.Name).ToArray());
    }

    [TestMethod]
    public void GetArchetypesByType_ReturnsEmptyWhenNoMatches()
    {
        // Arrange
        var martial = CreateArchetype("Warrior", ArchetypeType.Martial);
        var magical = CreateArchetype("Mage", ArchetypeType.Magical);

        _context.Archetypes.AddRange(martial, magical);
        _context.SaveChanges();

        // Act
        var result = _archetypeDao.GetArchetypesByType((ArchetypeType)99);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetArchetypesByType_IncludesArchetypeProperties()
    {
        // Arrange
        var archetype = CreateArchetype("Mage", ArchetypeType.Magical);

        archetype.Description = "A master of elemental magic.";
        archetype.IsCore = true;
        archetype.HealthBase = 15;
        archetype.MagicBonus = 8;
        archetype.DefenseBonus = 4;
        archetype.ResistanceBonus = 7;
        archetype.Speed = 6;
        archetype.ResourceName = "Mana";
        archetype.MaxResource = 30;
        archetype.RecoveryRate = 8;
        archetype.RecoveryGrowth = 2;
        archetype.MagicMultiplier = 1.5M;
        archetype.ResourceMultiplier = 2.0M;

        _context.Archetypes.Add(archetype);
        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        // Act
        var result = _archetypeDao.GetArchetypesByType(ArchetypeType.Magical);

        // Assert
        Assert.AreEqual(1, result.Count);

        var retrieved = result.Single();

        Assert.AreEqual("Mage", retrieved.Name);
        Assert.AreEqual("A master of elemental magic.", retrieved.Description);
        Assert.IsTrue(retrieved.IsCore);
        Assert.AreEqual(15, retrieved.HealthBase);
        Assert.AreEqual(8, retrieved.MagicBonus);
        Assert.AreEqual(4, retrieved.DefenseBonus);
        Assert.AreEqual(7, retrieved.ResistanceBonus);
        Assert.AreEqual(6, retrieved.Speed);
        Assert.AreEqual("Mana", retrieved.ResourceName);
        Assert.AreEqual(30, retrieved.MaxResource);
        Assert.AreEqual(8, retrieved.RecoveryRate);
        Assert.AreEqual(2, retrieved.RecoveryGrowth);
        Assert.AreEqual(1.5M, retrieved.MagicMultiplier);
        Assert.AreEqual(2.0M, retrieved.ResourceMultiplier);
    }
    
    private static void AssertArchetypePropertiesAreEqual(Archetype expected, Archetype actual)
    {
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.Description, actual.Description);
        Assert.AreEqual(expected.IsCore, actual.IsCore);
        Assert.AreEqual(expected.HealthBase, actual.HealthBase);
        Assert.AreEqual(expected.AttackBonus, actual.AttackBonus);
        Assert.AreEqual(expected.MagicBonus, actual.MagicBonus);
        Assert.AreEqual(expected.DefenseBonus, actual.DefenseBonus);
        Assert.AreEqual(expected.ResistanceBonus, actual.ResistanceBonus);
        Assert.AreEqual(expected.Speed, actual.Speed);
        Assert.AreEqual(expected.ArchetypeType, actual.ArchetypeType);
        Assert.AreEqual(expected.ResourceName, actual.ResourceName);
        Assert.AreEqual(expected.MaxResource, actual.MaxResource);
        Assert.AreEqual(expected.RecoveryRate, actual.RecoveryRate);
        Assert.AreEqual(expected.RecoveryGrowth, actual.RecoveryGrowth);
        Assert.AreEqual(expected.AttackMultiplier, actual.AttackMultiplier);
        Assert.AreEqual(expected.MagicMultiplier, actual.MagicMultiplier);
        Assert.AreEqual(expected.DefenseMultiplier, actual.DefenseMultiplier);
        Assert.AreEqual(expected.ResistanceMultiplier, actual.ResistanceMultiplier);
        Assert.AreEqual(expected.SpeedMultiplier, actual.SpeedMultiplier);
        Assert.AreEqual(expected.ResourceMultiplier, actual.ResourceMultiplier);
    }
}