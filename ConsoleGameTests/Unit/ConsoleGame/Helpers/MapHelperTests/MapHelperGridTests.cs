using ConsoleGame.Helpers;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.MapHelperTests;

[TestClass]
public class MapHelperTests
{
    private MapHelper _mapHelper = null!;

    [TestInitialize]
    public void Setup()
    {
        _mapHelper = new MapHelper();
    }

    private static Room CreateRoom(string name, int id)
    {
        return new Room(name, $"{name} description")
        {
            Id = id
        };
    }

    #region LinkRooms

    [TestMethod]
    [DataRow("North")]
    [DataRow("South")]
    [DataRow("East")]
    [DataRow("West")]
    public void LinkRooms_ShouldCreateBidirectionalConnection(string direction)
    {
        // Arrange
        var roomA = CreateRoom("Room A", 1);
        var roomB = CreateRoom("Room B", 2);

        // Act
        _mapHelper.LinkRooms(roomA, roomB, direction);

        // Assert
        Assert.AreSame(roomB, _mapHelper.GetNeighbor(roomA, direction));
        Assert.AreSame(roomA, _mapHelper.GetNeighbor(
            roomB, _mapHelper.GetReverseDirection(direction)));
    }

    #endregion

    #region UnlinkRooms

    [TestMethod]
    [DataRow("North")]
    [DataRow("South")]
    [DataRow("East")]
    [DataRow("West")]
    public void UnlinkDirection_ShouldRemoveBidirectionalConnection(string direction)
    {
        // Arrange
        var roomA = CreateRoom("Room A", 1);
        var roomB = CreateRoom("Room B", 2);

        _mapHelper.LinkRooms(roomA, roomB, direction);

        // Act
        _mapHelper.UnlinkDirection(roomA, roomB, direction);

        // Assert
        Assert.IsNull(_mapHelper.GetNeighbor(roomA, direction));
        Assert.IsNull(_mapHelper.GetNeighbor(
            roomB, _mapHelper.GetReverseDirection(direction)));
    }

