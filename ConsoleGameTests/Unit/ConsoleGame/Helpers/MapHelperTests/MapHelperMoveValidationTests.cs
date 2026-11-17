using ConsoleGame.Helpers;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Helpers.MapHelperTests;

[TestClass]
public class MapHelperMoveValidationTests
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

    private List<Room> FindRoomsDisconnectedByMove(
        Room entrance,
        Room roomToMove,
        Room newParent,
        string direction,
        params Room[] rooms)
    {
        return _mapHelper.FindRoomsDisconnectedByMove(
            entrance,
            roomToMove,
            newParent,
            direction,
            [.. rooms]);
    }

    [TestMethod]
    public void FindRoomsDisconnectedByMove_NoDependentRooms_ReturnsEmptyList()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var oldParent = CreateRoom("Old Parent");
        var newParent = CreateRoom("New Parent");
        var roomToMove = CreateRoom("Room To Move");

        Link(entrance, oldParent, "East");
        Link(entrance, newParent, "South");
        Link(oldParent, roomToMove, "East");

        // Act
        var result = FindRoomsDisconnectedByMove(
            entrance,
            roomToMove,
            newParent,
            "East",
            entrance,
            oldParent,
            newParent,
            roomToMove);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void FindRoomsDisconnectedByMove_RoomIsOnlyConnectionToBranch_ReturnsBranchRooms()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var oldParent = CreateRoom("Old Parent");
        var newParent = CreateRoom("New Parent");
        var roomToMove = CreateRoom("Room To Move");
        var branch = CreateRoom("Branch");

        Link(entrance, oldParent, "East");
        Link(entrance, newParent, "South");
        Link(oldParent, roomToMove, "East");
        Link(roomToMove, branch, "East");

        // Act
        var result = FindRoomsDisconnectedByMove(
            entrance,
            roomToMove,
            newParent,
            "East",
            entrance,
            oldParent,
            newParent,
            roomToMove,
            branch);

        // Assert
        CollectionAssert.AreEquivalent(
            new[] { branch },
            result);
    }

    [TestMethod]
    public void FindRoomsDisconnectedByMove_RoomHasMultiplePathsToEntrance_DoesNotReturnStillReachableRooms()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var oldParent = CreateRoom("Old Parent");
        var newParent = CreateRoom("New Parent");
        var alternateParent = CreateRoom("Alternate Parent");
        var roomToMove = CreateRoom("Room To Move");
        var branch = CreateRoom("Branch");

        Link(entrance, oldParent, "East");
        Link(entrance, newParent, "South");
        Link(entrance, alternateParent, "West");

        Link(oldParent, roomToMove, "East");

        // The branch has a second route through alternateParent.
        Link(roomToMove, branch, "North");
        Link(alternateParent, branch, "South");

        // Act
        var result = FindRoomsDisconnectedByMove(
            entrance,
            roomToMove,
            newParent,
            "East",
            entrance,
            oldParent,
            newParent,
            alternateParent,
            roomToMove,
            branch);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void FindRoomsDisconnectedByMove_ConnectedGroupBehindRoom_ReturnsAllNewlyDisconnectedMembers()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var oldParent = CreateRoom("Old Parent");
        var newParent = CreateRoom("New Parent");
        var roomToMove = CreateRoom("Room To Move");
        var branch1 = CreateRoom("Branch 1");
        var branch2 = CreateRoom("Branch 2");
        var branch3 = CreateRoom("Branch 3");

        Link(entrance, oldParent, "East");
        Link(entrance, newParent, "South");
        Link(oldParent, roomToMove, "East");

        Link(roomToMove, branch1, "East");
        Link(branch1, branch2, "East");
        Link(branch2, branch3, "East");

        // Act
        var result = FindRoomsDisconnectedByMove(
            entrance,
            roomToMove,
            newParent,
            "East",
            entrance,
            oldParent,
            newParent,
            roomToMove,
            branch1,
            branch2,
            branch3);

        // Assert
        CollectionAssert.AreEquivalent(
            new[] { branch1, branch2, branch3 },
            result);
    }

    [TestMethod]
    public void FindRoomsDisconnectedByMove_PreExistingDisconnectedGroup_IsNotReportedAlongsideNewlyDisconnectedBranch()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var oldParent = CreateRoom("Old Parent");
        var newParent = CreateRoom("New Parent");
        var roomToMove = CreateRoom("Room To Move");

        var newlyDisconnectedBranch = CreateRoom("Newly Disconnected Branch");

        var existingDisconnected1 = CreateRoom("Existing Disconnected 1");
        var existingDisconnected2 = CreateRoom("Existing Disconnected 2");

        Link(entrance, oldParent, "East");
        Link(entrance, newParent, "South");
        Link(oldParent, roomToMove, "East");

        Link(roomToMove, newlyDisconnectedBranch, "East");

        // This group is already disconnected before the proposed move.
        Link(existingDisconnected1, existingDisconnected2, "East");

        // Act
        var result = FindRoomsDisconnectedByMove(
            entrance,
            roomToMove,
            newParent,
            "East",
            entrance,
            oldParent,
            newParent,
            roomToMove,
            newlyDisconnectedBranch,
            existingDisconnected1,
            existingDisconnected2);

        // Assert
        CollectionAssert.AreEquivalent(
            new[] { newlyDisconnectedBranch },
            result);

        Assert.IsFalse(result.Contains(existingDisconnected1));
        Assert.IsFalse(result.Contains(existingDisconnected2));
    }

    [TestMethod]
    public void FindRoomsDisconnectedByMove_RoomWasAlreadyDisconnected_DoesNotReportItOrItsGroup()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var connectedParent = CreateRoom("Connected Parent");
        var disconnectedParent = CreateRoom("Disconnected Parent");
        var roomToMove = CreateRoom("Room To Move");
        var disconnectedBranch = CreateRoom("Disconnected Branch");

        Link(entrance, connectedParent, "East");

        // This entire group is initially disconnected.
        Link(disconnectedParent, roomToMove, "East");
        Link(roomToMove, disconnectedBranch, "East");

        // Act
        var result = FindRoomsDisconnectedByMove(
            entrance,
            roomToMove,
            connectedParent,
            "South",
            entrance,
            connectedParent,
            disconnectedParent,
            roomToMove,
            disconnectedBranch);

        // Assert
        Assert.AreEqual(0, result.Count);

        // The moved room becomes reachable, but its old group connection
        // is removed by UnlinkRoom, so the branch remains disconnected.
    }

    [TestMethod]
    public void FindRoomsDisconnectedByMove_DoesNotModifyOriginalConnections()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var oldParent = CreateRoom("Old Parent");
        var newParent = CreateRoom("New Parent");
        var roomToMove = CreateRoom("Room To Move");
        var branch = CreateRoom("Branch");

        Link(entrance, oldParent, "East");
        Link(entrance, newParent, "South");
        Link(oldParent, roomToMove, "East");
        Link(roomToMove, branch, "East");

        // Capture the original connections.
        var originalConnections = new Dictionary<Room, (Room? North, Room? South, Room? East, Room? West)>
        {
            [entrance] = (entrance.North, entrance.South, entrance.East, entrance.West),
            [oldParent] = (oldParent.North, oldParent.South, oldParent.East, oldParent.West),
            [newParent] = (newParent.North, newParent.South, newParent.East, newParent.West),
            [roomToMove] = (roomToMove.North, roomToMove.South, roomToMove.East, roomToMove.West),
            [branch] = (branch.North, branch.South, branch.East, branch.West)
        };

        // Act
        _mapHelper.FindRoomsDisconnectedByMove(
            entrance,
            roomToMove,
            newParent,
            "East",
            [entrance, oldParent, newParent, roomToMove, branch]);

        // Assert
        foreach (var (room, connections) in originalConnections)
        {
            Assert.AreSame(connections.North, room.North, $"{room.Name} North connection changed.");
            Assert.AreSame(connections.South, room.South, $"{room.Name} South connection changed.");
            Assert.AreSame(connections.East, room.East, $"{room.Name} East connection changed.");
            Assert.AreSame(connections.West, room.West, $"{room.Name} West connection changed.");
        }
    }

    [TestMethod]
    public void FindRoomsDisconnectedByMove_AttemptingToMoveEntrance_ThrowsArgumentException()
    {
        // Arrange
        var entrance = CreateRoom("Entrance", 1);
        var newParent = CreateRoom("New Parent");

        Link(entrance, newParent, "East");

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            _mapHelper.FindRoomsDisconnectedByMove(
                entrance,
                entrance,
                newParent,
                "South",
                [entrance, newParent]));
    }
}