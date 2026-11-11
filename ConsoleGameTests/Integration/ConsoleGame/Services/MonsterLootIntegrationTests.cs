using ConsoleGame.Factories;
using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Models;
using ConsoleGame.Services;
using ConsoleGame.Services.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Exceptions;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Runes.Recipes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGameTests.Integration.ConsoleGame.Services;

[TestClass]
public class MonsterLootIntegrationTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private IIngredientDao _ingredientDao = null!;
    private IMonsterDropDao _monsterDropDao = null!;

    private IMonsterDao _monsterDao = null!;
    private ISkillDao _skillDao = null!;
    private IMonsterSkillSelector _skillSelector = null!;

    private ITransactionManager _transactionManager = null!;

    private ILootService _lootService = null!;
    private IMonsterFactory _monsterFactory = null!;

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

        //Set Up Monster Factory
        _skillSelector = new MonsterSkillSelector();
        _monsterDao = new MonsterDao(_context);
        _skillDao = new SkillDao(_context);
        _monsterFactory = new MonsterFactory(_monsterDao, _skillDao, _skillSelector);

        //Set Up Loot Service
        _ingredientDao = new IngredientDao(_context);
        _monsterDropDao = new MonsterDropDao(_context);
        _transactionManager = new TransactionManager(_context);
        _lootService = new LootService(_ingredientDao, _monsterDropDao, _transactionManager);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }
    
    private Floor CreateFloor(int level)
    {
        return new Floor
        {
            Level = level,
            Rooms = [],
            Monsters = [],
            Loot = [],
            NumberOfCursedItems = 0,
            ItemFactory = null!
        };
    }
    
    private static GameSession CreateSession(Player player, Floor floor, bool isCampaign)
    {
        return new GameSession(player, floor, isCampaign);
    }

    private void AddMonsterToFloor(GameSession session, Monster monster)
    {
        session.CurrentFloor.Monsters.Add(monster);
    }
    
    [TestMethod]
    public void MonsterFactoryAndLootService_GeneratedMonsterDropsIngredient()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        
        var ingredient = CreateIngredient("Test Ingredient", ComponentType.Core);
        var drop = CreateMonsterDrop(campaignMonster.AttackElement!.Value, campaignMonster.ThreatLevel, ingredient, 100.0M);
        
        _context.MonsterDrops.Add(drop);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(1, playerIngredients.Count);
        
        var playerIngredient = playerIngredients[0];
        Assert.AreEqual(ingredient.Id, playerIngredient.IngredientId);
        Assert.AreEqual(1, playerIngredient.Quantity);
    }
    
    [TestMethod]
    public void MonsterFactoryAndLootService_UsesGeneratedElementAndThreatLevelForDrops()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 5 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(5);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(5, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a High threat monster (has both AttackElement and Vulnerability)
        var campaignMonster = campaignMonsters.First(m => m.ThreatLevel == ThreatLevel.High);
        AddMonsterToFloor(session, campaignMonster);
        
        var ingredient1 = CreateIngredient("Test Ingredient 1", ComponentType.Core);
        var ingredient2 = CreateIngredient("Test Ingredient 2", ComponentType.Essence);
        var ingredient3 = CreateIngredient("Test Ingredient 3", ComponentType.MonsterPart);
        
        var drop1 = CreateMonsterDrop(campaignMonster.AttackElement!.Value,  campaignMonster.ThreatLevel, ingredient1, 100.0M); //match both
        var drop2 = CreateMonsterDrop(campaignMonster.Vulnerability!.Value, campaignMonster.ThreatLevel, ingredient2, 100.0M); //match ThreatLevel
        var drop3 = CreateMonsterDrop(campaignMonster.AttackElement!.Value, ThreatLevel.Boss, ingredient3, 100.0M); //match Element

        _context.MonsterDrops.AddRange(drop1, drop2, drop3);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(1, playerIngredients.Count);
        
        var playerIngredient = playerIngredients[0];
        Assert.AreEqual(ingredient1.Id, playerIngredient.IngredientId);
        Assert.AreEqual(1, playerIngredient.Quantity);
    }
    
    [TestMethod]
    public void MonsterFactoryAndLootService_DoesNotDropIngredientsForDifferentThreatLevel()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        
        var ingredient = CreateIngredient("Test Ingredient 1", ComponentType.Core);
        
        var drop1 = CreateMonsterDrop(campaignMonster.AttackElement!.Value,  ThreatLevel.High, ingredient, 100.0M);

        _context.MonsterDrops.Add(drop1);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(0, playerIngredients.Count);
    }
    
    [TestMethod]
    public void MonsterFactoryAndLootService_DoesNotDropIngredientsForDifferentElement()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 5 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(5);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(5, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a High threat monster (has Vulnerability)
        var campaignMonster = campaignMonsters.First(m => m.ThreatLevel == ThreatLevel.High);
        AddMonsterToFloor(session, campaignMonster);
        
        var ingredient = CreateIngredient("Test Ingredient 1", ComponentType.Core);
        
        var drop1 = CreateMonsterDrop(campaignMonster.Vulnerability!.Value,  campaignMonster.ThreatLevel, ingredient, 100.0M);

        _context.MonsterDrops.Add(drop1);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(0, playerIngredients.Count);
    }
    
    [TestMethod]
    public void LootService_PersistsMultipleIngredientDrops()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        
        var ingredient1 = CreateIngredient("Test Ingredient 1", ComponentType.Core);
        var ingredient2 = CreateIngredient("Test Ingredient 2", ComponentType.Essence);
        var ingredient3 = CreateIngredient("Test Ingredient 3", ComponentType.MonsterPart);
        
        var drop1 = CreateMonsterDrop(campaignMonster.AttackElement!.Value,  campaignMonster.ThreatLevel, ingredient1, 100.0M); 
        var drop2 = CreateMonsterDrop(campaignMonster.AttackElement!.Value, campaignMonster.ThreatLevel, ingredient2, 100.0M); 
        var drop3 = CreateMonsterDrop(campaignMonster.AttackElement!.Value, campaignMonster.ThreatLevel, ingredient3, 100.0M); 

        _context.MonsterDrops.AddRange(drop1, drop2, drop3);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(3, playerIngredients.Count);
        
        var ingredientIds = playerIngredients
            .Select(pi => pi.IngredientId)
            .ToHashSet();

        CollectionAssert.AreEquivalent(
            new[] { ingredient1.Id, ingredient2.Id, ingredient3.Id },
            ingredientIds.ToArray());

        Assert.AreEqual(1, playerIngredients[0].Quantity);
        Assert.AreEqual(1, playerIngredients[1].Quantity);
        Assert.AreEqual(1, playerIngredients[2].Quantity);
    }
    
    [TestMethod]
    public void LootService_IncrementsExistingIngredientQuantity()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        
        var ingredient1 = CreateIngredient("Test Ingredient 1", ComponentType.Core);
        
        var drop1 = CreateMonsterDrop(campaignMonster.AttackElement!.Value,  campaignMonster.ThreatLevel, ingredient1, 100.0M); 

        _context.MonsterDrops.Add(drop1);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        _ingredientDao.AddIngredientToPlayer(ingredient1.Id, player.Id);
        _context.ChangeTracker.Clear();
        
        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(1, playerIngredients.Count);
        
        var playerIngredient = playerIngredients[0];
        Assert.AreEqual(ingredient1.Id, playerIngredient.IngredientId);
        Assert.AreEqual(2, playerIngredient.Quantity);
    }
    
    [TestMethod]
    public void LootService_DoesNotDuplicateIngredientsWhenLootingTwice()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        
        var ingredient1 = CreateIngredient("Test Ingredient 1", ComponentType.Core);
        var drop1 = CreateMonsterDrop(campaignMonster.AttackElement!.Value,  campaignMonster.ThreatLevel, ingredient1, 100.0M); 

        _context.MonsterDrops.Add(drop1);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(1, playerIngredients.Count);
        
        var playerIngredient = playerIngredients[0];
        Assert.AreEqual(ingredient1.Id, playerIngredient.IngredientId);
        Assert.AreEqual(1, playerIngredient.Quantity);
    }
    
    [TestMethod]
    public void LootService_DoesNotPersistIngredientsWhenNoMatchingDropsExist()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(0, playerIngredients.Count);
    }
    
    [TestMethod]
    public void LootService_PersistsTreasureAndIngredientsTogether()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        
        var ingredient1 = CreateIngredient("Test Ingredient 1", ComponentType.Core);
        var drop1 = CreateMonsterDrop(campaignMonster.AttackElement!.Value,  campaignMonster.ThreatLevel, ingredient1, 100.0M); 

        _context.MonsterDrops.Add(drop1);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var treasure = CreateItem("Test Treasure", 1, 3, TestItemType.Armor);
        treasure.Weight = 1M; //make sure the weight is under capacity
        campaignMonster.Treasure = treasure; //assign treasure to monster
        //persist item here if needed -- not currently needed because loot transfer only happens in game run memory 

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        Assert.IsTrue(campaignMonster.TreasureLooted);
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(1, playerIngredients.Count);
        
        var playerIngredient = playerIngredients[0];
        Assert.AreEqual(ingredient1.Id, playerIngredient.IngredientId);
        Assert.AreEqual(1, playerIngredient.Quantity);

        var items = player.Inventory.Items;
        Assert.AreEqual(1, items.Count);

        var loot = items[0];
        Assert.AreEqual(treasure.Name, loot.Name);
    }
    
    [TestMethod]
    public void LootService_RemovesMonsterAfterAllLootIsCollected()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        var room = CreateRoom("Test Room");
        player.CurrentRoom = room;
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        player.CurrentRoom.Monsters.Add(campaignMonster);
        
        var ingredient1 = CreateIngredient("Test Ingredient 1", ComponentType.Core);
        var drop1 = CreateMonsterDrop(campaignMonster.AttackElement!.Value,  campaignMonster.ThreatLevel, ingredient1, 100.0M); 

        _context.MonsterDrops.Add(drop1);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var treasure = CreateItem("Test Treasure", 1, 3, TestItemType.Armor);
        treasure.Weight = 1M; //make sure the weight is under capacity
        campaignMonster.Treasure = treasure; //assign treasure to monster
        //persist item here if needed -- not currently needed because loot transfer only happens in game run memory 

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        Assert.IsTrue(campaignMonster.TreasureLooted);
        
        Assert.IsFalse(session.CurrentFloor.Monsters.Contains(campaignMonster));
        Assert.IsFalse(player.CurrentRoom.Monsters.Contains(campaignMonster));
    }
    
    [TestMethod]
    public void LootService_KeepsMonsterAvailableWhenTreasureLootingFails()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        var room = CreateRoom("Test Room");
        player.CurrentRoom = room;
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        player.CurrentRoom.Monsters.Add(campaignMonster);
        
        var treasure = CreateItem("Test Treasure", 1, 3, TestItemType.Armor);
        treasure.Weight = 100M; //make sure the weight is over capacity
        campaignMonster.Treasure = treasure; //assign treasure to monster
        //persist item here if needed -- not currently needed because loot transfer only happens in game run memory 

        // Act
        campaignMonster.CurrentHealth = 0;
        Assert.ThrowsException<TreasureException>(() => _lootService.LootMonster(session, campaignMonster));

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        Assert.IsFalse(campaignMonster.TreasureLooted);
        
        Assert.IsTrue(session.CurrentFloor.Monsters.Contains(campaignMonster));
        Assert.IsTrue(player.CurrentRoom.Monsters.Contains(campaignMonster));
    }
    
    [TestMethod]
    public void LootService_AllowsIngredientLootWhenTreasureWasAlreadyLooted()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        var room = CreateRoom("Test Room");
        player.CurrentRoom = room;
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        player.CurrentRoom.Monsters.Add(campaignMonster);

        campaignMonster.TreasureLooted = true;
        
        var ingredient1 = CreateIngredient("Test Ingredient 1", ComponentType.Core);
        var drop1 = CreateMonsterDrop(campaignMonster.AttackElement!.Value,  campaignMonster.ThreatLevel, ingredient1, 100.0M); 

        _context.MonsterDrops.Add(drop1);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        Assert.IsTrue(campaignMonster.TreasureLooted);
        
        Assert.IsFalse(session.CurrentFloor.Monsters.Contains(campaignMonster));
        Assert.IsFalse(player.CurrentRoom.Monsters.Contains(campaignMonster));
        
        var playerIngredients = _ingredientDao.GetIngredientsByPlayerId(player.Id);
        Assert.AreEqual(1, playerIngredients.Count);
        
        var playerIngredient = playerIngredients[0];
        Assert.AreEqual(ingredient1.Id, playerIngredient.IngredientId);
        Assert.AreEqual(1, playerIngredient.Quantity);
    }
    
    [TestMethod]
    public void LootService_AllowsTreasureLootWhenIngredientsWereAlreadyLooted()
    {
        // Arrange: Seed Low, Medium, and High monsters so campaign mode at level 3 generates all three
        var lowMonster = CreateMonster("Test Low Monster", ThreatLevel.Low, DamageType.Magical, TestMonsterType.Common);
        var mediumMonster = CreateMonster("Test Medium Monster", ThreatLevel.Medium, DamageType.Magical, TestMonsterType.Common);
        var highMonster = CreateMonster("Test High Monster", ThreatLevel.High, DamageType.Magical, TestMonsterType.Common);
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        var room = CreateRoom("Test Room");
        player.CurrentRoom = room;
        
        _context.Monsters.AddRange(lowMonster, mediumMonster, highMonster);
        _context.Players.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var floor = CreateFloor(3);
        var session = CreateSession(player, floor, true);
        
        var campaignMonsters = _monsterFactory.GenerateMonsters(3, true);
        Assert.IsTrue(campaignMonsters.Any());

        // Pick a monster with AttackElement (Medium+ threat)
        var campaignMonster = campaignMonsters.First(m => m.AttackElement.HasValue);
        AddMonsterToFloor(session, campaignMonster);
        player.CurrentRoom.Monsters.Add(campaignMonster);
        
        var treasure = CreateItem("Test Treasure", 1, 3, TestItemType.Armor);
        treasure.Weight = 1M; //make sure the weight is under capacity
        campaignMonster.Treasure = treasure; //assign treasure to monster
        //persist item here if needed -- not currently needed because loot transfer only happens in game run memory 

        campaignMonster.IngredientsLooted = true;

        // Act
        campaignMonster.CurrentHealth = 0;
        _lootService.LootMonster(session, campaignMonster);

        // Assert
        _context.ChangeTracker.Clear();
        
        Assert.IsTrue(campaignMonster.IngredientsLooted);
        Assert.IsTrue(campaignMonster.TreasureLooted);
        
        
        var items = player.Inventory.Items;
        Assert.AreEqual(1, items.Count);

        var loot = items[0];
        Assert.AreEqual(treasure.Name, loot.Name);
        
        Assert.IsFalse(session.CurrentFloor.Monsters.Contains(campaignMonster));
        Assert.IsFalse(player.CurrentRoom.Monsters.Contains(campaignMonster));
    }
}