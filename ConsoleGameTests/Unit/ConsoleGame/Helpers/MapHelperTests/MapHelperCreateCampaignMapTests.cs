using ConsoleGame.Helpers;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.MapHelperTests;

[TestClass]
public class MapHelperCreateCampaignMapTests
{
    private Mock<IRandomProvider> _randomProvider = null!;
    private MapHelper _mapHelper = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _randomProvider = new Mock<IRandomProvider>();
        _mapHelper = new MapHelper(_randomProvider.Object);
    }

    private static Room CreateRoom(string name, int id = 0)
    {
        return new Room(name, $"{name} description") { Id = id };
    }

    private static List<Room> CreateAvailableRooms(int count)
    {
        var rooms = new List<Room>
        {
            CreateRoom("Entrance", 1)
        };

        for (int i = 1; i <= count; i++)
        {
            rooms.Add(CreateRoom($"Room {i}", i + 1));
        }

        return rooms;
    }

    private static HashSet<Room> GetReachableRooms(Room entrance)
    {
        var visited = new HashSet<Room>();
        var queue = new Queue<Room>();

        queue.Enqueue(entrance);

        while (queue.Count > 0)
        {
            var room = queue.Dequeue();

            if (!visited.Add(room))
                continue;

            if (room.North != null) queue.Enqueue(room.North);
            if (room.South != null) queue.Enqueue(room.South);
            if (room.East != null) queue.Enqueue(room.East);
            if (room.West != null) queue.Enqueue(room.West);
        }

        return visited;
    }

    private static void AssertConnectionsAreReciprocal(List<Room> rooms)
    {
        var roomSet = rooms.ToHashSet();

        foreach (var room in rooms)
        {
            if (room.North != null)
            {
                Assert.IsTrue(roomSet.Contains(room.North));
                Assert.AreSame(room, room.North.South);
            }

            if (room.South != null)
            {
                Assert.IsTrue(roomSet.Contains(room.South));
                Assert.AreSame(room, room.South.North);
            }

            if (room.East != null)
            {
                Assert.IsTrue(roomSet.Contains(room.East));
                Assert.AreSame(room, room.East.West);
            }

            if (room.West != null)
            {
                Assert.IsTrue(roomSet.Contains(room.West));
                Assert.AreSame(room, room.West.East);
            }
        }
    }

    [TestMethod]
    public void CreateCampaignMap_RequestedRoomCount_ReturnsRequestedNumberOfRooms()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(10);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(6, entrance, availableRooms);

        // Assert
        Assert.AreEqual(6, result.Count);
    }

    [TestMethod]
    public void CreateCampaignMap_EntranceIsIncluded_ReturnsEntrance()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(5);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(4, entrance, availableRooms);

        // Assert
        Assert.IsTrue(result.Contains(entrance));
    }

    [TestMethod]
    public void CreateCampaignMap_OnlyEntranceAllowed_ReturnsOnlyEntrance()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(5);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(1, entrance, availableRooms);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(entrance, result[0]);
    }

    [TestMethod]
    public void CreateCampaignMap_ZeroRoomsAllowed_ReturnsOnlyEntrance()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(5);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(0, entrance, availableRooms);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(entrance, result[0]);
    }

    [TestMethod]
    public void CreateCampaignMap_NegativeRoomLimit_ReturnsOnlyEntrance()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(5);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(-5, entrance, availableRooms);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(entrance, result[0]);
    }

    [TestMethod]
    public void CreateCampaignMap_RoomLimitExceedsAvailableRooms_ReturnsAllAvailableRooms()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(3);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(20, entrance, availableRooms);

        // Assert
        Assert.AreEqual(4, result.Count);
        Assert.IsTrue(result.Contains(entrance));
    }

    [TestMethod]
    public void CreateCampaignMap_EmptyAvailableRooms_ReturnsOnlyEntrance()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var availableRooms = new List<Room>();

        // Act
        var result = _mapHelper.CreateCampaignMap(5, entrance, availableRooms);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(entrance, result[0]);
    }

    [TestMethod]
    public void CreateCampaignMap_EntranceAlreadyInAvailableRooms_DoesNotDuplicateEntrance()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(5);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(5, entrance, availableRooms);

        // Assert
        Assert.AreEqual(1, result.Count(r => ReferenceEquals(r, entrance)));
        Assert.AreEqual(result.Count, result.Distinct().Count());
    }

    [TestMethod]
    public void CreateCampaignMap_GeneratedRoomsAreConnected_EveryRoomIsReachableFromEntrance()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(15);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(10, entrance, availableRooms);

        // Assert
        var reachableRooms = GetReachableRooms(entrance);

        Assert.AreEqual(result.Count, reachableRooms.Count);

        foreach (var room in result)
        {
            Assert.IsTrue(
                reachableRooms.Contains(room),
                $"{room.Name} should be reachable from the entrance.");
        }
    }

    [TestMethod]
    public void CreateCampaignMap_GeneratedRoomsHaveNoDuplicates()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(15);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(10, entrance, availableRooms);

        // Assert
        Assert.AreEqual(result.Count, result.Distinct().Count());
    }

    [TestMethod]
    public void CreateCampaignMap_GeneratedConnectionsAreReciprocal()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(15);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(10, entrance, availableRooms);

        // Assert
        AssertConnectionsAreReciprocal(result);
    }

    [TestMethod]
    public void CreateCampaignMap_GeneratedRoomsOccupyUniqueGridPositions()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(15);
        var entrance = availableRooms[0];

        // Act
        var result = _mapHelper.CreateCampaignMap(10, entrance, availableRooms);

        // Assert
        var grid = _mapHelper.GetRoomGrid(entrance);

        Assert.AreEqual(result.Count, grid.Count);
        Assert.AreEqual(result.Count, grid.Values.Distinct().Count());
    }

    [TestMethod]
    public void CreateCampaignMap_AvailableRoomsList_RemovesEntrance()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(5);
        var entrance = availableRooms[0];

        // Act
        _mapHelper.CreateCampaignMap(3, entrance, availableRooms);

        // Assert
        Assert.IsFalse(availableRooms.Contains(entrance));
    }

    [TestMethod]
    public void CreateCampaignMap_AvailableRoomsList_ContainsAllRemainingUnselectedRooms()
    {
        // Arrange
        var availableRooms = CreateAvailableRooms(10);
        var entrance = availableRooms[0];
        var originalRooms = availableRooms.ToList();

        // Act
        var result = _mapHelper.CreateCampaignMap(4, entrance, availableRooms);

        // Assert
        Assert.AreEqual(originalRooms.Count - 1, availableRooms.Count);
        Assert.IsFalse(availableRooms.Contains(entrance));

        foreach (var room in availableRooms)
        {
            Assert.IsTrue(originalRooms.Contains(room));
        }

        Assert.AreEqual(4, result.Count);
    }
}