using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Models;
using ConsoleGame.Services;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using Moq;

namespace ConsoleGameTests.ConsoleGame.Services;

[TestClass]
public class FloorProgressionServiceTests
{
    // Mock the floor factory so tests don't depend on actual map generation.
    private readonly Mock<IFloorFactory> _floorFactoryMock = new();

    // The service being tested.
    private readonly FloorProgressionService _service;

    public FloorProgressionServiceTests()
    {
        // Inject the mocked factory into the service.
        _service = new FloorProgressionService(_floorFactoryMock.Object);
    }

    // Creates a player with the dependencies required by Player.LevelUp().
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

        // Initialize health, inventory, and starting level.
        player.Initialize();

        return player;
    }

    // Creates a floor containing the entrance room required by GetEntrance().
    private static Floor CreateFloor()
    {
        return new Floor
        {
            Rooms =
            [
                new Room("Entrance", "The entrance to the floor.")
            ]
        };
    }

    [TestMethod]
    public void AdvanceToNextFloor_IncreasesPlayerLevel()
    {
        // Arrange
        var player = CreatePlayer();
        var initialLevel = player.Level;

        var currentFloor = CreateFloor();
        var nextFloor = CreateFloor();

        var session = new GameSession(player, currentFloor, false);

        // Return the prepared floor when the service requests the next floor.
        _floorFactoryMock
            .Setup(factory => factory.CreateFloor(
                initialLevel + 1,
                false,
                false))
            .Returns(nextFloor);

        // Act
        _service.AdvanceToNextFloor(session, false);

        // Assert
        Assert.AreEqual(initialLevel + 1, player.Level);
        
        // Verify the next floor was generated using the updated player level.
        _floorFactoryMock.Verify(factory => factory.CreateFloor(
                initialLevel + 1,
                false,
                false),
            Times.Once);
    }

    [TestMethod]
    public void AdvanceToNextFloor_MovesPlayerToNewFloorEntrance()
    {
        // Arrange
        var player = CreatePlayer();

        var currentFloor = CreateFloor();
        var nextFloor = CreateFloor();

        var session = new GameSession(player, currentFloor, false);

        // Return the prepared floor when the service requests the next floor.
        _floorFactoryMock
            .Setup(factory => factory.CreateFloor(
                player.Level + 1,
                false,
                false))
            .Returns(nextFloor);

        // Act
        _service.AdvanceToNextFloor(session, false);

        // Assert
        Assert.AreSame(nextFloor.GetEntrance(), player.CurrentRoom);
        Assert.AreSame(nextFloor, session.CurrentFloor);
    }
    
    [TestMethod]
    public void AdvanceToNextFloor_CollectsMonsterTreasure()
    {
        // Arrange
        var player = CreatePlayer();
        var currentFloor = CreateFloor();
        var nextFloor = CreateFloor();

        // Give the player enough capacity to carry the weapon.
        player.Inventory.Capacity = 100;

        var weapon = new Weapon
        {
            Name = "Test Sword",
            Weight = 5,
            Value = 10
        };

        // Place a monster carrying treasure on the current floor.
        var monster = new Monster
        {
            Name = "Goblin",
            Treasure = weapon
        };

        currentFloor.Monsters.Add(monster);

        var session = new GameSession(player, currentFloor, false);

        _floorFactoryMock
            .Setup(factory => factory.CreateFloor(
                player.Level + 1,
                false,
                false))
            .Returns(nextFloor);

        // Act
        var collectedItems = _service.AdvanceToNextFloor(session, false);

        // Assert
        Assert.IsTrue(player.Inventory.Items.Contains(weapon));
        Assert.IsNull(monster.Treasure);
        Assert.AreEqual(1, collectedItems.Count);
        Assert.AreSame(weapon, collectedItems[0]);
    }

    [TestMethod]
    public void AdvanceToNextFloor_LeavesTreasureOnMonsterWhenInventoryIsFull()
    {
        // Arrange
        var player = CreatePlayer();

        // Set capacity below the weapon's weight.
        player.Inventory.Capacity = 1;

        var currentFloor = CreateFloor();
        var nextFloor = CreateFloor();

        var weapon = new Weapon
        {
            Name = "Heavy Sword",
            Weight = 10,
            Value = 20
        };

        var monster = new Monster
        {
            Name = "Goblin",
            Treasure = weapon
        };

        currentFloor.Monsters.Add(monster);

        var session = new GameSession(player, currentFloor, false);

        _floorFactoryMock
            .Setup(factory => factory.CreateFloor(
                player.Level + 1,
                false,
                false))
            .Returns(nextFloor);

        // Act
        var collectedItems = _service.AdvanceToNextFloor(session, false);

        // Assert
        Assert.IsFalse(player.Inventory.Items.Contains(weapon));
        Assert.AreSame(weapon, monster.Treasure);
        Assert.AreEqual(0, collectedItems.Count);
    }

    [TestMethod]
    public void AdvanceToNextFloor_CompletesWhenNoMonstersHaveTreasure()
    {
        // Arrange
        var player = CreatePlayer();

        var currentFloor = CreateFloor();
        var nextFloor = CreateFloor();

        // Add a monster without treasure.
        currentFloor.Monsters.Add(new Monster
        {
            Name = "Goblin",
            Treasure = null
        });

        var session = new GameSession(player, currentFloor, false);

        _floorFactoryMock
            .Setup(factory => factory.CreateFloor(
                player.Level + 1,
                false,
                false))
            .Returns(nextFloor);

        // Act
        var collectedItems = _service.AdvanceToNextFloor(session, false);

        // Assert
        Assert.AreEqual(0, collectedItems.Count);
        Assert.AreSame(nextFloor, session.CurrentFloor);
    }
    
    [TestMethod]
    public void AdvanceToNextFloor_PreservesMerchantProgressWhenMerchantWasMet()
    {
        // Arrange
        var player = CreatePlayer();
        var currentFloor = CreateFloor();
        var nextFloor = CreateFloor();

        // Mark the merchant as already encountered on the current floor.
        currentFloor.HasMetMerchant = true;

        var session = new GameSession(player, currentFloor, false);

        // Return the prepared next floor.
        _floorFactoryMock
            .Setup(factory => factory.CreateFloor(
                player.Level + 1,
                false,
                false))
            .Returns(nextFloor);

        // Act
        _service.AdvanceToNextFloor(session, false);

        // Assert
        Assert.IsTrue(nextFloor.HasMetMerchant);
    }

    [TestMethod]
    public void AdvanceToNextFloor_PreservesMerchantProgressWhenMerchantWasNotMet()
    {
        // Arrange
        var player = CreatePlayer();
        var currentFloor = CreateFloor();
        var nextFloor = CreateFloor();

        // Explicitly confirm the merchant hasn't been encountered.
        currentFloor.HasMetMerchant = false;

        var session = new GameSession(player, currentFloor, false);

        // Return the prepared next floor.
        _floorFactoryMock
            .Setup(factory => factory.CreateFloor(
                player.Level + 1,
                false,
                false))
            .Returns(nextFloor);

        // Act
        _service.AdvanceToNextFloor(session, false);

        // Assert
        Assert.IsFalse(nextFloor.HasMetMerchant);
    }
}