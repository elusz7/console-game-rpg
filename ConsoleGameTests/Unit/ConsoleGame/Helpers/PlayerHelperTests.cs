using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers;

[TestClass]
public class PlayerHelperTests
{
    private Mock<IInputManager> _inputManager = null!;
    private Mock<IOutputManager> _outputManager = null!;
    private Mock<IPlayerDao> _playerDao = null!;
    private Mock<IArchetypeDao> _archetypeDao = null!;

    [TestInitialize]
    public void Initialize()
    {
        // Create fresh mocks before each test.
        _inputManager = new Mock<IInputManager>();
        _outputManager = new Mock<IOutputManager>();
        _playerDao = new Mock<IPlayerDao>();
        _archetypeDao = new Mock<IArchetypeDao>();
    }

    // Creates a real PlayerHelper with mocked dependencies.
    private PlayerHelper CreatePlayerHelper()
    {
        return new PlayerHelper(
            _inputManager.Object,
            _outputManager.Object,
            _playerDao.Object,
            _archetypeDao.Object);
    }

    [TestMethod]
    public void SavePlayer_WhenPlayerAlreadyExists_PreservesInventoryAndGold()
    {
        // Arrange: create the current player and their inventory.
        var player = new Player
        {
            Name = "Test Player",
            Level = 3,
            MaxHealth = 30,
            Inventory = new Inventory
            {
                Gold = 100,
                Capacity = 25,
                Items = new List<Item>
                {
                    new Item { Id = 1, Name = "Iron Sword" },
                    new Item { Id = 2, Name = "Health Potion" }
                }
            }
        };

        // Arrange: create the existing database player.
        var dbPlayer = new Player
        {
            Name = "Test Player",
            Level = 1,
            MaxHealth = 10,
            Inventory = new Inventory
            {
                Id = 10,
                Gold = 20,
                Capacity = 15,
                Items = new List<Item>
                {
                    new Item { Id = 1, Name = "Iron Sword" },
                    new Item { Id = 2, Name = "Health Potion" }
                }
            }
        };

        // Return the existing player when SavePlayer looks them up.
        _playerDao
            .Setup(dao => dao.GetPlayerByName(player.Name))
            .Returns(dbPlayer);

        var helper = CreatePlayerHelper();

        // Act: save the current player.
        helper.SavePlayer(player);

        // Assert: player progression was updated.
        Assert.AreEqual(3, dbPlayer.Level);
        Assert.AreEqual(30, dbPlayer.MaxHealth);

        // Assert: inventory properties were preserved.
        Assert.AreEqual(100, dbPlayer.Inventory.Gold);
        Assert.AreEqual(25, dbPlayer.Inventory.Capacity);

        // Assert: both inventory items remain.
        Assert.AreEqual(2, dbPlayer.Inventory.Items.Count);
        Assert.IsTrue(dbPlayer.Inventory.Items.Any(item => item.Name == "Iron Sword"));
        Assert.IsTrue(dbPlayer.Inventory.Items.Any(item => item.Name == "Health Potion"));

        // Assert: the existing player was updated, not inserted.
        _playerDao.Verify(dao => dao.UpdatePlayer(dbPlayer), Times.Once);
        _playerDao.Verify(dao => dao.AddPlayer(It.IsAny<Player>()), Times.Never);
    }


    [TestMethod]
    public void SavePlayer_WhenPlayerDoesNotExist_InsertsPlayerWithInventory()
    {
        // Arrange: create a new player with inventory.
        var player = new Player
        {
            Name = "New Player",
            Level = 1,
            MaxHealth = 20,
            Inventory = new Inventory
            {
                Gold = 75,
                Capacity = 20,
                Items = new List<Item>
                {
                    new Item { Name = "Iron Sword" },
                    new Item { Name = "Health Potion" }
                }
            }
        };

        // Indicate that this player does not already exist.
        _playerDao
            .Setup(dao => dao.GetPlayerByName(player.Name))
            .Returns((Player?)null);

        var helper = CreatePlayerHelper();

        // Act: save the new player.
        helper.SavePlayer(player);

        // Assert: the complete player is inserted.
        _playerDao.Verify(
            dao => dao.AddPlayer(It.Is<Player>(p =>
                p.Name == "New Player" &&
                p.Inventory.Gold == 75 &&
                p.Inventory.Items.Count == 2)),
            Times.Once);

        // A new player should not go through the update path.
        _playerDao.Verify(
            dao => dao.UpdatePlayer(It.IsAny<Player>()),
            Times.Never);
    }

