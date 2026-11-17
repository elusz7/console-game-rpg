using ConsoleGame.Helpers;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.MapHelperTests;

[TestClass]
public class MapHelperUnlinkTests
{
    private Mock<IRandomProvider> _randomProvider = null!;
    private MapHelper _mapHelper = null!;

    [TestInitialize]
    public void Setup()
    {
        _randomProvider = new Mock<IRandomProvider>();
        _mapHelper = new MapHelper(_randomProvider.Object);
    }

    private static Room CreateRoom(string name, int id = 0)
    {
        return new Room(name, $"{name} description") { Id = id };
    }

    [TestMethod]
    public void UnlinkRoom_RoomHasFourConnections_ClearsAllConnections()
    {
        // Arrange
        var room = CreateRoom("Room");
        var north = CreateRoom("North");
        var south = CreateRoom("South");
        var east = CreateRoom("East");
        var west = CreateRoom("West");

        room.North = north;
        room.South = south;
        room.East = east;
        room.West = west;

        north.South = room;
        south.North = room;
        east.West = room;
        west.East = room;

        // Act
        _mapHelper.UnlinkRoom(room);

        // Assert
        Assert.IsNull(room.North);
        Assert.IsNull(room.South);
        Assert.IsNull(room.East);
        Assert.IsNull(room.West);

        Assert.IsNull(north.South);
        Assert.IsNull(south.North);
        Assert.IsNull(east.West);
        Assert.IsNull(west.East);
    }

    [TestMethod]
    public void UnlinkRoom_RoomHasPartialConnections_ClearsExistingConnections()
    {
        // Arrange
        var room = CreateRoom("Room");
        var north = CreateRoom("North");
        var east = CreateRoom("East");

        room.North = north;
        room.East = east;

        north.South = room;
        east.West = room;

        // Act
        _mapHelper.UnlinkRoom(room);

        // Assert
        Assert.IsNull(room.North);
        Assert.IsNull(room.South);
        Assert.IsNull(room.East);
        Assert.IsNull(room.West);

        Assert.IsNull(north.South);
        Assert.IsNull(east.West);
    }

    [TestMethod]
    public void UnlinkRoom_RoomHasNoConnections_RemainsUnchanged()
    {
        // Arrange
        var room = CreateRoom("Room");

        // Act
        _mapHelper.UnlinkRoom(room);

        // Assert
        Assert.IsNull(room.North);
        Assert.IsNull(room.South);
        Assert.IsNull(room.East);
        Assert.IsNull(room.West);
    }

    [TestMethod]
    public void UnlinkDirection_North_ClearsBothEndsOfConnection()
    {
        AssertDirectionUnlinked("North", "South");
    }

    [TestMethod]
    public void UnlinkDirection_South_ClearsBothEndsOfConnection()
    {
        AssertDirectionUnlinked("South", "North");
    }

    [TestMethod]
    public void UnlinkDirection_East_ClearsBothEndsOfConnection()
    {
        AssertDirectionUnlinked("East", "West");
    }

    [TestMethod]
    public void UnlinkDirection_West_ClearsBothEndsOfConnection()
    {
        AssertDirectionUnlinked("West", "East");
    }

    [TestMethod]
    public void UnlinkDirection_InvalidDirection_DoesNotModifyConnections()
    {
        // Arrange
        var room = CreateRoom("Room");
        var neighbor = CreateRoom("Neighbor");

        room.North = neighbor;
        neighbor.South = room;

        // Act
        _mapHelper.UnlinkDirection(room, neighbor, "Invalid");

        // Assert
        Assert.AreSame(neighbor, room.North);
        Assert.AreSame(room, neighbor.South);
    }

    [TestMethod]
    public void CheckForDisconnectedRooms_NoDisconnectedRooms_ReturnsEmptyList()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);

        entrance.East = roomA;
        roomA.West = entrance;
        roomA.East = roomB;
        roomB.West = roomA;

        var rooms = new List<Room> { entrance, roomA, roomB };

        // Act
        var disconnected = _mapHelper.CheckForDisconnectedRooms(rooms);

        // Assert
        Assert.AreEqual(0, disconnected.Count);
        Assert.AreSame(entrance, rooms[0]);
        Assert.AreSame(roomA, rooms[1]);
        Assert.AreSame(roomB, rooms[2]);
    }

    [TestMethod]
    public void CheckForDisconnectedRooms_DisconnectedRoom_ReturnsAndUnlinksRoom()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var disconnectedRoom = CreateRoom("Disconnected", 3);

        entrance.East = roomA;
        roomA.West = entrance;

        var rooms = new List<Room>
        {
            entrance,
            roomA,
            disconnectedRoom
        };

        // Act
        var disconnected = _mapHelper.CheckForDisconnectedRooms(rooms);

        // Assert
        Assert.AreEqual(1, disconnected.Count);
        Assert.AreSame(disconnectedRoom, disconnected[0]);

        Assert.IsNull(disconnectedRoom.North);
        Assert.IsNull(disconnectedRoom.South);
        Assert.IsNull(disconnectedRoom.East);
        Assert.IsNull(disconnectedRoom.West);

        Assert.AreSame(roomA, entrance.East);
        Assert.AreSame(entrance, roomA.West);
    }

    [TestMethod]
    public void CheckForDisconnectedRooms_DisconnectedGroup_UnlinksEachRoomAndPreservesConnectedMap()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);
        var roomC = CreateRoom("Room C", 4);
        var roomD = CreateRoom("Room D", 5);

        entrance.East = roomA;
        roomA.West = entrance;

        roomB.East = roomC;
        roomC.West = roomB;

        roomC.East = roomD;
        roomD.West = roomC;

        var rooms = new List<Room>
        {
            entrance,
            roomA,
            roomB,
            roomC,
            roomD
        };

        // Act
        var disconnected = _mapHelper.CheckForDisconnectedRooms(rooms);

        // Assert
        Assert.AreEqual(3, disconnected.Count);
        CollectionAssert.AreEquivalent(
            new List<Room> { roomB, roomC, roomD },
            disconnected);

        Assert.IsNull(roomB.East);
        Assert.IsNull(roomC.West);
        Assert.IsNull(roomC.East);
        Assert.IsNull(roomD.West);

        Assert.AreSame(roomA, entrance.East);
        Assert.AreSame(entrance, roomA.West);
    }

    private void AssertDirectionUnlinked(
        string direction,
        string reverseDirection)
    {
        // Arrange
        var room = CreateRoom("Room");
        var neighbor = CreateRoom("Neighbor");

        _mapHelper.LinkRooms(room, neighbor, direction);

        // Act
        _mapHelper.UnlinkDirection(room, neighbor, direction);

        // Assert
        Assert.IsNull(GetNeighbor(room, direction));
        Assert.IsNull(GetNeighbor(neighbor, reverseDirection));
    }

    private static Room? GetNeighbor(Room room, string direction)
    {
        return direction switch
        {
            "North" => room.North,
            "South" => room.South,
            "East" => room.East,
            "West" => room.West,
            _ => null
        };
    }
}