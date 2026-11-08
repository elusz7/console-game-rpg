using ConsoleGame.Models;
using ConsoleGame.Services;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGameTests.Unit.ConsoleGame.Services;

[TestClass]
public class LootServiceTests
{
    // The service being tested.
    private readonly LootService _service = new();

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
        Assert.ThrowsException<TreasureException>(() => _service.LootMonster(session, monster));

        // The monster is removed even though no treasure was collected.
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

        // The treasure should be restored to the monster.
        Assert.AreSame(treasure, monster.Treasure);

        // The monster should still be removed from the room and floor.
        Assert.AreEqual(0, player.CurrentRoom!.Monsters.Count);
        Assert.AreEqual(0, session.CurrentFloor.Monsters.Count);
    }
}