    [TestMethod]
    public void SavePlayer_WhenItemWasRemoved_RemovesItemFromSavedInventory()
    {
        // Arrange: current player has only one item remaining.
        var player = new Player
        {
            Name = "Test Player",
            Inventory = new Inventory
            {
                Gold = 150,
                Capacity = 20,
                Items = new List<Item>
                {
                    new Item { Id = 1, Name = "Iron Sword" }
                }
            }
        };

        // The saved player still has both items.
        var dbPlayer = new Player
        {
            Name = "Test Player",
            Inventory = new Inventory
            {
                Id = 10,
                Gold = 100,
                Capacity = 20,
                Items = new List<Item>
                {
                    new Item { Id = 1, Name = "Iron Sword" },
                    new Item { Id = 2, Name = "Health Potion" }
                }
            }
        };

        _playerDao
            .Setup(dao => dao.GetPlayerByName(player.Name))
            .Returns(dbPlayer);

        var helper = CreatePlayerHelper();

        // Act: save the current player.
        helper.SavePlayer(player);

        // Assert: the removed item is no longer in the saved inventory.
        Assert.AreEqual(1, dbPlayer.Inventory.Items.Count);
        Assert.IsTrue(dbPlayer.Inventory.Items.Any(item => item.Name == "Iron Sword"));
        Assert.IsFalse(dbPlayer.Inventory.Items.Any(item => item.Name == "Health Potion"));

        // Confirm the updated player was saved.
        _playerDao.Verify(
            dao => dao.UpdatePlayer(dbPlayer),
            Times.Once);
    }

    [TestMethod]
    public void SavePlayer_WhenCalledRepeatedly_DoesNotDuplicateInventoryItems()
    {
        // Arrange: create the current player with one item.
        var player = new Player
        {
            Name = "Test Player",
            Inventory = new Inventory
            {
                Gold = 100,
                Capacity = 20,
                Items = new List<Item>
                {
                    new Item { Id = 1, Name = "Iron Sword" }
                }
            }
        };

        // Create the existing database player with the same item.
        var dbPlayer = new Player
        {
            Name = "Test Player",
            Inventory = new Inventory
            {
                Id = 10,
                Gold = 50,
                Capacity = 15,
                Items = new List<Item>
                {
                    new Item { Id = 1, Name = "Iron Sword" }
                }
            }
        };

        _playerDao
            .Setup(dao => dao.GetPlayerByName(player.Name))
            .Returns(dbPlayer);

        var helper = CreatePlayerHelper();

        // Act: save the same player twice.
        helper.SavePlayer(player);
        helper.SavePlayer(player);

        // Assert: the inventory still contains exactly one item.
        Assert.AreEqual(1, dbPlayer.Inventory.Items.Count);
        Assert.AreEqual("Iron Sword", dbPlayer.Inventory.Items[0].Name);

        // Confirm both save operations updated the existing player.
        _playerDao.Verify(
            dao => dao.UpdatePlayer(dbPlayer),
            Times.Exactly(2));
    }

    [TestMethod]
    public void SavePlayer_WhenMultipleNewItemsHaveZeroIds_AddsAllItems()
    {
        // Arrange: create a player with two new items.
        var player = new Player
        {
            Name = "Test Player",
            Inventory = new Inventory
            {
                Gold = 100,
                Capacity = 20,
                Items = new List<Item>
                {
                    new Item { Id = 0, Name = "Iron Sword" },
                    new Item { Id = 0, Name = "Health Potion" }
                }
            }
        };

        // The existing database player starts with an empty inventory.
        var dbPlayer = new Player
        {
            Name = "Test Player",
            Inventory = new Inventory
            {
                Id = 10,
                Gold = 50,
                Capacity = 20,
                Items = new List<Item>()
            }
        };

        _playerDao
            .Setup(dao => dao.GetPlayerByName(player.Name))
            .Returns(dbPlayer);

        var helper = CreatePlayerHelper();

        // Act: save the player.
        helper.SavePlayer(player);

        // Assert: both new items were added independently.
        Assert.AreEqual(2, dbPlayer.Inventory.Items.Count);
        Assert.IsTrue(dbPlayer.Inventory.Items.Any(item => item.Name == "Iron Sword"));
        Assert.IsTrue(dbPlayer.Inventory.Items.Any(item => item.Name == "Health Potion"));
    }

    [TestMethod]
    public void SavePlayer_WhenItemDurabilityChanges_UpdatesSavedItem()
    {
        // Arrange: current player has a damaged weapon.
        var player = new Player
        {
            Name = "Test Player",
            Inventory = new Inventory
            {
                Gold = 100,
                Capacity = 20,
                Items = new List<Item>
                {
                    new Item
                    {
                        Id = 1,
                        Name = "Iron Sword",
                        Durability = 40
                    }
                }
            }
        };

        // The database still has the weapon's previous durability.
        var dbPlayer = new Player
        {
            Name = "Test Player",
            Inventory = new Inventory
            {
                Id = 10,
                Gold = 100,
                Capacity = 20,
                Items = new List<Item>
                {
                    new Item
                    {
                        Id = 1,
                        Name = "Iron Sword",
                        Durability = 100
                    }
                }
            }
        };

        _playerDao
            .Setup(dao => dao.GetPlayerByName(player.Name))
            .Returns(dbPlayer);

        var helper = CreatePlayerHelper();

        // Act: save the current player.
        helper.SavePlayer(player);

        // Assert: the saved weapon reflects its current durability.
        Assert.AreEqual(40, dbPlayer.Inventory.Items[0].Durability);
    }
}