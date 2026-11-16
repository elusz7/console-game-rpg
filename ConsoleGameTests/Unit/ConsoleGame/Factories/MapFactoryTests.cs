using ConsoleGame.Factories;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers;
using ConsoleGame.Helpers.Interfaces;
using ConsoleGameEntities.Models.Entities;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Factories;

[TestClass]
public class MapFactoryTests
{
    private Mock<IRoomDao> _roomDaoMock = null!;
    private MapFactory _mapFactory = null!;

    [TestInitialize]
    public void Setup()
    {
        _roomDaoMock = new Mock<IRoomDao>();
        _mapFactory = new MapFactory(
            _roomDaoMock.Object,
            new MapHelper());
    }

    [TestMethod]
    public void GenerateMap_CampaignMode_UsesCreateCampaignMap()
    {
        // Arrange
        var entrance = new Room { Name = "Entrance" };
        var chamber = new Room { Name = "Chamber" };
        var rooms = new List<Room> { entrance, chamber };

        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(rooms);

        // Act
        var result = _mapFactory.GenerateMap(level: 1, campaign: true, randomMap: false);

        // Assert
        _roomDaoMock.Verify(d => d.GetAllRooms(), Times.Once);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GenerateMap_StandardMode_RemovesUnconnectedRooms()
    {
        // Arrange
        var entrance = new Room { Name = "Entrance" };
        var hall = new Room { Name = "Hall" };
        var rooms = new List<Room> { entrance, hall };

        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(rooms);

        // Act
        var result = _mapFactory.GenerateMap(level: 1, campaign: false, randomMap: false);

        // Assert
        _roomDaoMock.Verify(d => d.GetAllRooms(), Times.Once);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Entrance", result[0].Name);
    }

    [TestMethod]
    public void GenerateMap_MissingEntrance_Throws()
    {
        // Arrange
        var rooms = new List<Room> { new Room { Name = "SomethingElse" } };
        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(rooms);

        // Act
        Assert.ThrowsException<InvalidOperationException>(() =>
            _mapFactory.GenerateMap(level: 1, campaign: false, randomMap: false));
    }


    [TestMethod]
    public void GenerateMap_RandomMapMode_GeneratesMap()
    {
        // Arrange
        var entrance = new Room { Name = "Entrance", Id = 1 };
        var hall = new Room { Name = "Hall", Id = 2 };

        entrance.East = hall;
        hall.West = entrance;

        var rooms = new List<Room> { entrance, hall };
        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(rooms);

        // Act
        var result = _mapFactory.GenerateMap(
            level: 1,
            campaign: false,
            randomMap: true);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(r => r.Name == "Entrance"));
        Assert.IsTrue(result.Any(r => r.Name == "Hall"));
    }

    [TestMethod]
    public void GenerateMap_CampaignMode_ClonesRooms()
    {
        // Arrange
        var entrance = new Room { Name = "Entrance", Id = 1 };
        var hall = new Room { Name = "Hall", Id = 2 };

        entrance.East = hall;
        hall.West = entrance;

        var rooms = new List<Room> { entrance, hall };
        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(rooms);

        // Act
        var result = _mapFactory.GenerateMap(
            level: 1,
            campaign: true,
            randomMap: false);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsFalse(result.Contains(entrance));
        Assert.IsFalse(result.Contains(hall));
    }

    [TestMethod]
    public void GenerateMap_RandomMapMode_ClonesRooms()
    {
        // Arrange
        var entrance = new Room { Name = "Entrance", Id = 1 };
        var hall = new Room { Name = "Hall", Id = 2 };

        entrance.East = hall;
        hall.West = entrance;

        var rooms = new List<Room> { entrance, hall };
        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(rooms);

        // Act
        var result = _mapFactory.GenerateMap(
            level: 1,
            campaign: false,
            randomMap: true);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsFalse(result.Contains(entrance));
        Assert.IsFalse(result.Contains(hall));
    }

    [TestMethod]
    public void GenerateMap_StandardMode_PreservesOriginalRoomInstances()
    {
        // Arrange
        var entrance = new Room { Name = "Entrance", Id = 1 };
        var hall = new Room { Name = "Hall", Id = 2 };

        entrance.East = hall;
        hall.West = entrance;

        var rooms = new List<Room> { entrance, hall };
        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(rooms);

        // Act
        var result = _mapFactory.GenerateMap(
            level: 1,
            campaign: false,
            randomMap: false);

        // Assert
        Assert.AreSame(entrance, result[0]);
        Assert.AreSame(hall, result[1]);
    }

    [TestMethod]
    public void GenerateMap_CampaignMode_LimitsRoomCountBasedOnLevel()
    {
        // Arrange
        var entrance = new Room { Name = "Entrance", Id = 1 };
        var roomA = new Room { Name = "Room A", Id = 2 };
        var roomB = new Room { Name = "Room B", Id = 3 };
        var roomC = new Room { Name = "Room C", Id = 4 };
        var roomD = new Room { Name = "Room D", Id = 5 };

        var rooms = new List<Room>
        {
            entrance,
            roomA,
            roomB,
            roomC,
            roomD
        };

        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(rooms);

        // Act
        var result = _mapFactory.GenerateMap(
            level: 0,
            campaign: true,
            randomMap: false);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(r => r.Name == "Entrance"));
    }
}