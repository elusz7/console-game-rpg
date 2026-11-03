using ConsoleGame.Models;
using ConsoleGame.Services;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGame.Factories.Interfaces;
using ConsoleGameEntities.Models.Items;
using Moq;

namespace ConsoleGameTests.ConsoleGame.Services;

[TestClass]
public class RoomNavigationServiceTests
{
    // Mocks the item factory used when refreshing merchant inventory.
    private readonly Mock<IItemFactory> _itemFactoryMock = new();
    
    // The service being tested.
    private readonly RoomNavigationService _service = new();

    // Creates a player with an initialized inventory and archetype.
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

    // Creates a floor with its entrance room and mocked item factory.
    private Floor CreateFloor()
    {
        return new Floor
        {
            Rooms =
            [
                new Room("Entrance", "The entrance to the floor.")
            ],
            ItemFactory = _itemFactoryMock.Object,
            Level = 1
        };
    }
    
    [TestMethod]
    public void ChangeCurrentRoom_MovesPlayerToConnectedRoom()
    {
        // Arrange
        var player = CreatePlayer();
        var entrance = CreateFloor().GetEntrance();
        var northRoom = new Room("North Room", "A room to the north.");

        // Connect the rooms.
        entrance.North = northRoom;

        var floor = new Floor
        {
            Rooms = [entrance, northRoom],
            ItemFactory = _itemFactoryMock.Object,
            Level = 1
        };

        // Configure the merchant inventory response.
        _itemFactoryMock
            .Setup(factory => factory.CreateMerchantItems(1))
            .Returns(new List<Item>());

        player.CurrentRoom = entrance;

        var session = new GameSession(player, floor, false);
        
        // Act
        _service.ChangeCurrentRoom(session, "North");

        // Assert
        Assert.AreSame(northRoom, player.CurrentRoom);
        // Verify merchant inventory is refreshed when leaving the entrance.
        _itemFactoryMock.Verify(
            factory => factory.CreateMerchantItems(1),
            Times.Once);
    }
    
    [TestMethod]
    public void ChangeCurrentRoom_InvalidDirection_ThrowsException()
    {
        // Arrange
        var player = CreatePlayer();
        var entrance = new Room("Entrance", "The entrance.");

        var floor = new Floor
        {
            Rooms = [entrance],
            ItemFactory = _itemFactoryMock.Object,
            Level = 1
        };

        player.CurrentRoom = entrance;

        var session = new GameSession(player, floor, false);

        // Verify that invalid movement throws an InvalidOperationException.
        Assert.ThrowsException<InvalidOperationException>(
            () => _service.ChangeCurrentRoom(session, "North"));

        // Confirm the player remains in the original room.
        Assert.AreSame(entrance, player.CurrentRoom);
    }

    [TestMethod]
    public void CleanUpDeadMonsters_RemovesMonstersWithoutTreasure()
    {
        // Arrange
        var player = CreatePlayer();
        var entrance = new Room("Entrance", "The entrance.");

        // Create one monster without treasure and one carrying treasure.
        var monsterWithoutTreasure = new Monster
        {
            Name = "Empty Goblin",
            Treasure = null
        };

        var monsterWithTreasure = new Monster
        {
            Name = "Loot Goblin",
            Treasure = new Weapon
            {
                Name = "Goblin Sword",
                Weight = 5,
                Value = 10
            }
        };

        entrance.Monsters.Add(monsterWithoutTreasure);
        entrance.Monsters.Add(monsterWithTreasure);

        var floor = new Floor
        {
            Rooms = [entrance],
            Monsters = [monsterWithoutTreasure, monsterWithTreasure],
            ItemFactory = _itemFactoryMock.Object,
            Level = 1
        };

        player.CurrentRoom = entrance;

        var session = new GameSession(player, floor, false);

        // Act
        _service.CleanUpDeadMonsters(session);

        // Assert
        Assert.AreEqual(1, entrance.Monsters.Count);
        Assert.AreSame(monsterWithTreasure, entrance.Monsters[0]);

        Assert.AreEqual(1, floor.Monsters.Count);
        Assert.AreSame(monsterWithTreasure, floor.Monsters[0]);
    }

    [TestMethod]
    public void ChangeCurrentRoom_WhenLeavingNonEntranceRoom_DoesNotRefreshMerchantItems()
    {
        // Arrange
        var player = CreatePlayer();

        var entrance = new Room("Entrance", "The entrance.");
        var currentRoom = new Room("Current Room", "A room in the dungeon.");
        var nextRoom = new Room("Next Room", "Another room.");

        // Connect the two non-entrance rooms.
        currentRoom.North = nextRoom;

        var floor = new Floor
        {
            Rooms = [entrance, currentRoom, nextRoom],
            ItemFactory = _itemFactoryMock.Object,
            Level = 1
        };

        player.CurrentRoom = currentRoom;

        var session = new GameSession(player, floor, false);

        // Act
        _service.ChangeCurrentRoom(session, "North");

        // Assert
        Assert.AreSame(nextRoom, player.CurrentRoom);

        // Merchant inventory should not be refreshed when leaving an ordinary room.
        _itemFactoryMock.Verify(
            factory => factory.CreateMerchantItems(It.IsAny<int>()),
            Times.Never);
    }
}