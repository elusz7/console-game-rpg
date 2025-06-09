using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Items;


namespace ConsoleGameTests.ConsoleGameEntities.Models.Items;

[TestClass]
public class ItemTests
{
    [TestMethod]
    public void Use_DecrementsDurability()
    {
        var item = new Item
        {
            Name = "Test Sword",
            Durability = 5
        };

        item.Use();

        Assert.AreEqual(4, item.Durability);
    }

    [TestMethod]
    public void Use_Throws_When_Durability_Zero()
    {
        var item = new Item
        {
            Name = "Broken Sword",
            Durability = 0
        };

        Assert.ThrowsException<ItemDurabilityException>(() => item.Use());
    }

    [TestMethod]
    public void Use_MaxDurablity()
    {
        var item = new Item
        {
            Durability = 98
        };

        item.RecoverDurability(6);

        Assert.AreEqual(100, item.Durability);
    }
}
