using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.CrudHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Entities;
using Moq;

namespace ConsoleGameTests.ConsoleGame.Helpers.CrudHelpers;

[TestClass]
public class ArchetypeManagementTests
{
    private readonly Mock<IInputManager> _inputManagerMock;
    private readonly Mock<IOutputManager> _outputManagerMock;
    private readonly Mock<IArchetypeDao> _archetypeDaoMock;
    private readonly ArchetypeManagement _archetypeManagement;

    public ArchetypeManagementTests()
    {
        _inputManagerMock = new Mock<IInputManager>();
        _outputManagerMock = new Mock<IOutputManager>();
        _archetypeDaoMock = new Mock<IArchetypeDao>();
        _archetypeManagement = new ArchetypeManagement(
            _inputManagerMock.Object,
            _outputManagerMock.Object,
            _archetypeDaoMock.Object
        );
    }

    [TestMethod]
    public void AddArchetype_ShouldAddArchetype_WhenInputIsValid()
    {
        // Arrange
        _inputManagerMock.SetupSequence(m => m.ReadInt(It.IsAny<string>(), null, false))
            .Returns(1) // Martial
            .Returns(1) // Stat priorities
            .Returns(2)
            .Returns(3)
            .Returns(4)
            .Returns(5)
            .Returns(6)
            .Returns(10) // Damage bonus
            .Returns(100) // Health
            .Returns(5) // Defense
            .Returns(3) // Resistance
            .Returns(7) // Speed
            .Returns(20); // Max Resource

        _inputManagerMock.Setup(m => m.ReadString(It.IsAny<string>(), null))
            .Returns("Test");

        _inputManagerMock.Setup(m => m.LoopAgain("create")).Returns(false);

        _archetypeDaoMock.Setup(m => m.AddArchetype(It.IsAny<Archetype>()));

        // Act
        var addArchetypeMethod = typeof(ArchetypeManagement)
            .GetMethod("AddArchetype", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        addArchetypeMethod.Invoke(_archetypeManagement, null);

        // Assert
        _archetypeDaoMock.Verify(m => m.AddArchetype(It.IsAny<Archetype>()), Times.Once);
        _outputManagerMock.Verify(m => m.WriteLine(It.Is<string>(s => s.Contains("created successfully")), ConsoleColor.Green), Times.Once);
    }

    [TestMethod]
    public void EditArchetype_ShouldNotEdit_WhenNoArchetypesExist()
    {
        // Arrange
        _archetypeDaoMock.Setup(m => m.GetAllNonCoreArchetypes()).Returns(new List<Archetype>());

        // Act
        var editArchetypeMethod = typeof(ArchetypeManagement)
            .GetMethod("EditArchetype", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        editArchetypeMethod.Invoke(_archetypeManagement, null);

        // Assert
        _outputManagerMock.Verify(m => m.WriteLine("No archetypes available to edit.", ConsoleColor.Red), Times.Once);
    }

    [TestMethod]
    public void DeleteArchetype_ShouldNotDelete_WhenNoArchetypesExist()
    {
        // Arrange
        _archetypeDaoMock.Setup(m => m.GetAllNonCoreArchetypes()).Returns(new List<Archetype>());

        // Act
        var deleteArchetypeMethod = typeof(ArchetypeManagement)
            .GetMethod("DeleteArchetype", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        deleteArchetypeMethod.Invoke(_archetypeManagement, null);

        // Assert
        _outputManagerMock.Verify(m => m.WriteLine("No archetypes available to delete.", ConsoleColor.Red), Times.Once);
    }
}