    [TestMethod]
    public void UnlinkRoom_ShouldRemoveAllConnections()
    {
        // Arrange
        var room = CreateRoom("Room", 1);
        var north = CreateRoom("North", 2);
        var south = CreateRoom("South", 3);
        var east = CreateRoom("East", 4);
        var west = CreateRoom("West", 5);

        _mapHelper.LinkRooms(room, north, "North");
        _mapHelper.LinkRooms(room, south, "South");
        _mapHelper.LinkRooms(room, east, "East");
        _mapHelper.LinkRooms(room, west, "West");

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

    #endregion

    #region GetNeighbor

    [TestMethod]
    [DataRow("North")]
    [DataRow("South")]
    [DataRow("East")]
    [DataRow("West")]
    public void GetNeighbor_ShouldReturnCorrectNeighbor(string direction)
    {
        // Arrange
        var roomA = CreateRoom("Room A", 1);
        var roomB = CreateRoom("Room B", 2);

        _mapHelper.LinkRooms(roomA, roomB, direction);

        // Act
        var result = _mapHelper.GetNeighbor(roomA, direction);

        // Assert
        Assert.AreSame(roomB, result);
    }

    [TestMethod]
    public void GetNeighbor_ShouldReturnNullForInvalidDirection()
    {
        // Arrange
        var room = CreateRoom("Room", 1);

        // Act
        var result = _mapHelper.GetNeighbor(room, "Invalid");

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region GetReverseDirection

    [TestMethod]
    [DataRow("North", "South")]
    [DataRow("South", "North")]
    [DataRow("East", "West")]
    [DataRow("West", "East")]
    public void GetReverseDirection_ShouldReturnOppositeDirection(
        string direction,
        string expected)
    {
        // Act
        var result = _mapHelper.GetReverseDirection(direction);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void GetReverseDirection_ShouldReturnEmptyForInvalidDirection()
    {
        // Act
        var result = _mapHelper.GetReverseDirection("Invalid");

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    #endregion

    #region GetDirectionOffset

    [TestMethod]
    [DataRow("North", 0, -1)]
    [DataRow("South", 0, 1)]
    [DataRow("East", 1, 0)]
    [DataRow("West", -1, 0)]
    [DataRow("Invalid", 0, 0)]
    public void GetDirectionOffset_ShouldReturnCorrectOffset(
        string direction,
        int expectedX,
        int expectedY)
    {
        // Act
        var result = _mapHelper.GetDirectionOffset(direction);

        // Assert
        Assert.AreEqual(expectedX, result.dx);
        Assert.AreEqual(expectedY, result.dy);
    }

    #endregion

    #region GetRoomGrid

    [TestMethod]
    public void GetRoomGrid_ShouldReturnCorrectCoordinatesForConnectedRooms()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var north = CreateRoom("North", 2);
        var south = CreateRoom("South", 3);
        var east = CreateRoom("East", 4);
        var west = CreateRoom("West", 5);

        _mapHelper.LinkRooms(entrance, north, "North");
        _mapHelper.LinkRooms(entrance, south, "South");
        _mapHelper.LinkRooms(entrance, east, "East");
        _mapHelper.LinkRooms(entrance, west, "West");

        // Act
        var grid = _mapHelper.GetRoomGrid(entrance);

        // Assert
        Assert.AreEqual(5, grid.Count);

        Assert.AreSame(entrance, grid[(0, 0)]);
        Assert.AreSame(north, grid[(0, -1)]);
        Assert.AreSame(south, grid[(0, 1)]);
        Assert.AreSame(east, grid[(1, 0)]);
        Assert.AreSame(west, grid[(-1, 0)]);
    }

    [TestMethod]
    public void GetRoomGrid_ShouldHandleCyclesWithoutInfiniteTraversal()
    {
        // Arrange
        var roomA = CreateRoom("Room A", 1);
        var roomB = CreateRoom("Room B", 2);
        var roomC = CreateRoom("Room C", 3);
        var roomD = CreateRoom("Room D", 4);

        _mapHelper.LinkRooms(roomA, roomB, "East");
        _mapHelper.LinkRooms(roomB, roomC, "South");
        _mapHelper.LinkRooms(roomC, roomD, "West");
        _mapHelper.LinkRooms(roomD, roomA, "North");

        // Act
        var grid = _mapHelper.GetRoomGrid(roomA);

        // Assert
        Assert.AreEqual(4, grid.Count);
    }

    [TestMethod]
    public void GetRoomGrid_ShouldReturnOnlyReachableRooms()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var connected = CreateRoom("Connected", 2);
        var disconnected = CreateRoom("Disconnected", 3);

        _mapHelper.LinkRooms(entrance, connected, "East");

        // Act
        var grid = _mapHelper.GetRoomGrid(entrance);

        // Assert
        Assert.AreEqual(2, grid.Count);
        Assert.IsFalse(grid.Values.Contains(disconnected));
    }

    #endregion

    #region FindRoomCoordinates

    [TestMethod]
    public void FindRoomCoordinates_ShouldReturnCorrectCoordinates()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var north = CreateRoom("North", 2);

        _mapHelper.LinkRooms(entrance, north, "North");

        var grid = _mapHelper.GetRoomGrid(entrance);

        // Act
        var result = _mapHelper.FindRoomCoordinates(north, grid);

        // Assert
        Assert.AreEqual((0, -1), result);
    }

    [TestMethod]
    public void FindRoomCoordinates_ShouldThrowWhenRoomIsNotInGrid()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var missing = CreateRoom("Missing", 2);

        var grid = _mapHelper.GetRoomGrid(entrance);

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(
            () => _mapHelper.FindRoomCoordinates(missing, grid));
    }

    #endregion

    #region HasReverseConflict

    [TestMethod]
    [DataRow("North")]
    [DataRow("South")]
    [DataRow("East")]
    [DataRow("West")]
    public void HasReverseConflict_ShouldReturnTrueWhenReverseConnectionExists(
        string direction)
    {
        // Arrange
        var baseRoom = CreateRoom("Base", 1);
        var neighbor = CreateRoom("Neighbor", 2);

        _mapHelper.LinkRooms(baseRoom, neighbor, direction);

        // Act
        var result = _mapHelper.HasReverseConflict(
            baseRoom, direction, [neighbor]);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasReverseConflict_ShouldReturnFalseWhenNoConflictExists()
    {
        // Arrange
        var baseRoom = CreateRoom("Base", 1);
        var otherRoom = CreateRoom("Other", 2);

        // Act
        var result = _mapHelper.HasReverseConflict(
            baseRoom, "North", [otherRoom]);

        // Assert
        Assert.IsFalse(result);
    }

    #endregion

    #region Connectivity

    [TestMethod]
    public void FindUnconnectedRooms_ShouldReturnOnlyRoomsNotReachableFromEntrance()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);
        var roomC = CreateRoom("Room C", 4);

