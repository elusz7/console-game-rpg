using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Entities;
[TestClass]
public class RoomTests
{
    [TestMethod]
    public void Clone_ReturnsCopyWithSameProperties()
    {
        var original = new Room("Hall", "A long, dark hallway") { Id = 1 };
        var clone = original.Clone();

        Assert.AreEqual(original.Id, clone.Id);
        Assert.AreEqual(original.Name, clone.Name);
        Assert.AreEqual(original.Description, clone.Description);
        Assert.AreNotSame(original, clone);
    }
    [TestMethod]
    public void GetConnections_ReturnsOnlyNonNullConnections()
    {
        var northRoom = new Room("North Room", "North description");
        var room = new Room("Center", "Center room") { North = northRoom };

        var connections = room.GetConnections();

        Assert.AreEqual(1, connections.Count);
        Assert.IsTrue(connections.ContainsKey("North"));
        Assert.AreSame(northRoom, connections["North"]);
    }
}
