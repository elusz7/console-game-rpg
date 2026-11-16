using ConsoleGame.Helpers;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.MapHelperTests;

[TestClass]
public class MapHelperAvailableDirectionsTests
{
    private MapHelper _mapHelper = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mapHelper = new MapHelper();
    }

    private static Room CreateRoom(string name, int id = 0)
    {
        return new Room(name, $"{name} description") { Id = id };
    }

    private void Link(Room from, Room to, string direction)
    {
        _mapHelper.LinkRooms(from, to, direction);
    }

    // GetAvailableDirectionsForRoom

    [TestMethod]
    public void GetAvailableDirectionsForRoom_AllAdjacentPositionsAvailable_ReturnsAllDirections()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room = CreateRoom("Room");

        Link(entrance, room, "East");

        // Act
        var result = _mapHelper.GetAvailableDirectionsForRoom(room, [entrance, room]);

        // Assert
        CollectionAssert.AreEquivalent(
            new[] { "North", "South", "East" },
            result);
    }

    [TestMethod]
    [DataRow("North")]
    [DataRow("South")]
    [DataRow("East")]
    [DataRow("West")]
    public void GetAvailableDirectionsForRoom_AdjacentPositionOccupied_ExcludesOccupiedDirection(
        string occupiedDirection)
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room = CreateRoom("Room");
        var neighbor = CreateRoom("Neighbor");

        Link(entrance, room, "East");
        Link(room, neighbor, occupiedDirection);

        // Act
        var result = _mapHelper.GetAvailableDirectionsForRoom(room, [entrance, room, neighbor]);

        // Assert
        Assert.IsFalse(result.Contains(occupiedDirection));
    }

    [TestMethod]
    public void GetAvailableDirectionsForRoom_MultipleAdjacentPositionsOccupied_ReturnsOnlyAvailableDirections()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room = CreateRoom("Room");
        var north = CreateRoom("North");
        var east = CreateRoom("East");

        Link(entrance, room, "East");
        Link(room, north, "North");
        Link(room, east, "East");

        // Act
        var result = _mapHelper.GetAvailableDirectionsForRoom(
            room,
            [entrance, room, north, east]);

        // Assert
        CollectionAssert.AreEquivalent(
            new[] { "South" },
            result);
    }

    [TestMethod]
    public void GetAvailableDirectionsForRoom_AllAdjacentPositionsOccupied_ReturnsEmptyList()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room = CreateRoom("Room");

        var north = CreateRoom("North");
        var south = CreateRoom("South");
        var east = CreateRoom("East");
        var west = CreateRoom("West");

        Link(entrance, room, "East");

        Link(room, north, "North");
        Link(room, south, "South");
        Link(room, east, "East");
        Link(room, west, "West");

        // Act
        var result = _mapHelper.GetAvailableDirectionsForRoom(
            room,
            [entrance, room, north, south, east, west]);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetAvailableDirectionsForRoom_RoomNotInConnectedMap_ReturnsEmptyList()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room = CreateRoom("Room");
        var disconnectedRoom = CreateRoom("Disconnected");

        Link(entrance, room, "East");

        // Act
        var result = _mapHelper.GetAvailableDirectionsForRoom(
            disconnectedRoom,
            [entrance, room, disconnectedRoom]);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [DataRow("North", "South")]
    [DataRow("South", "North")]
    [DataRow("East", "West")]
    [DataRow("West", "East")]
    public void GetAvailableDirectionsForRoom_ReverseDirectionConflict_ExcludesConflictingDirection(
        string direction,
        string reverseDirection)
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room = CreateRoom("Room");
        var conflictingRoom = CreateRoom("Conflicting Room");

        Link(entrance, room, "East");

        // Create a reverse-direction conflict without occupying the target coordinate.
        switch (reverseDirection)
        {
            case "North":
                conflictingRoom.North = room;
                break;
            case "South":
                conflictingRoom.South = room;
                break;
            case "East":
                conflictingRoom.East = room;
                break;
            case "West":
                conflictingRoom.West = room;
                break;
        }

        // Act
        var result = _mapHelper.GetAvailableDirectionsForRoom(
            room,
            [entrance, room, conflictingRoom]);

        // Assert
        Assert.IsFalse(result.Contains(direction));
    }

    [TestMethod]
    public void GetAvailableDirectionsForRoom_EmptyRoomCollection_ReturnsEmptyList()
    {
        // Arrange
        var room = CreateRoom("Room");

        // Act
        var result = _mapHelper.GetAvailableDirectionsForRoom(room, []);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    // GetAvailableDirections2

    [TestMethod]
    public void GetAvailableDirections2_MultipleRoomsHaveAvailableDirections_ReturnsAllEligibleRooms()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room1 = CreateRoom("Room 1");
        var room2 = CreateRoom("Room 2");

        Link(entrance, room1, "East");
        Link(room1, room2, "East");

        // Act
        var result = _mapHelper.GetAvailableDirections2([entrance, room1, room2]);

        // Assert
        // Assert
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.ContainsKey(entrance));
        Assert.IsTrue(result.ContainsKey(room1));
        Assert.IsTrue(result.ContainsKey(room2));
    }

    [TestMethod]
    public void GetAvailableDirections2_RoomHasNoAvailableDirections_ExcludesRoom()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room = CreateRoom("Room");

        var north = CreateRoom("North");
        var south = CreateRoom("South");
        var east = CreateRoom("East");
        var west = CreateRoom("West");

        Link(entrance, room, "East");

        Link(room, north, "North");
        Link(room, south, "South");
        Link(room, east, "East");
        Link(room, west, "West");

        // Act
        var result = _mapHelper.GetAvailableDirections2(
            [entrance, room, north, south, east, west]);

        // Assert
        Assert.IsFalse(result.ContainsKey(room));
    }

    [TestMethod]
    public void GetAvailableDirections2_EditingRoomSupplied_ExcludesEditingRoom()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room = CreateRoom("Room");
        var editingRoom = CreateRoom("Editing Room");

        Link(entrance, room, "East");
        Link(room, editingRoom, "East");

        // Act
        var result = _mapHelper.GetAvailableDirections2(
            [entrance, room, editingRoom],
            editingRoom);

        // Assert
        Assert.IsFalse(result.ContainsKey(editingRoom));
        Assert.IsTrue(result.ContainsKey(entrance));
        Assert.IsTrue(result.ContainsKey(room));
    }

    [TestMethod]
    public void GetAvailableDirections2_EditingRoomIsCriticalConnection_DoesNotExcludeOtherRooms()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room = CreateRoom("Room");
        var editingRoom = CreateRoom("Editing Room");
        var branch = CreateRoom("Branch");

        Link(entrance, room, "East");
        Link(room, editingRoom, "East");
        Link(editingRoom, branch, "East");

        // Act
        var result = _mapHelper.GetAvailableDirections2(
            [entrance, room, editingRoom, branch],
            editingRoom);

        // Assert
        Assert.IsFalse(result.ContainsKey(editingRoom));
        Assert.IsTrue(result.ContainsKey(entrance));
        Assert.IsTrue(result.ContainsKey(room));
        Assert.IsTrue(result.ContainsKey(branch));
    }

    [TestMethod]
    public void GetAvailableDirections2_EditingRoomHasMultiplePaths_DoesNotExcludeOtherRooms()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var room1 = CreateRoom("Room 1");
        var room2 = CreateRoom("Room 2");
        var editingRoom = CreateRoom("Editing Room");

        Link(entrance, room1, "East");
        Link(entrance, room2, "South");
        Link(room1, editingRoom, "East");
        Link(room2, editingRoom, "East");

        // Act
        var result = _mapHelper.GetAvailableDirections2(
            [entrance, room1, room2, editingRoom],
            editingRoom);

        // Assert
        Assert.IsFalse(result.ContainsKey(editingRoom));
        Assert.IsTrue(result.ContainsKey(entrance));
        Assert.IsTrue(result.ContainsKey(room1));
        Assert.IsTrue(result.ContainsKey(room2));
    }

    [TestMethod]
    public void GetAvailableDirections2_EmptyRoomCollection_ReturnsEmptyDictionary()
    {
        // Act
        var result = _mapHelper.GetAvailableDirections2([]);

        // Assert
        Assert.AreEqual(0, result.Count);
    }
}