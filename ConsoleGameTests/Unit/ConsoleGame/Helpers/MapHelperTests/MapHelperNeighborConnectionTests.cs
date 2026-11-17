using System.Reflection;
using ConsoleGame.Helpers;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.MapHelperTests;

[TestClass]
public class MapHelperNeighborConnectionTests
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

    private void Link(Room from, Room to, string direction)
    {
        _mapHelper.LinkRooms(from, to, direction);
    }

    // Creates a square:
    //
    // Entrance -- Room A
    //    |          |
    // Room B  -- Room C
    //
    // The Entrance-to-Room A, Entrance-to-Room B,
    // and Room B-to-Room C connections already exist.
    // Room A-to-Room C is the one unlinked adjacency.

    private (Room entrance, Room roomA, Room roomB, Room roomC, List<Room> rooms)
        CreateSquareWithOneUnlinkedConnection()
    {
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);
        var roomC = CreateRoom("Room C", 4);

        Link(entrance, roomA, "East");
        Link(entrance, roomB, "South");
        Link(roomB, roomC, "East");

        return (
            entrance,
            roomA,
            roomB,
            roomC,
            [entrance, roomA, roomB, roomC]);
    }

    private void InvokeConnectNeighbors(List<Room> rooms)
    {
        var method = typeof(MapHelper).GetMethod(
            "ConnectNeighbors",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(method, "ConnectNeighbors method was not found.");

        method.Invoke(_mapHelper, [rooms]);
    }

    // FindUnlinkedNeighbors

    [TestMethod]
    public void FindUnlinkedNeighbors_OneUnlinkedAdjacentPair_ReturnsThatPair()
    {
        // Arrange
        var (entrance, roomA, _, roomC, rooms) =
            CreateSquareWithOneUnlinkedConnection();

        // Act
        var result = _mapHelper.FindUnlinkedNeighbors(rooms);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Room C", result["Room A-South"]);
    }

    [TestMethod]
    public void FindUnlinkedNeighbors_AlreadyLinkedAdjacentRooms_ReturnsEmptyDictionary()
    {
        // Arrange
        var (entrance, roomA, roomB, roomC, rooms) =
            CreateSquareWithOneUnlinkedConnection();

        Link(roomA, roomC, "South");

        // Act
        var result = _mapHelper.FindUnlinkedNeighbors(rooms);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void FindUnlinkedNeighbors_LinearMapWithNoAdditionalAdjacencies_ReturnsEmptyDictionary()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);

        Link(entrance, roomA, "East");
        Link(roomA, roomB, "East");

        var rooms = new List<Room> { entrance, roomA, roomB };

        // Act
        var result = _mapHelper.FindUnlinkedNeighbors(rooms);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void FindUnlinkedNeighbors_DisconnectedRoomNotOnGrid_IgnoresDisconnectedRoom()
    {
        // Arrange
        var (entrance, roomA, roomB, roomC, rooms) =
            CreateSquareWithOneUnlinkedConnection();

        var disconnectedRoom = CreateRoom("Disconnected");
        rooms.Add(disconnectedRoom);

        // Act
        var result = _mapHelper.FindUnlinkedNeighbors(rooms);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsFalse(result.Keys.Any(k => k.Contains("Disconnected")));
    }

    [TestMethod]
    public void FindUnlinkedNeighbors_ReversePairIsNotDuplicated_ReturnsSingleEntry()
    {
        // Arrange
        var (entrance, roomA, roomB, roomC, rooms) =
            CreateSquareWithOneUnlinkedConnection();

        // Act
        var result = _mapHelper.FindUnlinkedNeighbors(rooms);

        // Assert
        Assert.AreEqual(1, result.Count);

        Assert.IsTrue(
            result.ContainsKey("Room A-South") ||
            result.ContainsKey("Room C-North"));
    }

    [TestMethod]
    public void FindUnlinkedNeighbors_DoesNotModifyExistingConnections()
    {
        // Arrange
        var (entrance, roomA, roomB, roomC, rooms) =
            CreateSquareWithOneUnlinkedConnection();

        // Act
        _mapHelper.FindUnlinkedNeighbors(rooms);

        // Assert
        Assert.AreSame(roomA, entrance.East);
        Assert.AreSame(roomB, entrance.South);
        Assert.AreSame(roomC, roomB.East);

        Assert.IsNull(roomA.South);
        Assert.IsNull(roomC.North);
    }

    [TestMethod]
    public void FindUnlinkedNeighbors_EmptyRoomCollection_ThrowsInvalidOperationException()
    {
        // Arrange
        var rooms = new List<Room>();

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(
            () => _mapHelper.FindUnlinkedNeighbors(rooms));
    }

    [TestMethod]
    public void FindUnlinkedNeighbors_EntranceMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var roomA = CreateRoom("Room A");
        var roomB = CreateRoom("Room B");

        Link(roomA, roomB, "East");

        var rooms = new List<Room> { roomA, roomB };

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(
            () => _mapHelper.FindUnlinkedNeighbors(rooms));
    }

    // ParseKey

    [TestMethod]
    public void ParseKey_ValidKey_ReturnsRoomNameAndDirection()
    {
        // Arrange
        const string key = "Room A-East";

        // Act
        var result = _mapHelper.ParseKey(key);

        // Assert
        Assert.AreEqual("Room A", result.name);
        Assert.AreEqual("East", result.direction);
    }

    // ConnectNeighbors

    [TestMethod]
    public void ConnectNeighbors_OneUnlinkedAdjacentPair_CreatesConnection()
    {
        // Arrange
        var (entrance, roomA, roomB, roomC, rooms) =
            CreateSquareWithOneUnlinkedConnection();

        // Act
        InvokeConnectNeighbors(rooms);

        // Assert
        Assert.AreSame(roomC, roomA.South);
        Assert.AreSame(roomA, roomC.North);
    }

    [TestMethod]
    public void ConnectNeighbors_NoUnlinkedAdjacentPairs_PreservesExistingConnections()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);

        Link(entrance, roomA, "East");
        Link(roomA, roomB, "East");

        var rooms = new List<Room> { entrance, roomA, roomB };

        // Act
        InvokeConnectNeighbors(rooms);

        // Assert
        Assert.AreSame(roomA, entrance.East);
        Assert.AreSame(entrance, roomA.West);
        Assert.AreSame(roomB, roomA.East);
        Assert.AreSame(roomA, roomB.West);

        Assert.IsNull(entrance.North);
        Assert.IsNull(entrance.South);
        Assert.IsNull(roomB.North);
        Assert.IsNull(roomB.South);
    }

    [TestMethod]
    public void ConnectNeighbors_CreatesReciprocalConnections()
    {
        // Arrange
        var (entrance, roomA, roomB, roomC, rooms) =
            CreateSquareWithOneUnlinkedConnection();

        // Act
        InvokeConnectNeighbors(rooms);

        // Assert
        Assert.AreSame(roomC, roomA.South);
        Assert.AreSame(roomA, roomC.North);

        Assert.AreSame(roomB, entrance.South);
        Assert.AreSame(entrance, roomB.North);

        Assert.AreSame(roomC, roomB.East);
        Assert.AreSame(roomB, roomC.West);
    }

    [TestMethod]
    public void ConnectNeighbors_DoesNotRemoveExistingConnections()
    {
        // Arrange
        var (entrance, roomA, roomB, roomC, rooms) =
            CreateSquareWithOneUnlinkedConnection();

        // Act
        InvokeConnectNeighbors(rooms);

        // Assert
        Assert.AreSame(roomA, entrance.East);
        Assert.AreSame(entrance, roomA.West);

        Assert.AreSame(roomB, entrance.South);
        Assert.AreSame(entrance, roomB.North);

        Assert.AreSame(roomC, roomB.East);
        Assert.AreSame(roomB, roomC.West);
    }

    [TestMethod]
    public void ConnectNeighbors_DoesNotCreateConnectionsOutsideSuppliedRooms()
    {
        // Arrange
        var (entrance, roomA, roomB, roomC, rooms) =
            CreateSquareWithOneUnlinkedConnection();

        var outsideRoom = CreateRoom("Outside");

        // Act
        InvokeConnectNeighbors(rooms);

        // Assert
        foreach (var room in rooms)
        {
            Assert.AreNotSame(outsideRoom, room.North);
            Assert.AreNotSame(outsideRoom, room.South);
            Assert.AreNotSame(outsideRoom, room.East);
            Assert.AreNotSame(outsideRoom, room.West);
        }
    }
}
