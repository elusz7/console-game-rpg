using ConsoleGame.Models;
using ConsoleGame.Services;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Runes.Recipes;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGame.Services;

[TestClass]
public class LootServiceTests
{
    // The service being tested.
    private readonly Mock<IMonsterDropDao> _monsterDropDaoMock = new();
    private readonly Mock<IIngredientDao> _ingredientDaoMock = new();
    private readonly Mock<ITransactionManager> _transactionManager = new();

    private LootService _service;

    [TestInitialize]
    public void SetUp()
    {
        _monsterDropDaoMock.Reset();
        _ingredientDaoMock.Reset();
        _transactionManager.Reset();

        _transactionManager
            .Setup(t => t.ExecuteInTransaction(It.IsAny<Action>()))
            .Callback<Action>(action => action());

        _service = new LootService(
            _ingredientDaoMock.Object,
            _monsterDropDaoMock.Object,
            _transactionManager.Object);
    }

    // Creates a player with an initialized inventory.
    private static Player CreatePlayer()
    {
        var player = new Player
        {
            Name = "Test Player",
            Archetype = new Archetype
            {
                Name = "Warrior",
                HealthBase = 10,
                ArchetypeType = ModelEnums.ArchetypeType.Martial
            }
        };

        player.Initialize();
        return player;
    }

    // Creates a monster carrying a weapon as treasure.
    private static Monster CreateMonster()
    {
        return new Monster
        {
            Name = "Test Goblin",
            CurrentHealth = 0,
            Treasure = new Weapon
            {
                Name = "Goblin Sword",
                Weight = 5,
                Value = 10
            }
        };
    }
    
    private static Monster CreateElementalMonster()
    {
        var monster = CreateMonster();

        monster.AttackElement = ElementType.Fire;
        monster.ThreatLevel = ThreatLevel.Low;
        monster.Level = 5;

        return monster;
    }

    private static MonsterDrop CreateDrop(
        Ingredient ingredient,
        decimal dropRate = 5m)
    {
        return new MonsterDrop
        {
            Element = ElementType.Fire,
            ThreatLevel = ThreatLevel.Low,
            Ingredient = ingredient,
            IngredientId = ingredient.Id,
            DropRate = dropRate
        };
    }

    // Creates a session with the monster in both monster collections.
    private static GameSession CreateSession(
        Player player,
        Monster monster)
    {
        var room = new Room("Entrance", "The entrance room.");
        room.Monsters.Add(monster);

        var floor = new Floor
        {
            Rooms = [room],
            Monsters = [monster],
            Level = 1
        };

        player.CurrentRoom = room;

        return new GameSession(player, floor, false);
    }

    [TestMethod]
    public void LootMonster_WithTreasure_AddsItemAndRemovesMonster()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateMonster();
        var session = CreateSession(player, monster);
        // Capture the treasure and assert it exists before testing collection.
        var treasure = monster.Treasure
                       ?? throw new InvalidOperationException("Test monster should have treasure.");

        // Act
        _service.LootMonster(session, monster);

