using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleGameTests.ConsoleGame.Factories;

using System;
using System.Collections.Generic;
using System.Linq;
using global::ConsoleGame.Factories;
using global::ConsoleGame.GameDao.Interfaces;
using global::ConsoleGame.Helpers.Interfaces;
using global::ConsoleGameEntities.Models.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class MapFactoryTests
{
    private Mock<IRoomDao> _roomDaoMock;
    private Mock<IMapHelper> _mapHelperMock;
    private MapFactory _mapFactory;

    [TestInitialize]
    public void Setup()
    {
        _roomDaoMock = new Mock<IRoomDao>();
        _mapHelperMock = new Mock<IMapHelper>();
        _mapFactory = new MapFactory(_roomDaoMock.Object, _mapHelperMock.Object);

        var entrance = new Room { Name = "Entrance" };
        var hall = new Room { Name = "Hall" };
        var clonedRooms = new List<Room> { entrance, hall };

        // Simulate cloning behavior (if needed)
        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(clonedRooms);
        _mapHelperMock.Setup(h => h.CreateCampaignMap(It.IsAny<int>(), It.IsAny<Room>(), It.IsAny<List<Room>>()))
                      .Returns(clonedRooms);

        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(clonedRooms);

        _mapHelperMock.Setup(h => h.FindUnconnectedRooms(It.IsAny<Room>(), It.IsAny<List<Room>>()))
                      .Returns([hall]);
    }

    [TestMethod]
    public void GenerateMap_CampaignMode_UsesCreateCampaignMap()
    {
        // Arrange
        var entrance = new Room { Name = "Entrance" };
        var chamber = new Room { Name = "Chamber" };
        var rooms = new List<Room> { entrance, chamber };

        _roomDaoMock.Setup(d => d.GetAllRooms()).Returns(rooms);
        _mapHelperMock.Setup(h => h.CreateCampaignMap(It.IsAny<int>(), It.Is<Room>(r => r.Name == "Entrance"), It.IsAny<List<Room>>()))
                      .Returns(rooms);

        // Act
        var result = _mapFactory.GenerateMap(level: 1, campaign: true, randomMap: false);

        // Assert
        _roomDaoMock.Verify(d => d.GetAllRooms(), Times.Once);
        _mapHelperMock.Verify(h => h.CreateCampaignMap(
            It.IsAny<int>(),
            It.Is<Room>(r => r.Name == "Entrance"),
            It.IsAny<List<Room>>()), Times.Once);

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
        _mapHelperMock.Setup(h => h.FindUnconnectedRooms(entrance, rooms)).Returns(new List<Room> { hall });

        // Act
        var result = _mapFactory.GenerateMap(level: 1, campaign: false, randomMap: false);

        // Assert
        _roomDaoMock.Verify(d => d.GetAllRooms(), Times.Once);
        _mapHelperMock.Verify(h => h.FindUnconnectedRooms(
            It.Is<Room>(r => r.Name == "Entrance"),
            It.IsAny<List<Room>>()), Times.Once);
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
        Assert.ThrowsException<InvalidOperationException>(() =>_mapFactory.GenerateMap(level: 1, campaign: false, randomMap: false));
    }
}
