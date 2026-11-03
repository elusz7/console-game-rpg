
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Models;
using ConsoleGame.Services;
using ConsoleGameEntities.Models.Entities;
using Moq;

namespace ConsoleGameTests.ConsoleGame.Services;

[TestClass]
public class CampaignPersistenceServiceTests
{
    // Mock the DAO so tests can verify persistence without a database.
    private readonly Mock<IPlayerDao> _playerDaoMock = new();

    // The service being tested.
    private readonly CampaignPersistenceService _service;

    public CampaignPersistenceServiceTests()
    {
        // PlayerHelper is real because its SavePlayer method is not virtual.
        // Its dependencies are mocked so the tests remain isolated.
        var playerHelper = new PlayerHelper(
            new Mock<IInputManager>().Object,
            new Mock<IOutputManager>().Object,
            _playerDaoMock.Object,
            new Mock<IArchetypeDao>().Object);

        _service = new CampaignPersistenceService(playerHelper);
    }

    // Creates an initialized player with the dependencies required by Initialize().
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

        // Initialize the player's health, inventory, and starting level.
        player.Initialize();

        return player;
    }

    [TestMethod]
    public void SaveIfCampaign_CampaignQuit_SavesPlayer()
    {
        // Arrange
        var player = CreatePlayer();
        var session = new GameSession(player, new Floor(), true);

        // Indicate that the player chose to quit the campaign.
        session.End(AdventureStatus.Quit);

        // Treat this as a new player who needs to be added.
        _playerDaoMock
            .Setup(dao => dao.GetPlayerByName(player.Name))
            .Returns((Player?)null);

        // Act
        _service.SaveIfCampaign(session);

        // Assert
        _playerDaoMock.Verify(
            dao => dao.GetPlayerByName(player.Name),
            Times.Once);

        _playerDaoMock.Verify(
            dao => dao.AddPlayer(player),
            Times.Once);
    }

    [TestMethod]
    public void SaveIfCampaign_CampaignPlayerDies_SavesPlayer()
    {
        // Arrange
        var player = CreatePlayer();
        var session = new GameSession(player, new Floor(), true);

        // Indicate that the player died during the campaign.
        session.End(AdventureStatus.PlayerDead);

        // Treat this as a new player who needs to be added.
        _playerDaoMock
            .Setup(dao => dao.GetPlayerByName(player.Name))
            .Returns((Player?)null);

        // Act
        _service.SaveIfCampaign(session);

        // Assert
        _playerDaoMock.Verify(
            dao => dao.AddPlayer(player),
            Times.Once);
    }

    [TestMethod]
    public void SaveIfCampaign_AdventureQuit_DoesNotSavePlayer()
    {
        // Arrange
        var player = CreatePlayer();
        var session = new GameSession(player, new Floor(), false);

        // Indicate that the player chose to quit the adventure.
        session.End(AdventureStatus.Quit);

        // Act
        _service.SaveIfCampaign(session);

        // Assert
        // A regular adventure should not access the persistence layer.
        _playerDaoMock.Verify(
            dao => dao.GetPlayerByName(It.IsAny<string>()),
            Times.Never);

        _playerDaoMock.Verify(
            dao => dao.AddPlayer(It.IsAny<Player>()),
            Times.Never);

        _playerDaoMock.Verify(
            dao => dao.UpdatePlayer(It.IsAny<Player>()),
            Times.Never);
    }

    [TestMethod]
    public void SaveIfCampaign_AdventurePlayerDies_DoesNotSavePlayer()
    {
        // Arrange
        var player = CreatePlayer();
        var session = new GameSession(player, new Floor(), false);

        // Indicate that the player died during the adventure.
        session.End(AdventureStatus.PlayerDead);

        // Act
        _service.SaveIfCampaign(session);

        // Assert
        // A regular adventure should never trigger a database operation.
        _playerDaoMock.Verify(
            dao => dao.GetPlayerByName(It.IsAny<string>()),
            Times.Never);

        _playerDaoMock.Verify(
            dao => dao.AddPlayer(It.IsAny<Player>()),
            Times.Never);

        _playerDaoMock.Verify(
            dao => dao.UpdatePlayer(It.IsAny<Player>()),
            Times.Never);
    }
}
