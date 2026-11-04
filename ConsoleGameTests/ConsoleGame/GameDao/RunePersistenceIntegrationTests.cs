using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Runes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGameTests.ConsoleGame.GameDao;

[TestClass]
public class RunePersistenceIntegrationTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private RuneDao _runeDao = null!;

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
        _runeDao = new RuneDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }
    
    [TestMethod]
    public void GetRunesByPlayerId_WhenPlayerHasNoRunes_ReturnsEmptyList()
    {
        // Arrange
        var player = CreateAndSavePlayer("Player One");

        // Act
        var runes = _runeDao.GetRunesByPlayerId(player.Id);

        // Assert
        Assert.IsNotNull(runes);
        Assert.AreEqual(0, runes.Count);
    }

    [TestMethod]
    public void GetRunesByPlayerId_WhenPlayerHasRunes_ReturnsOnlyTheirRunes()
    {
        // Arrange
        var playerOne = CreateAndSavePlayer("Player One");
        var playerTwo = CreateAndSavePlayer("Player Two");

        var runeOne = CreateRune("Ember Rune");
        var runeTwo = CreateRune("Frost Rune");

        _context.Runes.AddRange(runeOne, runeTwo);
        _context.SaveChanges();

        _context.InventoryRunes.AddRange(
            new InventoryRune
            {
                InventoryId = playerOne.Inventory.Id,
                RuneId = runeOne.Id,
                Tier = 1,
                Quantity = 3
            },
            new InventoryRune
            {
                InventoryId = playerOne.Inventory.Id,
                RuneId = runeTwo.Id,
                Tier = 2,
                Quantity = 1
            },
            new InventoryRune
            {
                InventoryId = playerTwo.Inventory.Id,
                RuneId = runeOne.Id,
                Tier = 1,
                Quantity = 5
            });

        _context.SaveChanges();

        // Clear tracking so the DAO query reads from SQLite.
        _context.ChangeTracker.Clear();

        // Act
        var runes = _runeDao.GetRunesByPlayerId(playerOne.Id);

        // Assert
        Assert.IsNotNull(runes);
        Assert.AreEqual(2, runes.Count);

        Assert.IsTrue(runes.All(
            rune => rune.Inventory.PlayerId == playerOne.Id));

        Assert.IsTrue(runes.Any(
            rune => rune.RuneId == runeOne.Id &&
                    rune.Tier == 1 &&
                    rune.Quantity == 3));

        Assert.IsTrue(runes.Any(
            rune => rune.RuneId == runeTwo.Id &&
                    rune.Tier == 2 &&
                    rune.Quantity == 1));
    }

    [TestMethod]
    public void GetRuneDefinitionById_WhenRuneExists_ReturnsRune()
    {
        // Arrange
        var rune = CreateRune("Ember Rune");

        _context.Runes.Add(rune);
        _context.SaveChanges();

        var runeId = rune.Id;

        _context.ChangeTracker.Clear();

        // Act
        var result = _runeDao.GetRuneDefinitionById(runeId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(runeId, result.Id);
        Assert.AreEqual("Ember Rune", result.Name);
        Assert.IsInstanceOfType<WeaponRune>(result);
    }

    [TestMethod]
    public void GetRuneDefinitionById_WhenRuneDoesNotExist_ReturnsNull()
    {
        // Act
        var result = _runeDao.GetRuneDefinitionById(500);

        // Assert
        Assert.IsNull(result);
    }
    
    [TestMethod]
    public void AddRuneToPlayer_ZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => _runeDao.AddRuneToPlayer(999, 999, 1, 0));
    }

    [TestMethod]
    public void AddRuneToPlayer_NegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => _runeDao.AddRuneToPlayer(1, 1, 1, -5));
    }
    
    [TestMethod]
    public void AddRuneToPlayer_DefaultQuantity_AddsRuneToInventory()
    {
        // Arrange
        var player = CreateAndSavePlayer("RuneTestPlayer");
        var rune = CreateRune("Ember Rune");

        _context.Runes.Add(rune);
        _context.SaveChanges();

        // Act
        _runeDao.AddRuneToPlayer(rune.Id, player.Id, 1);

        // Assert
        _context.ChangeTracker.Clear();

        var runes = _runeDao.GetRunesByPlayerId(player.Id);

        Assert.AreEqual(1, runes.Count);
        Assert.AreEqual(rune.Id, runes[0].RuneId);
        Assert.AreEqual(1, runes[0].Tier);
        Assert.AreEqual(1, runes[0].Quantity);
    }
    
    [TestMethod]
    public void AddRuneToPlayer_ExistingRune_IncreasesQuantity()
    {
        // Arrange
        var player = CreateAndSavePlayer("RuneStackTestPlayer");
        var rune = CreateRune("Ember Rune");

        _context.Runes.Add(rune);
        _context.SaveChanges();

        _runeDao.AddRuneToPlayer(rune.Id, player.Id, 1, 2);

        // Act
        _runeDao.AddRuneToPlayer(rune.Id, player.Id, 1, 5);

        // Assert
        _context.ChangeTracker.Clear();

        var runes = _runeDao.GetRunesByPlayerId(player.Id);

        Assert.AreEqual(1, runes.Count);
        Assert.AreEqual(rune.Id, runes[0].RuneId);
        Assert.AreEqual(1, runes[0].Tier);
        Assert.AreEqual(7, runes[0].Quantity);
    }

    [TestMethod]
    public void AddRuneToPlayer_SameRuneDifferentTier_CreatesSeparateOwnershipRecords()
    {
        // Arrange
        var player = CreateAndSavePlayer("RuneStackTestPlayer");
        var rune = CreateRune("Ember Rune");

        _context.Runes.Add(rune);
        _context.SaveChanges();

        _runeDao.AddRuneToPlayer(rune.Id, player.Id, 1, 2);

        // Act
        _runeDao.AddRuneToPlayer(rune.Id, player.Id, 2, 5);

        // Assert
        _context.ChangeTracker.Clear();

        var runes = _runeDao.GetRunesByPlayerId(player.Id);

        Assert.AreEqual(2, runes.Count);

        var tierOneRune = runes.Single(r => r.Tier == 1);
        var tierTwoRune = runes.Single(r => r.Tier == 2);

        Assert.AreEqual(rune.Id, tierOneRune.RuneId);
        Assert.AreEqual(2, tierOneRune.Quantity);

        Assert.AreEqual(rune.Id, tierTwoRune.RuneId);
        Assert.AreEqual(5, tierTwoRune.Quantity);
    }
    
    [TestMethod]
    public void UpdatePlayerRuneQuantity_ZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => _runeDao.UpdatePlayerRuneQuantity(1, 1, 1, 0));
    }
    
    [TestMethod]
    public void UpdatePlayerRuneQuantity_NegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => _runeDao.UpdatePlayerRuneQuantity(1, 1, 1, -5));
    }
    
    [TestMethod]
    public void UpdatePlayerRuneQuantity_RuneNotOwned_ThrowsInventoryException()
    {
        // Arrange
        var player = CreateAndSavePlayer("RuneUpdateTestPlayer");
        var rune = CreateRune("Ember Rune");

        _context.Runes.Add(rune);
        _context.SaveChanges();

        // Act & Assert
        Assert.ThrowsException<InventoryException>(
            () => _runeDao.UpdatePlayerRuneQuantity(rune.Id, player.Id, 1, 5));
    }
    
    [TestMethod]
    public void UpdatePlayerRuneQuantity_ExistingRune_UpdatesQuantity()
    {
        // Arrange
        var player = CreateAndSavePlayer("RuneUpdateTestPlayer");
        var rune = CreateRune("Ember Rune");

        _context.Runes.Add(rune);
        _context.SaveChanges();

        _runeDao.AddRuneToPlayer(rune.Id, player.Id, 1, 5);

        // Act
        _runeDao.UpdatePlayerRuneQuantity(rune.Id, player.Id, 1, 1);

        // Assert
        _context.ChangeTracker.Clear();

        var runes = _runeDao.GetRunesByPlayerId(player.Id);

        Assert.AreEqual(1, runes.Count);

        var updatedRune = runes.Single(r => r.Tier == 1);

        Assert.AreEqual(rune.Id, updatedRune.RuneId);
        Assert.AreEqual(1, updatedRune.Quantity);
    }
    
    [TestMethod]
    public void RemoveRuneFromPlayer_WhenPlayerDoesNotOwnRune_ThrowsInventoryException()
    {
        // Arrange
        var player = CreateAndSavePlayer("RuneRemovalTestPlayer");
        var rune = CreateRune("Ember Rune");

        _context.Runes.Add(rune);
        _context.SaveChanges();

        // Act & Assert
        Assert.ThrowsException<InventoryException>(
            () => _runeDao.RemoveRuneFromPlayer(rune.Id, player.Id, 1));
    }
    
    [TestMethod]
    public void RemoveRuneFromPlayer_WhenPlayerOwnsRune_RemovesOwnershipRecord()
    {
        // Arrange
        var player = CreateAndSavePlayer("RuneRemovalTestPlayer");
        var rune = CreateRune("Ember Rune");

        _context.Runes.Add(rune);
        _context.SaveChanges();

        _runeDao.AddRuneToPlayer(rune.Id, player.Id, 1, 3);

        // Act
        _runeDao.RemoveRuneFromPlayer(rune.Id, player.Id, 1);

        // Assert
        _context.ChangeTracker.Clear();

        var runes = _runeDao.GetRunesByPlayerId(player.Id);
        var persistedRune = _context.Runes.Find(rune.Id);

        Assert.AreEqual(0, runes.Count);
        Assert.IsNotNull(persistedRune);
        Assert.AreEqual("Ember Rune", persistedRune.Name);
    }
    
    private Player CreateAndSavePlayer(string name)
    {
        var archetype = new Archetype
        {
            Name = "Warrior",
            Description = "A strong melee fighter.",
            HealthBase = 10,
            ArchetypeType = ArchetypeType.Martial,
            ResourceName = "Stamina",
            MaxResource = 20,
            RecoveryRate = 5,
            RecoveryGrowth = 1,
            AttackMultiplier = 1.0M,
            MagicMultiplier = 0.5M,
            DefenseMultiplier = 1.0M,
            ResistanceMultiplier = 0.5M,
            SpeedMultiplier = 0.5M,
            ResourceMultiplier = 1.0M
        };

        var player = new Player
        {
            Name = name,
            Archetype = archetype
        };

        player.Initialize();

        _context.Players!.Add(player);
        _context.SaveChanges();

        return player;
    }

    private static WeaponRune CreateRune(string name)
    {
        return new WeaponRune
        {
            Name = name,
            Power = 2,
            Element = ElementType.Fire,
            Rarity = RarityLevel.Common,
            Duration = 3
        };
    }
}