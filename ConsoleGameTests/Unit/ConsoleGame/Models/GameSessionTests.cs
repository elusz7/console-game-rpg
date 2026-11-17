using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Models;

[TestClass]
public class GameSessionTests
{
    private Mock<IItemFactory> _itemFactory = new();
    private Mock<IRandomProvider> _randomProvider = new();
    
    // Creates a player with the dependencies required by Player.Initialize().
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

        // Initialize the player's health and inventory.
        player.Initialize();

        return player;
    }

    [TestMethod]
    public void IsFloorCleared_WhenLivingMonsterExists_ReturnsFalse()
    {
        // Arrange
        var floor = new Floor(_itemFactory.Object, _randomProvider.Object);

        // Add a living monster to the floor.
        floor.Monsters.Add(new Monster
        {
            Name = "Goblin",
            CurrentHealth = 10
        });

        var session = new GameSession(CreatePlayer(), floor, false);

        // Act
        var isFloorCleared = session.IsFloorCleared;

        // Assert
        Assert.IsFalse(isFloorCleared);
    }

    [TestMethod]
    public void IsFloorCleared_WhenNoLivingMonstersExist_ReturnsTrue()
    {
        // Arrange
        var floor = new Floor(_itemFactory.Object, _randomProvider.Object);

        // Add a defeated monster to confirm dead monsters don't prevent completion.
        floor.Monsters.Add(new Monster
        {
            Name = "Goblin",
            CurrentHealth = 0
        });

        var session = new GameSession(CreatePlayer(), floor, false);

        // Act
        var isFloorCleared = session.IsFloorCleared;

        // Assert
        Assert.IsTrue(isFloorCleared);
    }


    [TestMethod]
    public void Constructor_SetsInitialStatusToRunning()
    {
        // Arrange
        var player = CreatePlayer();
        var floor = new Floor(_itemFactory.Object, _randomProvider.Object);

        // Act
        var session = new GameSession(player, floor, false);

        // Assert
        Assert.AreEqual(AdventureStatus.Running, session.Status);
    }

    [TestMethod]
    public void End_WithQuit_SetsStatusToQuit()
    {
        // Arrange
        var floor = new Floor(_itemFactory.Object, _randomProvider.Object);
        var session = new GameSession(CreatePlayer(), floor, false);

        // Act
        session.End(AdventureStatus.Quit);

        // Assert
        Assert.AreEqual(AdventureStatus.Quit, session.Status);
    }

    [TestMethod]
    public void End_WithPlayerDead_SetsStatusToPlayerDead()
    {
        // Arrange
        var floor = new Floor(_itemFactory.Object, _randomProvider.Object);
        var session = new GameSession(CreatePlayer(), floor, false);

        // Act
        session.End(AdventureStatus.PlayerDead);

        // Assert
        Assert.AreEqual(AdventureStatus.PlayerDead, session.Status);
    }

    [TestMethod]
    public void End_WithRunningOrNotStarted_ThrowsArgumentException(
    )
    {
        // Arrange
        var floor = new Floor(_itemFactory.Object, _randomProvider.Object);
        var session = new GameSession(CreatePlayer(), floor, false);

        // Act and Assert
        Assert.ThrowsException<ArgumentException>(() => session.End(AdventureStatus.Running));

        Assert.ThrowsException<ArgumentException>(() => session.End(AdventureStatus.NotStarted));
    }
}