        _mapHelper.LinkRooms(entrance, roomA, "East");
        _mapHelper.LinkRooms(roomA, roomB, "South");

        var rooms = new List<Room>
        {
            entrance,
            roomA,
            roomB,
            roomC
        };

        // Act
        var result = _mapHelper.FindUnconnectedRooms(entrance, rooms);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(roomC, result[0]);
    }

    [TestMethod]
    public void FindUnconnectedRooms_ShouldReturnEmptyWhenAllRoomsAreConnected()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);

        _mapHelper.LinkRooms(entrance, roomA, "East");
        _mapHelper.LinkRooms(roomA, roomB, "South");

        var rooms = new List<Room> { entrance, roomA, roomB };

        // Act
        var result = _mapHelper.FindUnconnectedRooms(entrance, rooms);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void FindUnconnectedRooms_ShouldReturnAllRoomsWhenEntranceHasNoConnections()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);

        var rooms = new List<Room> { entrance, roomA, roomB };

        // Act
        var result = _mapHelper.FindUnconnectedRooms(entrance, rooms);

        // Assert
        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEquivalent(
            new[] { roomA, roomB }, result);
    }

    [TestMethod]
    public void FindUnconnectedRooms_ShouldHandleCycles()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);
        var roomC = CreateRoom("Room C", 4);

        _mapHelper.LinkRooms(entrance, roomA, "East");
        _mapHelper.LinkRooms(roomA, roomB, "South");
        _mapHelper.LinkRooms(roomB, roomC, "West");
        _mapHelper.LinkRooms(roomC, entrance, "North");

        var rooms = new List<Room>
        {
            entrance, roomA, roomB, roomC
        };

        // Act
        var result = _mapHelper.FindUnconnectedRooms(entrance, rooms);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void CheckForDisconnectedRooms_ShouldClearDisconnectedRooms()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);
        var roomC = CreateRoom("Room C", 4);

        _mapHelper.LinkRooms(entrance, roomA, "East");
        _mapHelper.LinkRooms(roomA, roomB, "South");
        _mapHelper.LinkRooms(roomC, CreateRoom("External", 5), "West");

        var rooms = new List<Room>
        {
            entrance, roomA, roomB, roomC
        };

        // Act
        var result = _mapHelper.CheckForDisconnectedRooms(rooms);

        // Assert
        CollectionAssert.AreEquivalent(new[] { roomC }, result);

        Assert.IsNull(roomC.North);
        Assert.IsNull(roomC.South);
        Assert.IsNull(roomC.East);
        Assert.IsNull(roomC.West);
    }

    [TestMethod]
    public void CheckForDisconnectedRooms_ShouldNotModifyConnectedRooms()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 2);
        var roomB = CreateRoom("Room B", 3);

        _mapHelper.LinkRooms(entrance, roomA, "East");
        _mapHelper.LinkRooms(roomA, roomB, "South");

        var rooms = new List<Room> { entrance, roomA, roomB };

        // Act
        var result = _mapHelper.CheckForDisconnectedRooms(rooms);

        // Assert
        Assert.AreEqual(0, result.Count);

        Assert.AreSame(roomA, entrance.East);
        Assert.AreSame(roomB, roomA.South);
    }

    #endregion
    
    [TestMethod]
    public void FindRoomCoordinates_ShouldNotMatchDifferentRoomWithSameId()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var roomA = CreateRoom("Room A", 0);
        var roomB = CreateRoom("Room B", 0);

        _mapHelper.LinkRooms(entrance, roomA, "East");

        var grid = _mapHelper.GetRoomGrid(entrance);

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(
            () => _mapHelper.FindRoomCoordinates(roomB, grid));
    }
}