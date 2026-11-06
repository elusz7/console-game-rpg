using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;
using ConsoleGameEntities.Models.Runes.Recipes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGameTests.ConsoleGame.GameDao;

[TestClass]
public class MonsterDropPersistenceIntegrationTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private MonsterDropDao _monsterDropDao = null!;

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
        _monsterDropDao = new MonsterDropDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void GetDropsByElementType_ReturnsEmptyList_WhenNoResultsFound()
    {
        var results = _monsterDropDao.GetDropsByElementType(ElementType.Abyssal);
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void GetDropsByElementType_ReturnsMatchingDrops_WithRelatedEntities()
    {
        var ingredient1 = CreateIngredient("Test1", ComponentType.Essence);
        var bossDrop = CreateMonsterDrop(ElementType.Abyssal, ThreatLevel.Boss, ingredient1, 5.0m);
        var ingredient2 = CreateIngredient("Test2", ComponentType.Core);
        var eliteDrop = CreateMonsterDrop(ElementType.Ice, ThreatLevel.Elite, ingredient2, 15.0m);
        var ingredient3 = CreateIngredient("Test3", ComponentType.MonsterPart);
        var lowDrop = CreateMonsterDrop(ElementType.Abyssal, ThreatLevel.Low, ingredient3, 25.0m);
        
        _context.MonsterDrops.AddRange(bossDrop, eliteDrop, lowDrop);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var results = _monsterDropDao.GetDropsByElementType(ElementType.Abyssal);
        Assert.IsNotNull(results);
        Assert.AreEqual(2, results.Count);

        var dbBossDrop = results.Single(r => r.ThreatLevel == ThreatLevel.Boss);
        var dbLowDrop = results.Single(r => r.ThreatLevel == ThreatLevel.Low);
        
        AssertDropEquality(bossDrop, dbBossDrop);
        AssertDropEquality(lowDrop, dbLowDrop);
    }
    
    [TestMethod]
    public void GetDropsByThreatLevel_ReturnsEmptyList_WhenNoResultsFound()
    {
        var results = _monsterDropDao.GetDropsByThreatLevel(ThreatLevel.High);
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void GetDropsByThreatLevel_ReturnsMatchingDrops_WithRelatedEntities()
    {
        var ingredient1 = CreateIngredient("Test1", ComponentType.Essence);
        var abyssalDrop = CreateMonsterDrop(ElementType.Abyssal, ThreatLevel.Boss, ingredient1, 5.0m);
        var ingredient2 = CreateIngredient("Test2", ComponentType.Core);
        var iceDrop = CreateMonsterDrop(ElementType.Ice, ThreatLevel.Elite, ingredient2, 15.0m);
        var ingredient3 = CreateIngredient("Test3", ComponentType.MonsterPart);
        var fireDrop = CreateMonsterDrop(ElementType.Fire, ThreatLevel.Boss, ingredient3, 25.0m);
        
        _context.MonsterDrops.AddRange(abyssalDrop, iceDrop, fireDrop);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var results = _monsterDropDao.GetDropsByThreatLevel(ThreatLevel.Boss);
        Assert.IsNotNull(results);
        Assert.AreEqual(2, results.Count);

        var dbAbyssalDrop = results.Single(r => r.Element == ElementType.Abyssal);
        var dbFireDrop = results.Single(r => r.Element == ElementType.Fire);
        
        AssertDropEquality(abyssalDrop, dbAbyssalDrop);
        AssertDropEquality(fireDrop, dbFireDrop);
    }
    
    [TestMethod]
    public void GetDropsByElementTypeAndThreatLevel_ReturnsEmptyList_WhenNoResultsFound()
    {
        var results = _monsterDropDao.GetDropsByElementTypeAndThreatLevel(ElementType.Nature, ThreatLevel.High);
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count);
    }
    
    [TestMethod]
    public void GetDropsByElementTypeAndThreatLevel_ReturnsMatchingDrops_WithRelatedEntities()
    {
        var ingredient1 = CreateIngredient("Test1", ComponentType.Essence);
        var abyssalBossDrop = CreateMonsterDrop(ElementType.Abyssal, ThreatLevel.Boss, ingredient1, 5.0m);
        var ingredient2 = CreateIngredient("Test2", ComponentType.Core);
        var iceBossDrop = CreateMonsterDrop(ElementType.Ice, ThreatLevel.Boss, ingredient2, 15.0m);
        var ingredient3 = CreateIngredient("Test3", ComponentType.MonsterPart);
        var abyssalMediumDrop = CreateMonsterDrop(ElementType.Abyssal, ThreatLevel.Medium, ingredient3, 25.0m);
        var ingredient4 = CreateIngredient("Test4", ComponentType.Core);
        var iceBossDrop2 = CreateMonsterDrop(ElementType.Ice, ThreatLevel.Boss, ingredient4, 35.0m);
        
        _context.MonsterDrops.AddRange(abyssalBossDrop, iceBossDrop, abyssalMediumDrop, iceBossDrop2);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var results = _monsterDropDao.GetDropsByElementTypeAndThreatLevel(ElementType.Ice, ThreatLevel.Boss);
        Assert.IsNotNull(results);
        Assert.AreEqual(2, results.Count);

        var dbIceBossDrop = results.Single(r => r.DropRate == 15.0m);
        var dbIceBossDrop2 = results.Single(r => r.DropRate == 35.0m);
        
        AssertDropEquality(iceBossDrop, dbIceBossDrop);
        AssertDropEquality(iceBossDrop2, dbIceBossDrop2);
    }

    private static void AssertDropEquality(MonsterDrop originalDrop, MonsterDrop databaseDrop)
    {
        Assert.AreEqual(originalDrop.Element, databaseDrop.Element);
        Assert.AreEqual(originalDrop.ThreatLevel, databaseDrop.ThreatLevel);
        Assert.AreEqual(originalDrop.DropRate, databaseDrop.DropRate);
        Assert.AreEqual(originalDrop.IngredientId, databaseDrop.IngredientId);
        
        var originalIngredient = originalDrop.Ingredient;
        var databaseIngredient = databaseDrop.Ingredient;
        Assert.AreEqual(originalIngredient.Id, databaseIngredient.Id);
        Assert.AreEqual(originalIngredient.Name, databaseIngredient.Name);
        Assert.AreEqual(originalIngredient.ComponentType, databaseIngredient.ComponentType);
    }
}