        // Assert
        Assert.IsTrue(player.Inventory.Items.Contains(treasure));
        Assert.IsNull(monster.Treasure);
        Assert.AreEqual(0, player.CurrentRoom!.Monsters.Count);
        Assert.AreEqual(0, session.CurrentFloor.Monsters.Count);
    }

    [TestMethod]
    public void LootMonster_WithoutTreasure_ThrowsExceptionAndRemovesMonster()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = new Monster
        {
            Name = "Empty Goblin",
            CurrentHealth = 0,
            Treasure = null
        };

        var session = CreateSession(player, monster);

        // Act & Assert
        _service.LootMonster(session, monster);

        // The monster is removed even though no treasure was collected.
        Assert.IsTrue(monster.TreasureLooted);
        Assert.IsTrue(monster.IngredientsLooted);
        Assert.AreEqual(0, player.CurrentRoom!.Monsters.Count);
        Assert.AreEqual(0, session.CurrentFloor.Monsters.Count);
    }

    [TestMethod]
    public void LootMonster_WhenInventoryIsFull_RestoresTreasureAndRemovesMonster()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateMonster();
        var session = CreateSession(player, monster);
        // Capture the treasure and assert it exists before testing collection.
        var treasure = monster.Treasure
                       ?? throw new InvalidOperationException("Test monster should have treasure.");

        // Set capacity below the treasure's weight to force inventory insertion to fail.
        player.Inventory.Capacity = 1;

        // Act & Assert
        Assert.ThrowsException<TreasureException>(() => _service.LootMonster(session, monster));

        // Treasure is restored so it can be collected later.
        Assert.AreSame(treasure, monster.Treasure);

        // Ingredient processing is complete, but treasure processing isn't.
        Assert.IsTrue(monster.IngredientsLooted);
        Assert.IsFalse(monster.TreasureLooted);

        // Monster remains available for another attempt.
        Assert.AreEqual(1, player.CurrentRoom!.Monsters.Count);
        Assert.AreEqual(1, session.CurrentFloor.Monsters.Count);
    }
    
    [TestMethod]
    public void LootMonster_WhenIngredientDropSucceeds_AwardsIngredient()
    {
        var player = CreatePlayer();
        var monster = CreateElementalMonster();
        var session = CreateSession(player, monster);

        var ingredient = new Ingredient
        {
            Id = 1,
            Name = "Fire Essence",
            ComponentType = ComponentType.Essence
        };

        _monsterDropDaoMock
            .Setup(d => d.GetDropsByElementTypeAndThreatLevel(
                ElementType.Fire, ThreatLevel.Low))
            .Returns([CreateDrop(ingredient, 90m)]);

        _service.LootMonster(session, monster);

        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(ingredient.Id, player.Id, 1),
            Times.Once);
    }
    
    [TestMethod]
    public void LootMonster_WhenIngredientDropFails_DoesNotAwardIngredient()
    {
        var player = CreatePlayer();
        var monster = CreateElementalMonster();
        monster.Level = 0;
        var session = CreateSession(player, monster);

        var ingredient = new Ingredient
        {
            Id = 2,
            Name = "Fire Essence",
            ComponentType = ComponentType.Essence
        };

        _monsterDropDaoMock
            .Setup(d => d.GetDropsByElementTypeAndThreatLevel(
                ElementType.Fire, ThreatLevel.Low))
            .Returns([CreateDrop(ingredient, 0m)]);

        _service.LootMonster(session, monster);

        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(
                    ingredient.Id, player.Id, It.IsAny<int>()),
            Times.Never);
    }
    
    [TestMethod]
    public void LootMonster_WithoutTreasure_StillAwardsIngredients()
    {
        var player = CreatePlayer();
        var monster = CreateElementalMonster();
        monster.Treasure = null;

        var session = CreateSession(player, monster);

        var ingredient = new Ingredient
        {
            Id = 3,
            Name = "Fire Essence",
            ComponentType = ComponentType.Essence
        };

        _monsterDropDaoMock
            .Setup(d => d.GetDropsByElementTypeAndThreatLevel(
                ElementType.Fire, ThreatLevel.Low))
            .Returns([CreateDrop(ingredient, 100m)]);
        
        // Act
        _service.LootMonster(session, monster);

        // Assert
        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(ingredient.Id, player.Id, 1),
            Times.Once);
        
        Assert.IsTrue(monster.TreasureLooted);
        Assert.IsTrue(monster.IngredientsLooted);
        Assert.AreEqual(0, player.CurrentRoom!.Monsters.Count);
        Assert.AreEqual(0, session.CurrentFloor.Monsters.Count);
    }
    
    [TestMethod]
    public void LootMonster_WhenCalledTwice_DoesNotAwardIngredientsTwice()
    {
        var player = CreatePlayer();
        var monster = CreateElementalMonster();
        var session = CreateSession(player, monster);

        var ingredient = new Ingredient
        {
            Id = 4,
            Name = "Fire Essence",
            ComponentType = ComponentType.Essence
        };

        _monsterDropDaoMock
            .Setup(d => d.GetDropsByElementTypeAndThreatLevel(
                ElementType.Fire, ThreatLevel.Low))
            .Returns([CreateDrop(ingredient, 100m)]);

        _service.LootMonster(session, monster);
        _service.LootMonster(session, monster);

        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(ingredient.Id, player.Id, 1),
            Times.Once);

        Assert.IsTrue(monster.IngredientsLooted);
        Assert.IsTrue(monster.TreasureLooted);
    }
    
    [TestMethod]
    public void LootMonster_WhenMonsterHasNoAttackElement_SkipsDropLookup()
    {
        var player = CreatePlayer();
        var monster = CreateMonster();
        var session = CreateSession(player, monster);

        monster.AttackElement = null;

        _service.LootMonster(session, monster);

        _monsterDropDaoMock.Verify(d =>
                d.GetDropsByElementTypeAndThreatLevel(
                    It.IsAny<ElementType>(),
                    It.IsAny<ThreatLevel>()),
            Times.Never);

        Assert.IsTrue(monster.IngredientsLooted);
        Assert.IsTrue(monster.TreasureLooted);
    }
    
    [TestMethod]
    public void LootMonster_WhenNoMatchingDropsExist_MarksIngredientsLooted()
    {
        var player = CreatePlayer();
        var monster = CreateElementalMonster();
        var session = CreateSession(player, monster);

        _monsterDropDaoMock
            .Setup(d => d.GetDropsByElementTypeAndThreatLevel(
                ElementType.Fire, ThreatLevel.Low))
            .Returns([]);

        _service.LootMonster(session, monster);

        Assert.IsTrue(monster.IngredientsLooted);
        Assert.IsTrue(monster.TreasureLooted);

        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }
    
    [TestMethod]
    public void LootMonster_WhenMonsterIsAlive_DoesNotLoot()
    {
        var player = CreatePlayer();
        var monster = CreateMonster();
        monster.CurrentHealth = 10;

        var session = CreateSession(player, monster);

        Assert.ThrowsException<TreasureException>(
            () => _service.LootMonster(session, monster));

        Assert.IsFalse(monster.IngredientsLooted);
        Assert.IsFalse(monster.TreasureLooted);

        _monsterDropDaoMock.Verify(d =>
                d.GetDropsByElementTypeAndThreatLevel(
                    It.IsAny<ElementType>(), It.IsAny<ThreatLevel>()),
            Times.Never);

        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        Assert.AreEqual(1, player.CurrentRoom!.Monsters.Count);
        Assert.AreEqual(1, session.CurrentFloor.Monsters.Count);
    }
    
    [TestMethod]
    public void LootMonster_WhenTransactionFails_DoesNotMarkIngredientsLooted()
    {
        var player = CreatePlayer();
        var monster = CreateElementalMonster();
        var session = CreateSession(player, monster);

        var ingredient = new Ingredient
        {
            Id = 6,
            Name = "Fire Essence",
            ComponentType = ComponentType.Essence
        };

        _monsterDropDaoMock
            .Setup(d => d.GetDropsByElementTypeAndThreatLevel(
                ElementType.Fire, ThreatLevel.Low))
            .Returns([CreateDrop(ingredient, 100m)]);

        _transactionManager
            .Setup(t => t.ExecuteInTransaction(It.IsAny<Action>()))
            .Callback<Action>(_ =>
                throw new InvalidOperationException("Transaction failed."));

        Assert.ThrowsException<InvalidOperationException>(
            () => _service.LootMonster(session, monster));

        Assert.IsFalse(monster.IngredientsLooted);
        Assert.IsFalse(monster.TreasureLooted);

        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        Assert.AreEqual(1, player.CurrentRoom!.Monsters.Count);
        Assert.AreEqual(1, session.CurrentFloor.Monsters.Count);
    }
    
    [TestMethod]
    public void LootMonster_WhenTreasureRetrySucceeds_DoesNotAwardIngredientsAgain()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateElementalMonster();
        var session = CreateSession(player, monster);

        var ingredient = new Ingredient
        {
            Id = 5,
            Name = "Fire Essence",
            ComponentType = ComponentType.Essence
        };

        var treasure = monster.Treasure
                       ?? throw new InvalidOperationException(
                           "Test monster should have treasure.");

        _monsterDropDaoMock
            .Setup(d => d.GetDropsByElementTypeAndThreatLevel(
                ElementType.Fire, ThreatLevel.Low))
            .Returns([CreateDrop(ingredient, 100m)]);

        // Force the first treasure transfer to fail.
        player.Inventory.Capacity = 1;

        // First attempt: ingredients succeed, treasure fails.
        Assert.ThrowsException<TreasureException>(
            () => _service.LootMonster(session, monster));

        Assert.IsTrue(monster.IngredientsLooted);
        Assert.IsFalse(monster.TreasureLooted);
        Assert.AreSame(treasure, monster.Treasure);

        // Restore inventory capacity for the retry.
        player.Inventory.Capacity = 100;

        // Second attempt: treasure succeeds.
        _service.LootMonster(session, monster);

        // Assert
        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(ingredient.Id, player.Id, 1),
            Times.Once);

        Assert.IsTrue(monster.IngredientsLooted);
        Assert.IsTrue(monster.TreasureLooted);

        Assert.IsTrue(player.Inventory.Items.Contains(treasure));
        Assert.IsNull(monster.Treasure);

        Assert.AreEqual(0, player.CurrentRoom!.Monsters.Count);
        Assert.AreEqual(0, session.CurrentFloor.Monsters.Count);
    }
    
    [TestMethod]
    public void LootMonster_WithNullSession_ThrowsArgumentNullException()
    {
        var monster = CreateMonster();

        Assert.ThrowsException<ArgumentNullException>(
            () => _service.LootMonster(null!, monster));
    }

    [TestMethod]
    public void LootMonster_WithNullMonster_ThrowsArgumentNullException()
    {
        var player = CreatePlayer();
        var session = CreateSession(player, CreateMonster());

        Assert.ThrowsException<ArgumentNullException>(
            () => _service.LootMonster(session, null!));
    }
    
    [TestMethod]
    public void LootMonster_WithMultipleIngredientDrops_AwardsAllIngredients()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateElementalMonster();
        var session = CreateSession(player, monster);

        var fireEssence = new Ingredient
        {
            Id = 7,
            Name = "Fire Essence",
            ComponentType = ComponentType.Essence
        };

        var fireCrystal = new Ingredient
        {
            Id = 8,
            Name = "Fire Crystal",
            ComponentType = ComponentType.Core
        };

        _monsterDropDaoMock
            .Setup(d => d.GetDropsByElementTypeAndThreatLevel(
                ElementType.Fire, ThreatLevel.Low))
            .Returns([
                CreateDrop(fireEssence, 100m),
                CreateDrop(fireCrystal, 100m)
            ]);

        // Act
        _service.LootMonster(session, monster);

        // Assert
        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(fireEssence.Id, player.Id, 1),
            Times.Once);

        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(fireCrystal.Id, player.Id, 1),
            Times.Once);

        Assert.IsTrue(monster.IngredientsLooted);
        Assert.IsTrue(monster.TreasureLooted);
    }
    
    [TestMethod]
    public void LootMonster_WhenTreasureAlreadyLooted_StillAwardsIngredients()
    {
        // Arrange
        var player = CreatePlayer();
        var monster = CreateElementalMonster();
        monster.TreasureLooted = true;

        var session = CreateSession(player, monster);

        var ingredient = new Ingredient
        {
            Id = 9,
            Name = "Fire Essence",
            ComponentType = ComponentType.Essence
        };

        _monsterDropDaoMock
            .Setup(d => d.GetDropsByElementTypeAndThreatLevel(
                ElementType.Fire, ThreatLevel.Low))
            .Returns([CreateDrop(ingredient, 100m)]);

        // Act
        _service.LootMonster(session, monster);

        // Assert
        _ingredientDaoMock.Verify(d =>
                d.AddIngredientToPlayer(ingredient.Id, player.Id, 1),
            Times.Once);

        Assert.IsTrue(monster.IngredientsLooted);
        Assert.IsTrue(monster.TreasureLooted);

        Assert.AreEqual(0, player.CurrentRoom!.Monsters.Count);
        Assert.AreEqual(0, session.CurrentFloor.Monsters.Count);
    }
    
}