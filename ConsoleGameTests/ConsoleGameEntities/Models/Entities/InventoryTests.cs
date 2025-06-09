using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Tests.Models.Entities;

[TestClass]
public class InventoryTests
{
    private static Inventory CreateInventory(int gold = 100, decimal capacity = 100.0M)
    {
        return new Inventory
        {
            Id = 1,
            Gold = gold,
            Capacity = capacity,
            Items = []
        };
    }
    private static Item CreateItem(decimal value = 42.0M, decimal weight = 10.2M)
    {
        return new Item
        {
            Id = 1,
            Name = "Sword",
            Value = value,
            Description = "A sharp blade.",
            Durability = 100,
            Weight = weight,
            RequiredLevel = 1
        };
    }
    private static Consumable CreateConsumable()
    {
        return new Consumable
        {
            Name = "Test Consumable",
            Value = 10,
            RequiredLevel = 1,
            Durability = 1
        };
    }
    private static Valuable CreateValuable()
    {
        return new Valuable
        {
            Name = "Test Valuable",
            Value = 1000,
            RequiredLevel = 1,
            Durability = 1
        };
    }

    [TestMethod]
    public void AddItem_AddsItemToInventory()
    {
        // Arrange
        var inventory = CreateInventory();
        var item = CreateItem();

        // Act
        inventory.AddItem(item);

        // Assert
        Assert.IsTrue(inventory.Items.Contains(item));
    }

    [TestMethod]
    public void AddItem_ThrowsException_WhenOverCapacity()
    {
        var inventory = CreateInventory(capacity: 5M);
        var item = CreateItem(weight: 10.6M);

        Assert.ThrowsException<OverweightException>(() => inventory.AddItem(item));
    }

    [TestMethod]
    public void RemoveItem_RemovesItemFromInventory()
    {
        // Arrange
        var inventory = CreateInventory();
        var item = CreateItem();
        inventory.Items.Add(item);

        // Act
        inventory.RemoveItem(item);

        // Assert
        Assert.IsFalse(inventory.Items.Contains(item));
    }

    [TestMethod]
    public void RemoveItem_ThrowsException_IfItemNotPresent()
    {
        // Arrange
        var inventory = CreateInventory();
        var item = CreateItem();

        // Act / Assert
        Assert.ThrowsException<ItemNotFoundException>(() => inventory.RemoveItem(item));
    }

    [TestMethod]
    public void AddConsumable()
    {
        //arrange
        var inventory = CreateInventory();
        var consumable = CreateConsumable();

        inventory.AddItem(consumable);

        Assert.IsTrue(inventory.ContainsConsumables());
    }

    [TestMethod]
    public void AddConsumable_CapacityIgnored()
    {
        var inventory = CreateInventory(capacity: 15);
        var item = CreateItem(weight: 15);
        var consumable = CreateConsumable();

        inventory.AddItem(item);
        inventory.AddItem(consumable);

        Assert.IsTrue(inventory.ContainsConsumables());
    }

    [TestMethod]
    public void AddValuable_CapacityIgnored()
    {
        var inventory = CreateInventory(capacity: 15);
        var item = CreateItem(weight: 15);
        var valuable = CreateValuable();

        inventory.AddItem(item);
        inventory.AddItem(valuable);

        Assert.IsTrue(inventory.Items.Contains(valuable));
    }
    [TestMethod]
    public void Buy_Success()
    {
        var inventory = CreateInventory();
        var item = CreateItem(value: 10);
        int expectedGold = inventory.Gold - (int)Math.Round(item.GetBuyPrice());

        inventory.Buy(item);

        Assert.AreEqual(expectedGold, inventory.Gold);
        Assert.IsTrue(inventory.Items.Contains(item));
    }
    [TestMethod]
    public void Buy_ThrowsException_NotEnoughGold()
    {
        var inventory = CreateInventory(gold: 10);
        var item = CreateItem(value: 63.0M);

        var ex = Assert.ThrowsException<ItemPurchaseException>(() => inventory.Buy(item));

        StringAssert.Contains(ex.Message, "short by");
    }

    [TestMethod]
    public void Buy_ThrowsException_NotEnoughCapacity()
    {
        var inventory = CreateInventory();
        var item = CreateItem(weight: 250M);

        var ex = Assert.ThrowsException<ItemPurchaseException>(() => inventory.Buy(item));

        StringAssert.Contains(ex.Message, "Will exceed carrying capacity");
    }
    [TestMethod]
    public void Sell_Success()
    {
        var inventory = CreateInventory();
        var item = CreateValuable();

        inventory.AddItem(item);

        var expectedGold = inventory.Gold + item.Value;

        inventory.Sell(item);

        Assert.AreEqual(expectedGold, inventory.Gold);
        Assert.IsFalse(inventory.Items.Contains(item));
    }
}