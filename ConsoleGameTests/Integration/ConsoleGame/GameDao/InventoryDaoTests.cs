using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;

namespace ConsoleGameTests.Integration.ConsoleGame.GameDao;

[TestClass]
public class InventoryDaoTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private InventoryDao _inventoryDao = null!;

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
        _inventoryDao = new InventoryDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void UpdateInventory_PersistsInventoryChanges()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("TestPlayer", archetype, inventory);

        _context.Players!.Add(player);
        _context.SaveChanges();

        // Act
        inventory.Gold = 500;
        inventory.Capacity = 200;
        _inventoryDao.UpdateInventory(inventory);

        // Assert
        _context.ChangeTracker.Clear();

        var result = _context.Inventories!
            .Single(i => i.Id == inventory.Id);

        Assert.AreEqual(500, result.Gold);
        Assert.AreEqual(200, result.Capacity);
    }

    [TestMethod]
    public void GetAvailableItems_ReturnsUnownedItemsWithinAvailableCapacity()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        inventory.Capacity = 100;

        var ownedItem = CreateItem("Owned Sword", 1, 10, TestItemType.Weapon);
        ownedItem.Weight = 20;
        inventory.AddItem(ownedItem);

        var player = CreatePlayer("TestPlayer", archetype, inventory);

        var availableItem1 = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        availableItem1.Weight = 30;

        var availableItem2 = CreateItem("Leather Armor", 1, 10, TestItemType.Armor);
        availableItem2.Weight = 50;

        _context.Players!.Add(player);
        _context.Items!.AddRange(availableItem1, availableItem2);
        _context.SaveChanges();

        // Act
        var result = _inventoryDao.GetAvailableItems(player);

        // Assert
        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEquivalent(
            new[] { availableItem1.Id, availableItem2.Id },
            result.Select(i => i.Id).ToList());
    }

    [TestMethod]
    public void GetAvailableItems_ExcludesItemsAlreadyOwnedByAnInventory()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);

        var playerInventory = CreateInventory(100);
        playerInventory.Capacity = 100;

        var otherInventory = CreateInventory(100);
        otherInventory.Capacity = 100;

        var ownedItem = CreateItem("Owned Sword", 1, 10, TestItemType.Weapon);
        ownedItem.Weight = 20;
        otherInventory.AddItem(ownedItem);

        var availableItem = CreateItem("Available Sword", 1, 10, TestItemType.Weapon);
        availableItem.Weight = 30;

        var player = CreatePlayer("TestPlayer", archetype, playerInventory);
        var otherPlayer = CreatePlayer("OtherPlayer", archetype, otherInventory);

        _context.Players!.AddRange(player, otherPlayer);
        _context.Items!.Add(availableItem);
        _context.SaveChanges();

        // Act
        var result = _inventoryDao.GetAvailableItems(player);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(availableItem.Id, result.Single().Id);
        Assert.IsFalse(result.Any(i => i.Id == ownedItem.Id));
    }

    [TestMethod]
    public void GetAvailableItems_ExcludesItemsExceedingAvailableCapacity()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        inventory.Capacity = 100;

        var ownedItem = CreateItem("Owned Sword", 1, 10, TestItemType.Weapon);
        ownedItem.Weight = 40;
        inventory.AddItem(ownedItem);

        var availableItem = CreateItem("Heavy Armor", 1, 10, TestItemType.Armor);
        availableItem.Weight = 70;

        var player = CreatePlayer("TestPlayer", archetype, inventory);

        _context.Players!.Add(player);
        _context.Items!.Add(availableItem);
        _context.SaveChanges();

        // Act
        var result = _inventoryDao.GetAvailableItems(player);

        // Assert
        Assert.AreEqual(0, result.Count);
        Assert.IsFalse(result.Any(i => i.Id == availableItem.Id));
    }

    [TestMethod]
    public void GetAvailableItems_ReturnsEmptyListWhenNoItemsAreAvailable()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("TestPlayer", archetype, inventory);

        _context.Players!.Add(player);
        _context.SaveChanges();

        // Act
        var result = _inventoryDao.GetAvailableItems(player);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}